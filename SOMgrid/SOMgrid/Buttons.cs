using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace SOMgrid
{
    class Buttons
    {
        public static List<int> axes = new List<int>() { 0, 1 };
        List<Rectangle> buttonouter = new List<Rectangle>();
        List<Rectangle> buttoninner = new List<Rectangle>();
        public List<string> buttontext = new List<string>();
        List<Color> buttoncolor = new List<Color>() { Color.CornflowerBlue, Color.Lime, Color.Red };
        bool clicked = false;
        bool learnt = false;
        bool learning = false;
        public Thread t = new Thread(delegate() { });

        public Buttons(int dimensions, Rectangle area)
        {
            for (int i = 0; i < dimensions; i++)
            {
                int x = area.X + 5 + i / ((area.Height - 100) / 50) * 45;
                int y = area.Y + 10 + (i % ((area.Height - 100) / 50)) * 45;
                Rectangle outer = new Rectangle(x, y, 40,40);
                Rectangle inner = new Rectangle(x + 2, y + 2, 36, 36);
                buttonouter.Add(outer);
                buttoninner.Add(inner);
                buttontext.Add(i.ToString());
            }
            Rectangle goouter = new Rectangle(area.X + 5, area.Y + 10 + area.Height - 100, area.Width - 15, 80);
            Rectangle goinner = new Rectangle(area.X + 7, area.Y + 12 + area.Height - 100, area.Width - 19, 76);
            buttonouter.Add(goouter);
            buttoninner.Add(goinner);
            buttontext.Add("Learn");
        }

        public void Update()
        {
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && !clicked)
            {
                for (int i = 0; i < buttonouter.Count - 1; i++)
                {
                    if (buttonouter[i].Contains(mouse.X, mouse.Y))
                    {
                        clicked = true;

                        if (axes.Contains(i))
                        {
                            axes.Reverse();
                            return;
                        }
                        else
                        {
                            //axes[0] = axes[1];
                            axes[1] = i;
                        }
                    }
                }
                if (buttonouter.Last().Contains(mouse.X, mouse.Y))
                {
                    clicked = true;

                    if (!learning)
                    {
                        if (!learnt)
                        {
                            learning = true;
                            buttontext[buttontext.Count - 1] = "Learning";
                            t = new Thread(delegate() { Main.Instance.grid.SOM(); learning = false; learnt = true; buttontext[buttontext.Count - 1] = "Unlearn"; });
                            t.Start();
                        }
                        else
                        {
                            learning = true;
                            buttontext[buttontext.Count - 1] = "Unlearning";
                            t = new Thread(delegate() { Main.Instance.grid = Main.Instance.grid.Reset(); learning = false; learnt = false; buttontext[buttontext.Count - 1] = "Learn"; });
                            t.Start();
                        }
                    }
                }
            }
            else if (mouse.LeftButton == ButtonState.Released)
            {
                clicked = false;
            }
        }

        public void Draw(SpriteBatch batch)
        {
            for (int i = 0; i < buttoninner.Count; i++)
            {
                Color c;
                if (i == buttoninner.Count - 1)
                {
                    c = buttoncolor[learning ? 0 : learnt ? 2 : 1];
                }
                else
                {
                    c = buttoncolor[axes.FindIndex(item => item == i) + 1];
                }

                Primitives.Instance.drawBoxFilled(batch, buttonouter[i], c);
                Primitives.Instance.drawBoxFilled(batch, buttoninner[i], Color.Black);
                float x = buttoninner[i].X + (buttoninner[i].Width - Main.Instance.logfont.MeasureString(buttontext[i]).X) / 2;
                float y = buttoninner[i].Y + (buttoninner[i].Height - Main.Instance.logfont.MeasureString(buttontext[i]).Y) / 2;;
                batch.DrawString(Main.Instance.logfont,buttontext[i], new Vector2(x,y),c);
            }

        }
    }
}
