using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FogOfWar {
    public class Sprites {
        public static string ORB = "orb";
        public static string TANK = "tank";

        private Dictionary<String, Texture2D> sprites;

        public Sprites(ContentManager content) {
            sprites = new Dictionary<string, Texture2D>();
            sprites[ORB] = content.Load<Texture2D>("entities/orb");
            sprites[TANK] = content.Load<Texture2D>("entities/tank");
        }

        public Texture2D this[string key] {
            get { return sprites[key]; }
            set { sprites[key] = value; }
        }
    }
}
