using System.Collections.Generic;
using game.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Linq;

namespace game;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    float enemySpeed = 1f;
    bool movingRight = true;
    float shootCooldown = 0.3f;
    float shootTimer = 0f;
    float enemyShootTimer = 0f;
    float enemyShootCooldown = 1.5f;
    float backgroundY = 0f;
    float scrollSpeed = 50f; //pixel per second
    bool isGameOver = false;
    bool isVictory = false;
    int lives = 3;
    int score = 0;
    Song backgroundMusic;
    SoundEffect shootSound;
    Player player;
    Texture2D playerTexture, enemyTexture, bulletTexture;
    Texture2D backgroundTexture;
    Texture2D victoryBackgroundTexture;
    Texture2D gameOverBackgroundTexture;
    Texture2D enemyBulletTexture;
    List<Enemy> enemies = new();
    List<EnemyBullet> enemyBullets = new();
    List<Bullet> bullets = new();
    SpriteBatch spriteBatch;
    SpriteFont font;
    SpriteFont hudFont;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        playerTexture = Content.Load<Texture2D>("player");
        enemyTexture = Content.Load<Texture2D>("enemy");
        enemyBulletTexture = Content.Load<Texture2D>("enemy_bullet");
        bulletTexture = Content.Load<Texture2D>("bullet");
        shootSound = Content.Load<SoundEffect>("shoot");
        backgroundMusic = Content.Load<Song>("background_music");
        backgroundTexture = Content.Load<Texture2D>("background");
        victoryBackgroundTexture = Content.Load<Texture2D>("victory_background");
        gameOverBackgroundTexture = Content.Load<Texture2D>("gameover_background");
        font = Content.Load<SpriteFont>("DefaultFont");
        hudFont = Content.Load<SpriteFont>("HudFont");

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.4f;
        MediaPlayer.Play(backgroundMusic);

        player = new Player
        {
            Texture = playerTexture,
            Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - 60),
            Speed = 5f,
            Scale = 0.6f
        };

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                enemies.Add(new Enemy
                {
                    Texture = enemyTexture,
                    Position = new Vector2(100 + col * 60, 50 + row * 60)
                });
            }
        }        
    }
    protected override void Update(GameTime gameTime)
    {
        shootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        bool changeDirection = false;

        backgroundY += scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (backgroundY >= GraphicsDevice.Viewport.Height)
            backgroundY = 0f;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (isVictory || isGameOver) //stop moving
            
            return;    

        var kstate = Keyboard.GetState();


        //Player Movement
            if (kstate.IsKeyDown(Keys.Left))
                player.Position.X -= player.Speed;
        if (kstate.IsKeyDown(Keys.Right))
            player.Position.X += player.Speed;

        //Shooting
        if (kstate.IsKeyDown(Keys.Space) && shootTimer >= shootCooldown)
        {
            bullets.Add(new Bullet
            {
                Texture = bulletTexture,
                Position = new Vector2(player.Position.X + player.Texture.Width / 2 - 4, player.Position.Y)
            });

            shootSound.Play(); // play the sound
            shootTimer = 0f;
        }

        //Enemy movement
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            enemy.Position.X += movingRight ? enemySpeed : -enemySpeed;
            if (enemy.Position.X > GraphicsDevice.Viewport.Width - enemy.Texture.Width || enemy.Position.X < 0)
            {
                changeDirection = true;
            }
        }
        if (changeDirection)
        {
            movingRight = !movingRight;
            foreach (var enemy in enemies) enemy.Position.Y += 10;
        }

        //Update Bullets
        foreach (var b in bullets) b.Update();
        bullets.RemoveAll(b => !b.Active);

        foreach (var eb in enemyBullets) eb.Update();
        enemyBullets.RemoveAll(b => !b.Active);

        //Bullet-enemy Collision
        foreach (var enemy in enemies)
        {
            foreach (var bullet in bullets)
            {
                Rectangle eRect = new((int)enemy.Position.X, (int)enemy.Position.Y, 20, 20);
                Rectangle bRect = new((int)bullet.Position.X, (int)bullet.Position.Y, 8, 16);

                if (enemy.IsAlive && bRect.Intersects(eRect))
                {
                    enemy.IsAlive = false;
                    bullet.Active = false;
                    score += 100; //increase rthe score
                }
            }
        }

        if (!isVictory && enemies.All(e => !e.IsAlive))
        {
            isVictory = true;
        }

        //Enemy shooting
            enemyShootTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (enemyShootTimer >= enemyShootCooldown)
        {
            var aliveEnemies = enemies.FindAll(e => e.IsAlive);
            if (aliveEnemies.Count > 0)
            {
                var random = new Random();
                var shooter = aliveEnemies[random.Next(aliveEnemies.Count)];

                enemyBullets.Add(new EnemyBullet
                {
                    Texture = enemyBulletTexture,
                    Position = new Vector2(shooter.Position.X + shooter.Texture.Width / 2 - 4, shooter.Position.Y + 20)
                });
            }

            enemyShootTimer = 0f;
        }

        //Check if player got hit
        foreach (var eb in enemyBullets)
        {
            Rectangle bRect = new((int)eb.Position.X, (int)eb.Position.Y, 8, 16);
            Rectangle pRect = new((int)player.Position.X, (int)player.Position.Y, 32, 32);

            if (bRect.Intersects(pRect))
            {
                eb.Active = false;
                lives--;
                if (lives <= 0)
                {
                    MediaPlayer.Stop();
                    isGameOver = true; //Game over
                }

            }
        }          

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
       
        spriteBatch.Draw(
            backgroundTexture,
            destinationRectangle: new Rectangle(0, (int)backgroundY, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            color: Color.White
        );
        spriteBatch.Draw(
            backgroundTexture,
            destinationRectangle: new Rectangle(0, (int)backgroundY - GraphicsDevice.Viewport.Height, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
            color: Color.White
        );

        if (isGameOver)
        {
            spriteBatch.Draw(
                gameOverBackgroundTexture,
                destinationRectangle: new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                color: Color.White
            );
        }

        if (!isGameOver && !isVictory)
        {
            player.Draw(spriteBatch);
            foreach (var e in enemies) e.Draw(spriteBatch);
            foreach (var b in bullets) b.Draw(spriteBatch);
            foreach (var eb in enemyBullets) eb.Draw(spriteBatch);

            //HUD
            spriteBatch.DrawString(hudFont,$"Lives: {lives}",new Vector2(10,10), Color.Red);
            spriteBatch.DrawString(hudFont,$"Score: {score}", new Vector2(10,30),Color.White);
        }      

        spriteBatch.End();

        //Game Over Message
        if (isGameOver)
        {
            spriteBatch.Begin();            
            string message = "GAME OVER!";
            Vector2 size = font.MeasureString(message);
            Vector2 messagePos = new Vector2(
                (GraphicsDevice.Viewport.Width - size.X) / 2,
                (GraphicsDevice.Viewport.Height - size.Y) / 2
            );

            string scoreText = $"Score: {score}";
            Vector2 scoreSize = font.MeasureString(scoreText);
            Vector2 scorePos = new Vector2(
                (GraphicsDevice.Viewport.Width - scoreSize.X) / 2,
                messagePos.Y + 50
            );

            spriteBatch.DrawString(font, message, messagePos, Color.Black);
            spriteBatch.DrawString(font, scoreText, scorePos, Color.Red);
            spriteBatch.End();
        }

        //Victory Message
        if (isVictory)
        {
            spriteBatch.Begin();
            string message = "CONGRATULATIONS! YOU WIN!!";
            Vector2 size = font.MeasureString(message);
            Vector2 messagePos = new Vector2(
                (GraphicsDevice.Viewport.Width - size.X) / 2,
                (GraphicsDevice.Viewport.Height - size.Y) / 2
            );

            string scoreText = $"Score: {score}";
            Vector2 scoreSize = font.MeasureString(scoreText);
            Vector2 scorePos = new Vector2(
                (GraphicsDevice.Viewport.Width - scoreSize.X) / 2,
                 messagePos.Y + 50
            );

            spriteBatch.DrawString(font, message, messagePos,Color.Yellow);
            spriteBatch.DrawString(font, scoreText, scorePos, Color.Red);
            spriteBatch.End();
        }      

        base.Draw(gameTime);
    }
}
