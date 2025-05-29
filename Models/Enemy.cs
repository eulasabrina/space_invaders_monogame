using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Models
{
    public class Enemy
    {
        public Texture2D Texture;
        public Vector2 Position;
        public bool IsAlive = true;
        public float Scale = 0.5f;
        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsAlive)
                spriteBatch.Draw(
                    Texture,
                    Position,
                    null,
                    Color.White,
                    0f, 
                    Vector2.Zero,
                    Scale = 0.6f,
                    SpriteEffects.None,
                    0f
                );
        }
    }
}