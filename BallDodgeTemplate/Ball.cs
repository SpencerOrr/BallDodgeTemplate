using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallDodgeTemplate
{
    internal class Ball
    {

        public int x, y;
        public int size = 6;
        public int xSpeed, ySpeed;

        public Ball(int _x, int _y, int _xspeed, int _yspeed)
        {
            x = _x;
            y = _y;
            xSpeed = _xspeed;
            ySpeed = _yspeed;

        }
        public void Move()
        {
            x += xSpeed;
            y += ySpeed;

            if (x < 0 || x > GameScreen.screenWidth - size)
            {
                xSpeed = -xSpeed;
            }
            if (y < 0 || y > GameScreen.screenHeight - size)
            {
                ySpeed = -ySpeed;
            }
        }
        public bool Collision(Rectangle rect)
        {
            Rectangle ballRect = new Rectangle(x, y, size, size);
            return ballRect.IntersectsWith(rect);
        }
    }
}
