using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SOMgrid
{
    public class Log
    {

        Color c;
        Color bg;
        SpriteFont f;
        Vector2 topLeft;
        Vector2 dimensions;
        float scrollval = 0;
        List<string> logs = new List<string>();
        int currindex = 0;

        public Log(Rectangle area, SpriteFont font, Color color, Color background)
        {

            bg = background;
            c = color;
            f = font;

            topLeft = new Vector2(area.X, area.Y);
            dimensions = new Vector2(area.Width, area.Height);

        }

        public void addLog(String s)
        {
            if (!s.Contains("\n"))
            {
                List<String> temp = new List<String>();
                int i = 0;
                while (i < s.Length)
                {
                    int width = (int)dimensions.X - 8;
                    String sub = "";
                    while (f.MeasureString(sub).X < width && i < s.Length)
                    {
                        sub = sub + s[i];
                        i++;
                    }
                    while (i < s.Length - 1 && s[i] != ' ')
                    {
                        if (width == 0)
                        {
                            return;
                        }
                        if (sub.Length >= 1)
                        {
                            sub = sub.Substring(0, sub.Length - 1);
                        }
                        else
                        {
                            break;
                        }
                        i--;
                    }
                    i++;
                    temp.Insert(0, sub);
                }
                temp.Reverse();
                for (int k = 0; k < temp.Count; k++)
                {
                    logs.Add(temp[k]);
                }

                int numlines = (int)((dimensions.Y - 6) / f.MeasureString("jl").Y) - 2;
                currindex = logs.Count - numlines >= 0 ? logs.Count - numlines : 0;
            }
            else
            {
                List<string> temp = s.Split(new char[] {'\n'}).ToList();
                temp.ForEach(item => { if (item != "") { addLog(item); } else { addLog("\t"); } });
            }
        }

        public void scroll(float scrolldiff, int X)
        {
            int x = (int)topLeft.X;
            int width = (int)dimensions.X;
            if (X > x && X < x + width)
            {
                currindex = Math.Max(0, Math.Min(logs.Count, (int)(currindex + scrolldiff / 30)));
            }
            if (currindex >= logs.Count)
            {
                currindex = logs.Count - 3;
            }
        }

        public void Update(MouseState m)
        {
            float diff = scrollval - m.ScrollWheelValue;
            
            int X = (int)(m.X);
            scroll(diff, X);
            scrollval = m.ScrollWheelValue;
        }

        public void Draw(SpriteBatch batch)
        {
            if (dimensions.X > 0 && logs.Count > 0)
            {
                Primitives.Instance.drawBoxFilled(batch, topLeft, topLeft+dimensions, bg);
                int currtop = 3;
                for (int k = currindex; k < logs.Count; k++)
                {
                    String s = logs[k];
                    int height = (int)(f.MeasureString(s).Y);
                    if (currtop + height <= topLeft.Y)
                    {
                        return;
                    }
                    batch.DrawString(f, s, new Vector2(topLeft.X + 3, currtop), c);
                    currtop += height;
                }
            }
        }

        public void Clear()
        {
            logs.Clear();
            currindex = 0;
        }
    }
}
