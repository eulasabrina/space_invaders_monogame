using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Models
{
    public class Bullet
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float Speed = 10f;
        public bool Active = true;


        public void Update()
        {
            Position.Y -= Speed;
            if (Position.Y < 0)
                Active = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}