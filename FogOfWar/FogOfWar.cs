using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FogOfWar {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class FogOfWar : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Sprites sprites;

        private int width = 500;
        private int height = 500;

        // Orb fields
        private Texture2D orbSprite;
        private Vector2 position;

        public FogOfWar() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;

            this.IsMouseVisible = false;
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

            position = new Vector2(height / 2, width / 2);
            orbSprite = sprites[Sprites.ORB];
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            sprites = new Sprites(Content);
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
            if (keyboard.IsKeyDown(Keys.D)) position.X += 1;
            if (keyboard.IsKeyDown(Keys.A)) position.X -= 1;
            if (keyboard.IsKeyDown(Keys.S)) position.Y += 1;
            if (keyboard.IsKeyDown(Keys.W)) position.Y -= 1;

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                null, null, null, null, null);
            spriteBatch.Draw(orbSprite, new Rectangle((int) position.X, (int) position.Y,
                orbSprite.Width, orbSprite.Height), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
