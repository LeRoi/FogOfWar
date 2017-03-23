using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

/**
 * A quick proof of concept for 2D fog of war.
 */
namespace FogOfWar {
    public class FogOfWar : Game {
        struct Entity {
            public Texture2D sprite;
            public Vector2 position;

            public Entity(Texture2D sprite, Vector2 position) {
                this.sprite = sprite;
                this.position = position;
            }

            public Rectangle getRect() {
                return new Rectangle((int) position.X, (int) position.Y,
                    sprite.Width, sprite.Height);
            }
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch fogBatch;
        Sprites sprites;

        Effect fog;

        private int width = 500;
        private int height = 500;

        // Background
        private Texture2D background;

        // Walls
        private List<Entity> walls;
        private Texture2D wallSprite;

        // Orb fields
        private Entity orb;
        private int orbSpeed = 1;

        // Tank fields
        private Entity tank;

        public FogOfWar() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;

            this.IsMouseVisible = true;
            this.IsFixedTimeStep = true;
            graphics.SynchronizeWithVerticalRetrace = true;
            // Default to 60fps
            // Use fixed game loop instead of variable TimeStep
            // May result in less smooth experience on high-end machines
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();

            background = sprites[Sprites.BACKGROUND];

            orb = new Entity(sprites[Sprites.ORB], new Vector2(width / 2, height / 2));
            tank = new Entity(sprites[Sprites.TANK], new Vector2(100, 100));

            wallSprite = sprites[Sprites.WALL];
            walls = new List<Entity>();
            for (int i = 0; i < width / wallSprite.Width + 1; i++) {
                walls.Add(new Entity(wallSprite, getWallPosition(i, 0)));
                walls.Add(new Entity(wallSprite, getWallPosition(i, height / wallSprite.Height)));
            }

            // Ignore the first and last to avoid doubling up on sprites in these areas.
            for (int j = 1; j < height / wallSprite.Height; j++) {
                walls.Add(new Entity(wallSprite, getWallPosition(0, j)));
                walls.Add(new Entity(wallSprite, getWallPosition(width / wallSprite.Width, j)));
            }

            walls.Add(new Entity(wallSprite, getWallPosition(4, 15)));
            walls.Add(new Entity(wallSprite, getWallPosition(5, 15)));
            walls.Add(new Entity(wallSprite, getWallPosition(6, 15)));
            walls.Add(new Entity(wallSprite, getWallPosition(7, 15)));
            walls.Add(new Entity(wallSprite, getWallPosition(8, 15)));
            walls.Add(new Entity(wallSprite, getWallPosition(9, 15)));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fogBatch = new SpriteBatch(GraphicsDevice);
            sprites = new Sprites(Content);

            fog = Content.Load<Effect>("shaders/FogShader");
            fog.Parameters["width"].SetValue(width);
            fog.Parameters["height"].SetValue(height);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyboard = Keyboard.GetState();

            Vector2 newPosition = new Vector2(orb.position.X, orb.position.Y);
            if (keyboard.IsKeyDown(Keys.D)) newPosition.X += orbSpeed;
            if (keyboard.IsKeyDown(Keys.A)) newPosition.X -= orbSpeed;
            if (keyboard.IsKeyDown(Keys.S)) newPosition.Y += orbSpeed;
            if (keyboard.IsKeyDown(Keys.W)) newPosition.Y -= orbSpeed;

            Rectangle newOrb = new Rectangle((int) newPosition.X, (int) newPosition.Y,
                    orb.sprite.Width, orb.sprite.Height);
            bool intersects = false;
            foreach (Entity wall in walls) {
                Rectangle wallDims = wall.getRect();
                if (wall.getRect().Intersects(newOrb)) intersects = true;
            }

            if (tank.getRect().Intersects(newOrb)) intersects = true;

            orb.position = intersects ? orb.position : newPosition;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            MouseState mouse = Mouse.GetState();
            float mousex = mouse.X;
            float mousey = mouse.Y;
            if (mousex < 0) mousex = 0;
            if (mousex > width) mousex = width;
            if (mousey < 0) mousey = 0;
            if (mousey > height) mousey = height;
            fog.Parameters["mouse_x"].SetValue(mousex);
            fog.Parameters["mouse_y"].SetValue(mousey);
            fog.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            foreach (Entity wall in walls) {
                spriteBatch.Draw(wall.sprite, wall.getRect(), Color.White);
            }

            spriteBatch.Draw(tank.sprite, tank.getRect(), Color.White);
            spriteBatch.Draw(orb.sprite, orb.getRect(), Color.White);
            spriteBatch.Draw(orb.sprite, orb.position, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private Vector2 getWallPosition(float x, float y) {
            return new Vector2((x - 0.5f) * wallSprite.Width, (y - 0.5f) * wallSprite.Height);
        }
    }
}
