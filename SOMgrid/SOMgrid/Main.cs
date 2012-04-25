using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace SOMgrid
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static Main Instance;
        public Grid grid;
        public SpriteFont logfont;
        Buttons buttons;
        Thread t = new Thread(delegate() { });
        Thread g = new Thread(delegate() { });
        public static MouseState lastmouse;
        public static Log log;

        public Main()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = 500;
            graphics.PreferredBackBufferWidth = 900;
            graphics.ApplyChanges();
            IsMouseVisible = true;

            base.Initialize();

            // TODO: Add your initialization logic here
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            logfont = Content.Load<SpriteFont>("logfont");
            // TODO: use this.Content to load your game content here

            log = new Log(new Rectangle(600,0,300,500), logfont, Color.Green, Color.Black);
            GetData.readFiles();
            int dimensions = GetData.inputs[0].Count;
            int numpoints = 2;

            g = new Thread(delegate() { grid = new Grid(numpoints, dimensions, new Rectangle(50, 50, 400, 400)); });
            g.Start();
            buttons = new Buttons(dimensions, new Rectangle(500, 0, 100, 500));
            t = new Thread(delegate() { while (true) { buttons.Update(); } });
            t.Start();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                log.addLog("Killing all outstanding threads.");
                try
                {
                    grid.subthreads.ForEach(delegate(List<Thread> item) { item.ForEach(delegate(Thread subt) { subt.Abort(); }); });
                    grid.t.Dispose();
                }
                catch (Exception)
                { 
                }
                buttons.t.Abort();
                t.Abort();
                g.Abort();
                log.addLog("All threads killed.");
                this.Exit();
            }

            // TODO: Add your update logic here

            log.Update(Mouse.GetState());
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            try
            {
                grid.Draw(spriteBatch, Buttons.axes);
            }
            catch (Exception)
            {
            }
            buttons.Draw(spriteBatch);
            log.Draw(spriteBatch);
            GetData.Draw(spriteBatch, new Rectangle(50, 50, 400, 400));
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
