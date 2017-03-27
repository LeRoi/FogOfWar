using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

/**
 * A quick proof of concept for 2D fog of war.
 */
namespace FogOfWar {
    public class FogOfWar : Game {
        public class Entity {
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

            public void draw(SpriteBatch batch) {
                batch.Draw(sprite, getRect(), Color.White);
            }
        }

        public class Wall : Entity {
            public int width;
            public int height;

            public Wall(Texture2D sprite, Vector2 position, int width, int height) :
                base(sprite, position) {
                this.width = width;
                this.height = height;
            }

            public new Rectangle getRect() {
                return new Rectangle((int) position.X, (int) position.Y,
                    sprite.Width * width, sprite.Height * height);
            }

            public new void draw(SpriteBatch batch) {
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        batch.Draw(sprite, new Vector2(position.X + sprite.Width * i,
                            position.Y + sprite.Height * j));
                    }
                }
            }
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch fogBatch;
        Sprites sprites;

        Effect fog;

        public static int width = 500;
        public static int height = 500;

        // Background
        private Texture2D background;

        // Walls
        private List<Wall> walls;
        private Texture2D wallSprite;
        private List<Vector2> polygon; // Light map polygon
        private GameGeometry geometry; // Light map geometry

        // Orb fields
        // Position is not centered on the sprite but that's OK for this demo.
        private Entity orb;
        private int orbSpeed = 2;

        // Tank fields
        private Entity tank;

        private bool showPolygon = false;

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
            walls = new List<Wall>();

            walls.Add(new Wall(wallSprite, getWallPosition(4, 15), 6, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(20, 20), 1, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(3, 6), 3, 3));
            walls.Add(new Wall(wallSprite, getWallPosition(10, 10), 1, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(12, 10), 1, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(14, 10), 1, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(16, 10), 1, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(18, 10), 1, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(20, 10), 1, 1));
            walls.Add(new Wall(wallSprite, getWallPosition(22, 10), 1, 1));
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
            foreach (Wall wall in walls) {
                Rectangle wallDims = wall.getRect();
                if (wall.getRect().Intersects(newOrb)) intersects = true;
            }

            if (tank.getRect().Intersects(newOrb)) intersects = true;

            orb.position = intersects ? orb.position : newPosition;
            
            polygon = LightMap.getLightMap(Mouse.GetState().Position, walls);
            geometry = new GameGeometry(polygon);

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
            fog.Parameters["mouseX"].SetValue(mousex);
            fog.Parameters["mouseY"].SetValue(mousey);
            fog.Parameters["orbX"].SetValue(orb.position.X);
            fog.Parameters["orbY"].SetValue(orb.position.Y);
            
            fog.Parameters["polygonCount"].SetValue(polygon.Count);
            fog.Parameters["xPoints"].SetValue(geometry.XPoints.ToArray());
            fog.Parameters["yPoints"].SetValue(geometry.YPoints.ToArray());
            fog.Parameters["multiples"].SetValue(geometry.PointMultiples.ToArray());
            fog.Parameters["constants"].SetValue(geometry.PointConstants.ToArray());

            fog.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            foreach (Wall wall in walls) {
                wall.draw(spriteBatch);
            }

            // Render outline of visible polygon.
            if (showPolygon) {
                foreach (Vector2 point in polygon) {
                    spriteBatch.Draw(orb.sprite, point, Color.White);
                }

                for (int i = 0; i < polygon.Count; i++) {
                    Vector2 point = polygon[i];
                    Vector2 prior = i == 0 ? polygon[polygon.Count - 1] : polygon[i - 1];
                    spriteBatch.DrawLine(point, prior, Color.Red);
                }
            }

            tank.draw(spriteBatch);
            orb.draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private Vector2 getWallPosition(float x, float y) {
            return new Vector2((x - 0.5f) * wallSprite.Width, (y - 0.5f) * wallSprite.Height);
        }
    }
}
