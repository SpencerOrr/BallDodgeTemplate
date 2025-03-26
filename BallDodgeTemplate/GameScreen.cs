using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace BallDodgeTemplate
{
    public partial class GameScreen : UserControl
    {
        public static int lives = 3;
        public static int points = 0;
        int BulletBallTimer = 0;
        int rounds = 1; 

        bool ballheld = true;
        bool bounce = true;

        public static int screenWidth;
        public static int screenHeight;

        double ballSpeed = 4;
        double speedMultiplier = 4;

        bool leftArrowDown, rightArrowDown;


        Ball chaseBall;
        Random randGen = new Random();
        SolidBrush greenBrush = new SolidBrush(Color.Green);
        SolidBrush redBrush = new SolidBrush(Color.Red);

        List<Ball> balls = new List<Ball>();
        Player hero = new Player();

        List<Bricks> bricks = new List<Bricks>();
        List<Powers> powerUps = new List<Powers>();




        public GameScreen()
        {
            InitializeComponent();

            screenHeight = this.Height;
            screenWidth = this.Width;

            InitGame();
        }

        public void InitGame()
        {
            hero = new Player();
            CreateBricks();

            // Start ball on top of paddle
            int startX = hero.x + (hero.width / 2) - 4; // Centered on paddle
            int startY = hero.y - 10; // Just above the paddle

            chaseBall = new Ball(startX, startY, Convert.ToInt32(ballSpeed), Convert.ToInt32(ballSpeed));

        }
        private void gameTImer_Tick(object sender, EventArgs e)
        {

            if (rightArrowDown == true)
            {
                hero.Move("right");
            } 
            if (leftArrowDown == true)
            {
                hero.Move("left");
            }
           

  
            if (ballheld)
            {
                chaseBall.x = hero.x + (hero.width / 2) - 4;
                chaseBall.y = hero.y - 10;
                if (chaseBall.ySpeed > 0)
                {
                    chaseBall.ySpeed = -chaseBall.ySpeed;
                }
            }
            else
            {
                chaseBall.Move();
            }

            foreach (Ball b in balls)
            {
                b.Move();
            }

            foreach (Powers p in powerUps)
            {
                p.Move();
            }

            if (hero.Collision(chaseBall))
            {
                points++;
            }


            if (chaseBall.y ==  this.Height - chaseBall.size)
            {
                lives--;

                hero = new Player();
                chaseBall.x = hero.x + (hero.width / 2) - 4;
                chaseBall.y = hero.y - 10;
                ballheld = true;

            }

            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                Powers p = powerUps[i];

               
                if (hero.x < p.x + p.size && hero.x + hero.width > p.x &&
                    hero.y < p.y + p.size && hero.y + hero.height > p.y)
                {
                    ApplyPowerUps(p.type);
                    powerUps.RemoveAt(i);

                }
            }

            if (bounce == false)
            {
                BulletBallTimer--;
                if (BulletBallTimer <= 0) bounce = true;          
            }

            if (lives == 0) 
            {
                gameTImer.Stop();
                Form1.ChangeScreen(this, new MenuScreen());
            }

            if (bricks.Count == 0)
            {
                rounds++;
                if (rounds == 2)CreateBricks2(); 
                else
                {
                    gameTImer.Stop();
                    Form1.ChangeScreen(this, new MenuScreen());
                }
            }

            CheckBallBrickCollision();

            Refresh();
        }

        private void GameScreen_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillEllipse(greenBrush, chaseBall.x, chaseBall.y, chaseBall.size, chaseBall.size);

            foreach (Ball b in balls)
            {
                e.Graphics.FillEllipse(redBrush, b.x, b.y, b.size, b.size);
            }

            foreach (var brick in bricks)
            {
                e.Graphics.FillRectangle(brick.Color, brick.Rect);
                e.Graphics.DrawRectangle(Pens.Black, brick.Rect);
            }

            foreach (Powers p in powerUps)
            {
                e.Graphics.FillEllipse(p.color, p.x, p.y, p.size, p.size);
            }


            e.Graphics.FillRectangle(greenBrush, hero.x, hero.y, hero.width, hero.height);



            // **Draw Score and Lives**
            Font drawFont = new Font("Arial", 14, FontStyle.Bold);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            e.Graphics.DrawString($"Score: {points}", drawFont, drawBrush, 10, 5);
            e.Graphics.DrawString($"Lives: {lives}", drawFont, drawBrush, this. Width -100, 5);
            //e.Graphics.DrawString($"Ball Speed: {ballSpeed}", drawFont, drawBrush, this.Width - 200, 300);


        }

        private void GameScreen_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = true;
                    if (ballheld) { ballheld = false; } // Release ball when moving
                    break;
                case Keys.Right:
                    rightArrowDown = true;
                    if (ballheld) { ballheld = false; } // Release ball when moving
                    break;
            }
        }

        private void GameScreen_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = false;
                    break;
                case Keys.Right:
                    rightArrowDown = false;
                    break;
            }
        }

        public void BadBall()
        {
            int x = randGen.Next(20, this.Width - 50);
            int y = randGen.Next(20, this.Height - 50);
            Ball b = new Ball(x, y, 8, 8);
            balls.Add(b);
        }

        public void CreateBricks()
        {
            bricks.Clear(); // Clear existing bricks to prevent duplication

            for (int row = 0; row < Bricks.numRows; row++)
            {
                for (int col = 0; col < Bricks.numCols; col++)
                {
                    int x = col * (Bricks.width + Bricks.spacing) + 2;// Offset from side
                    int y = row * (Bricks.height + Bricks.spacing) + 30; // Offset from top

                    // Assign a random color
                    Color brickColor = Color.FromArgb(randGen.Next(256), randGen.Next(256), randGen.Next(256));
                    SolidBrush brush = new SolidBrush(brickColor);


                    bricks.Add(new Bricks(x, y, Bricks.width, Bricks.height, brush));

                }
            }
        }

        public void CreateBricks2()
        {
            int brickWidth = Bricks.width;
            int brickHeight = Bricks.height;
            int spacing = Bricks.spacing;

            int startX = screenWidth / 2; 
            int startY = 50; 

            int rows = 5; 

            for (int row = 0; row < rows; row++)
            {
                int bricksInRow = rows - row; 

                for (int col = 0; col < bricksInRow; col++)
                {
                    int x = startX - (bricksInRow * (brickWidth + spacing) / 2) + col * (brickWidth + spacing);
                    int y = startY + row * (brickHeight + spacing);

                    Color brickColor = Color.FromArgb(randGen.Next(256), randGen.Next(256), randGen.Next(256));
                    SolidBrush brush = new SolidBrush(brickColor);

                    bricks.Add(new Bricks(x, y, brickWidth, brickHeight, brush));
                }
            }
        }


        private void CheckBallBrickCollision()
        {
            for (int i = 0; i < bricks.Count; i++)
            {
                if (chaseBall.Collision(bricks[i].Rect))
                {
                    BricksDestroyed(i);
                    bricks.RemoveAt(i);
                    if (bounce == true)
                    {
                        chaseBall.ySpeed = chaseBall.ySpeed * -1;
                    }
                    points += 10;
                    ballSpeed *= speedMultiplier;
                    speedMultiplier /= 1.5;
                }
            }
        }

        public void BricksDestroyed (int i)
        {

        
            if (randGen.Next(100) < 30)
            {
                string[] powerUpTypes = { "ExtraLife", "SpeedBoost", "BigPaddle", "Bullet" };
                string selectedPowerUp = powerUpTypes[randGen.Next(powerUpTypes.Length)];

                powerUps.Add(new Powers(bricks[i].Rect.X + Bricks.width / 2, bricks[i].Rect.Y, selectedPowerUp));
            }
        }

        public void ApplyPowerUps(string type)
        {
            if (type == "ExtraLife") lives++;
            else if (type == "SpeedBoost") hero.speed += 2;
            else if (type == "BigPaddle") hero.width += 20;
            else if (type == "Bullet")
            {
                bounce = false;
                BulletBallTimer = 200; 

            }

        }

    }
}



