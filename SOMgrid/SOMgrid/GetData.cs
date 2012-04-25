using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SOMgrid
{
    class GetData
    {
        public static List<List<float>> inputs = new List<List<float>>();
        public static List<float> outputs = new List<float>();
        public static List<List<float>> testinputs = new List<List<float>>();
        public static List<float> testoutputs = new List<float>();
        public static StreamReader trainfile = new StreamReader(".\\shuttle.trn");
        public static StreamReader testfile = new StreamReader(".\\shuttle.tst");
        public static List<Color> colors = new List<Color>() { Color.Red, Color.Green, Color.CornflowerBlue, Color.Yellow, Color.Purple, Color.Blue, Color.Brown};

        static GetData() { }

        public static void readFiles()
        {
            while (trainfile.Peek() != -1)
            {
                List<float> line = trainfile.ReadLine().Split(new char[] { ' ' }).ToList().ConvertAll<float>(new Converter<string, float>(item => float.Parse(item)));
                outputs.Add(line.Last());
                line.RemoveAt(line.Count - 1);
                inputs.Add(line);
            }
            while (testfile.Peek() != -1)
            {
                List<float> line = testfile.ReadLine().Split(new char[] { ' ' }).ToList().ConvertAll<float>(new Converter<string, float>(item => float.Parse(item)));
                testoutputs.Add(line.Last());
                line.RemoveAt(line.Count - 1);
                testinputs.Add(line);
            }
            trainfile.Close();
            testfile.Close();
            //RemoveOutliers();
            Normalize();
        }

        public static void Normalize()
        {
            List<List<float>> temp = new List<List<float>>();
            for (int i = 0; i < inputs[0].Count; i++)
            {
                temp.Add(new List<float>());
                for (int j = 0; j < inputs.Count; j++)
                {
                    temp[i].Add(inputs[j][i]);
                }
            }
            foreach (List<float> i in temp)
            {
                float min = i.Min();
                for (int j = 0; j < i.Count; j++)
                {
                    i[j] = i[j] - min;
                }
                float max = i.Max();
                for (int j = 0; j < i.Count; j++)
                {
                    i[j] = i[j] / max * 2 - 1;
                }
            }
            inputs.Clear();
            for (int i = 0; i < temp[0].Count; i++)
            {
                inputs.Add(new List<float>());
                for (int j = 0; j < temp.Count; j++)
                {
                    inputs[i].Add(temp[j][i]);
                }
            }

            temp = new List<List<float>>();
            for (int i = 0; i < testinputs[0].Count; i++)
            {
                temp.Add(new List<float>());
                for (int j = 0; j < testinputs.Count; j++)
                {
                    temp[i].Add(testinputs[j][i]);
                }
            }
            foreach (List<float> i in temp)
            {
                float min = i.Min();
                for (int j = 0; j < i.Count; j++)
                {
                    i[j] = i[j] - min;
                }
                float max = i.Max();
                for (int j = 0; j < i.Count; j++)
                {
                    i[j] = i[j] / max * 2 - 1;
                }
            }
            testinputs.Clear();
            for (int i = 0; i < temp[0].Count; i++)
            {
                testinputs.Add(new List<float>());
                for (int j = 0; j < temp.Count; j++)
                {
                    testinputs[i].Add(temp[j][i]);
                }
            }
        }

        public static void RemoveOutliers()
        {
            float stddev = 0;
            float mean = 0;

            float sum = 0;
            List<float> sumpoints = new List<float>();
            List<float> meanpoint = new List<float>();
            for (int i = 0; i < inputs[0].Count; i++)
            {
                sumpoints.Add(0);
                meanpoint.Add(0);
            }
            for (int i = 0; i < inputs.Count; i++ )
            {
                for (int j = 0; j < inputs[i].Count; j++)
                {
                    sumpoints[j] += inputs[i][j];
                }
            }
            for (int i = 0; i < sumpoints.Count; i++)
            {
                meanpoint[i] = sumpoints[i] / inputs.Count;
            }

            for (int i = 0; i < inputs.Count; i++)
            {
                sum += (float)Math.Sqrt(Grid.DistanceSqrd(meanpoint, inputs[i]));
            }
            mean = sum / inputs.Count;
            sum = 0;
            for (int i = 0; i < inputs.Count; i++)
            {
                sum += (float)Math.Pow((float)Math.Sqrt(Grid.DistanceSqrd(meanpoint,inputs[i])) - mean, 2);
            }
            stddev = (float)Math.Sqrt(sum / inputs.Count);

            List<int> removable = new List<int>();
            for (int i = 0; i < inputs.Count; i++)
            { 
                if (Math.Sqrt(Grid.DistanceSqrd(meanpoint, inputs[i])) > mean + 3 * stddev)
                {
                    removable.Add(i);
                }
            }
            for (int i = 0; i < removable.Count; i++)
            {
                inputs.RemoveAt(removable[i]);
            }
        }

        public static void Draw(SpriteBatch batch, Rectangle area)
        {
            for (int i = 0; i < inputs.Count; i++ )
            {
                Primitives.Instance.drawPixel(batch, (int)(area.X + (inputs[i][Buttons.axes[0]] + 1) / 2 * area.Width), (int)(area.Y + (-inputs[i][Buttons.axes[1]] + 1) / 2 * area.Height), colors[(int)outputs[i] - 1]);
            }
        }
    }
}
