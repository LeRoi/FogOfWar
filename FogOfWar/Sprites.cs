using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FogOfWar {
    public class Sprites {
        public static string BACKGROUND = "background";
        public static string ORB = "orb";
        public static string TANK = "tank";
        public static string WALL = "wall";

        private Dictionary<String, Texture2D> sprites;

        public Sprites(ContentManager content) {
            sprites = new Dictionary<string, Texture2D>();
            sprites[BACKGROUND] = content.Load<Texture2D>("backgrounds/background");
            sprites[ORB] = content.Load<Texture2D>("entities/orb");
            sprites[TANK] = content.Load<Texture2D>("entities/tank");
            sprites[WALL] = content.Load<Texture2D>("entities/wall");
        }

        public Texture2D this[string key] {
            get { return sprites[key]; }
            set { sprites[key] = value; }
        }
    }
}
