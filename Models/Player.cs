using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Models
{
    public class Player
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float Speed = 5f;
        public float Scale = 1f;

        public void Draw(SpriteBatch spriteBatch)
        {
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
