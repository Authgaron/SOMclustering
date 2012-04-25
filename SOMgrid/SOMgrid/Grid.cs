using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Diagnostics;

namespace SOMgrid
{
    public class Grid
    {
        #region Class Variables
        int radius = 15;
        List<List<float>> centres = new List<List<float>>();
        List<List<int>> adjacent = new List<List<int>>();
        Color color = new Color(96,0,0);
        Color linecolor = new Color(0,0,48);
        Vector2 topleft;
        Vector2 rdimensions;
        int gdimensions;
        int gridsize;
        List<List<int>> distances = new List<List<int>>();
        public List<List<Thread>> subthreads = new List<List<Thread>>();
        public Timer t = new Timer(new TimerCallback(delegate (object item) { }));
        int counter = 0;
        public static bool created = false;
        #endregion

        public Grid(int numpoints, int dimensions, Rectangle area)
        {
            topleft = new Vector2(area.X, area.Y);
            rdimensions = new Vector2(area.Width, area.Height);
            gdimensions = dimensions;
            gridsize = numpoints;
            if (numpoints <= 1)
            {
                return;
            }
            Main.log.addLog("Adding first dimension.");
            centres.Add(new List<float>() { 0 });
            adjacent.Add(new List<int>());
            for (int i = 1; i < numpoints; i++)
            {
                centres.Add(new List<float>() { i / (float)(numpoints - 1)});
                adjacent.Add(new List<int>());
                adjacent[i].Add(i - 1);
                adjacent[i - 1].Add(i);
            }

            Main.log.addLog("Adding higer dimensions.");
            for (int d = 1; d < dimensions; d++)
            {
                Main.log.addLog("Dimension: " + (d+1).ToString());
                for (int i = 0; i < centres.Count; i++)
                {
                    centres[i].Add(0);
                }
                int initpoints = centres.Count;
                for (int j = 1; j < numpoints; j++)
                {
                    for (int i = 0; i < initpoints; i++)
                    {
                        List<float> temp = centres[i].ToList();
                        temp[temp.Count - 1] = j / (float)(numpoints - 1);
                        centres.Add(temp);
                        adjacent.Add(adjacent[i].ConvertAll( new Converter<int,int>(item => item + j * initpoints)));
                        adjacent.Last().Add(initpoints * (j - 1) + i);
                    }
                }
            }
            for (int i = 0; i < adjacent.Count; i++)
            {
                for (int j = 0; j < adjacent[i].Count; j++)
                {
                    if (!adjacent[adjacent[i][j]].Contains(i))
                    {
                        adjacent[adjacent[i][j]].Add(i);
                    }
                }
                List<int> temp = new List<int>();
            }

            for (int i = 0; i < centres.Count; i++)
            {
                for (int j = 0; j < centres[i].Count; j++)
                {
                    centres[i][j] = centres[i][j] * 2 - 1;
                }
            }

            Main.log.addLog("Points: " + centres.Count);
            Main.log.addLog("Generating adjacency matrix (path of length 1 or 0)...");

            List<List<int>> basemat = distances.ToList();
            for (int i = 0; i < centres.Count; i++)
            {
                distances.Add(new List<int>());
                basemat.Add(new List<int>());
                for (int j = 0; j < centres.Count; j++)
                {
                    distances[i].Add(i == j ? 0 : adjacent[i].Contains(j) ? 1 : int.MaxValue);
                    basemat[i].Add(adjacent[i].Contains(j) ? 1 : 0);
                }
            }
            bool finished = false;
            int power = 2;
            
            List<List<int>> lastmat = basemat.ToList();
            while (!finished)
            {
                Main.log.addLog("Calculating number of paths with a length of " + power.ToString() + ".");
                finished = true;

                Stopwatch s = new Stopwatch();
                s.Start();
                lastmat = strassen(basemat, lastmat);
                s.Stop();
                Main.log.addLog("Time taken to run Strassen's algorithm - " + s.ElapsedMilliseconds.ToString() + "ms");
                t.Dispose();
                for (int i = 0; i < distances.Count; i++)
                {
                    for (int j = 0; j < distances.Count; j++)
                    {
                        distances[i][j] = (new List<int>() { lastmat[i][j] > 0 ? power : int.MaxValue, distances[i][j] }).Min();
                        if (distances[i][j] == int.MaxValue)
                        {
                            finished = false;
                        }
                    }
                }
                power++;
            }
            Main.log.addLog("\nGrid created.\n\tDimensions = " + dimensions.ToString() + "\n\tPoints = " + centres.Count.ToString() + "\n");
        }

        #region Strassen's Algorithm
        public List<List<List<int>>> split(List<List<int>> initial)
        {
            List<List<List<int>>> output = new List<List<List<int>>>();
            output.Add(new List<List<int>>());
            output.Add(new List<List<int>>());
            output.Add(new List<List<int>>());
            output.Add(new List<List<int>>());
            for (int k = 0; k < output.Count; k++)
            {
                for (int i = 0; i < initial.Count / 2; i++)
                {
                    output[k].Add(new List<int>());
                }
            }
            for (int i = 0; i < initial.Count; i++)
            {
                for (int j = 0; j < initial[i].Count; j++)
                {
                    int index = 0;
                    index += i >= initial.Count / 2 ? 1 : 0;
                    index += j >= initial[i].Count / 2 ? 2 : 0;
                    output[index][i - (i >= initial.Count / 2 ? initial.Count / 2 : 0)].Add(initial[i][j]);
                }
            }
            return output;
        }

        public List<List<int>> madd(List<List<int>> a, List<List<int>> b)
        {
            List<List<int>> output = new List<List<int>>();
            for (int i = 0; i < a.Count; i++)
            {
                output.Add(new List<int>());
                for (int j = 0; j < a[i].Count; j++)
                {
                    output[i].Add(a[i][j] + b[i][j]);
                }
            }
            return output;
        }

        public List<List<int>> msub(List<List<int>> a, List<List<int>> b)
        {
            List<List<int>> output = new List<List<int>>();
            for (int i = 0; i < a.Count; i++)
            {
                output.Add(new List<int>());
                for (int j = 0; j < a[i].Count; j++)
                {
                    output[i].Add(a[i][j] - b[i][j]);
                }
            }
            return output;
        }

        public List<List<List<int>>> getS(List<List<List<int>>> A, List<List<List<int>>> B)
        {
            List<List<List<int>>> S = new List<List<List<int>>>();
            S.Add(msub(B[1], B[3])); // S1
            S.Add(madd(A[0], A[1])); // S2
            S.Add(madd(A[2], A[3])); // S3
            S.Add(msub(B[2], B[0])); // S4
            S.Add(madd(A[0], A[3])); // S5
            S.Add(madd(B[0], B[3])); // S6
            S.Add(msub(A[1], A[3])); // S7
            S.Add(madd(B[2], B[3])); // S8
            S.Add(msub(A[0], A[2])); // S9
            S.Add(madd(B[0], B[1])); // S10
            return S;
        }

        public List<List<List<int>>> getP(List<List<List<int>>> A, List<List<List<int>>> B, List<List<List<int>>> S)
        {
            List<List<List<int>>> P = new List<List<List<int>>>();
            for (int i = 0; i < 7; i++)
            {
                P.Add(new List<List<int>>());
            }
            int index = counter;
            subthreads.Add(new List<Thread>());
            counter++;
            subthreads[index].Add(new Thread(new ThreadStart(delegate() { P[0] = strassen(A[0], S[0]);}))); // P1
            subthreads[index].Add(new Thread(new ThreadStart(delegate() { P[1] = strassen(S[1], B[3]);}))); // P2
            subthreads[index].Add(new Thread(new ThreadStart(delegate() { P[2] = strassen(S[2], B[0]);}))); // P3
            subthreads[index].Add(new Thread(new ThreadStart(delegate() { P[3] = strassen(A[3], S[3]);}))); // P4
            subthreads[index].Add(new Thread(new ThreadStart(delegate() { P[4] = strassen(S[4], S[5]);}))); // P5
            subthreads[index].Add(new Thread(new ThreadStart(delegate() { P[5] = strassen(S[6], S[7]);}))); // P6
            subthreads[index].Add(new Thread(new ThreadStart(delegate() { P[6] = strassen(S[8], S[9]);}))); // P7
            for (int i = 0; i < subthreads[index].Count; i++)
            {
                subthreads[index][i].Start();
            }
            for (int i = 0; i < subthreads[index].Count; i++)
            {
                subthreads[index][i].Join();
            }
            return P;
        }

        public List<List<int>> combine(List<List<int>> c11, List<List<int>> c12, List<List<int>> c21, List<List<int>> c22)
        {
            List<List<int>> C = new List<List<int>>();
            for (int i = 0; i < c11.Count; i++)
            {
                C.Add(new List<int>());
                C[i].AddRange(c11[i]);
                C[i].AddRange(c12[i]);
            }
            for (int i = 0; i < c21.Count; i++)
            {
                C.Add(new List<int>());
                C[i + c11.Count].AddRange(c21[i]);
                C[i + c11.Count].AddRange(c22[i]);
            }
            return C;
        }

        public List<List<int>> strassen(List<List<int>> a, List<List<int>> b)
        {
            if (a.Count <= 128)
            {
                return mmult(a,b);
            }
            else
            {
                List<List<int>> C = new List<List<int>>();
                List<List<List<int>>> A = split(a);
                List<List<List<int>>> B = split(b);
                List<List<List<int>>> S = getS(A, B);
                List<List<List<int>>> P = getP(A, B, S);

                List<List<int>> C11 = madd(msub(madd(P[4], P[3]), P[1]), P[5]);
                List<List<int>> C12 = madd(P[0], P[1]);
                List<List<int>> C21 = madd(P[2], P[3]);
                List<List<int>> C22 = msub(msub(madd(P[4], P[0]), P[2]), P[6]);

                C = combine(C11,C12,C21,C22);
                return C;
            }
        }

        public List<List<int>> mmult(List<List<int>> a, List<List<int>> b)
        {
            List<List<int>> output = new List<List<int>>();

            List<List<int>> bt = new List<List<int>>();
            for (int i = 0; i < b[0].Count; i++)
            {
                bt.Add(new List<int>());
                for (int j = 0; j < b.Count; j++)
                {
                    bt[i].Add(b[j][i]);
                }
            }

            for (int i = 0; i < a.Count; i++)
            {
                output.Add(new List<int>());
                for (int j = 0; j < bt.Count; j++)
                {
                    int sum = 0;
                    for (int k = 0; k < a[i].Count; k++)
                    {
                        sum += a[i][k] * bt[j][k];
                    }
                    output[i].Add(sum);
                }
            }
            return output;
        }
        #endregion

        public Grid Reset()
        {
            Grid newgrid = new Grid(gridsize, gdimensions, new Rectangle((int)topleft.X, (int)topleft.Y, (int)rdimensions.X, (int)rdimensions.Y));
            return newgrid;
        }

        public static float DistanceSqrd(List<float> a, List<float> b)
        {
            float sum = 0;
            for (int i = 0; i < a.Count; i++)
            {
                sum += (a[i] - b[i]) * (a[i] - b[i]);
            }
            return sum;
        }

        public float Neighbourhood(int distance, int runs)
        {
            return 1.0f / (distance * (runs+1));
        }

        public float Eta(int n = 0)
        {
            return (float)Math.Exp(-n);
        }

        public void SOM()
        {
            for (int n = 0; n < 10; n++)
            {
                Main.log.addLog("Learning iteration: " + n.ToString());
                int count = 0;
                List<List<int>> Nb = new List<List<int>>();
                for (int i = 0; i < centres.Count; i++)
                {
                    Nb.Add(new List<int>());
                }
                t = new Timer(new TimerCallback(delegate(object item) { Main.log.addLog("Points processed: " + count.ToString()); }), null, 0, 1000);

                for (int i = 0; i < GetData.inputs.Count; i++)
                {
                    int minindex = 0;
                    for (int j = 1; j < centres.Count; j++)
                    {
                        if (DistanceSqrd(centres[j], GetData.inputs[i]) < DistanceSqrd(centres[minindex], GetData.inputs[i]))
                        {
                            minindex = j;
                        }
                    }
                    Nb[minindex].Add(i);
                    count = i;
                }
                t.Dispose();

                Main.log.addLog("Moving nodes to cluster centres.");

                count = 0;
                t = new Timer(new TimerCallback(delegate(object item) { Main.log.addLog("Moving centre number: " + count.ToString()); }), null, 0, 1000);
                for (int i = 0; i < Nb.Count; i++)
                {
                    for (int j = 0; j < Nb[i].Count; j++)
                    {
                        for (int k = 0; k < centres[i].Count; k++)
                        {
                            centres[i][k] += Eta(n) * (GetData.inputs[Nb[i][j]][k] - centres[i][k]);
                        }
                        for (int l = 0; l < centres.Count; l++)
                        {
                            for (int k = 0; k < centres[i].Count; k++)
                            {
                                if (Nb[l].Count == 0)
                                {
                                    centres[l][k] += Neighbourhood(distances[i][l], 0) * Eta(n) * (GetData.inputs[Nb[i][j]][k] - centres[l][k]);
                                }
                            }
                        }
                    }
                    count = i;
                }
                t.Dispose();
            }
        }

        public void Draw(SpriteBatch batch, List<int> axes)
        {
            try
            {
                if (centres[0].Count > axes.Max())
                {
                    for (int i = 0; i < adjacent.Count; i++)
                    {
                        for (int j = 0; j < adjacent[i].Count; j++)
                        {
                            try
                            {
                                Vector2 start = new Vector2(topleft.X + (centres[i][axes[0]] + 1) / 2 * rdimensions.X, topleft.Y + (-centres[i][axes[1]] + 1) / 2 * rdimensions.Y);
                                Vector2 end = new Vector2(topleft.X + (centres[adjacent[i][j]][axes[0]] + 1) / 2 * rdimensions.X, topleft.Y + (-centres[adjacent[i][j]][axes[1]] + 1) / 2 * rdimensions.Y);
                                Primitives.Instance.drawLine(batch, start, end, linecolor, 1); 
                            }
                            catch (Exception e)
                            { }
                        }
                    }
                    for (int i = 0; i < centres.Count; i++)
                    {
                        float x = topleft.X + (centres[i][axes[0]] + 1) / 2 * rdimensions.X;
                        float y = topleft.Y + (-centres[i][axes[1]] + 1) / 2 * rdimensions.Y; ;
                        Vector2 pos = new Vector2(x, y);
                        Primitives.Instance.drawCircle(batch, pos, radius, color, 1);
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
            }
        }
    }
}
