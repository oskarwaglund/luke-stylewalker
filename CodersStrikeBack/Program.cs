﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    class Vector
    {
        public Vector(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

        public double x
        {
            get;
            set;
        }

        public double y
        {
            get;
            set;
        }

        public double length()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public Vector normalize()
        {
            double len = length();
            return new Vector(x / len, y / len);
        }

        public Vector add(Vector v)
        {
            return new Vector(v.x + x, v.y + y);
        }

        public Vector sub(Vector v)
        {
            return new Vector(x - v.x, y - v.y);
        }

        public Vector mul(double d)
        {
            return new Vector(x * d, y * d);
        }

        public double scalar(Vector v)
        {
            return v.x * x + v.y * y;
        }

        public double cross(Vector v)
        {
            return x * v.y - y * v.x;
        }

        public Vector flip(Vector v)
        {
            Vector uV = v.normalize();
            Vector a = uV.mul(this.scalar(uV));
            Vector b = this.sub(a);

            return a.sub(b);
        }

        public Vector vectorProjection(Vector v)
        {
            Vector uV = v.normalize();
            return uV.mul(this.scalar(uV));
        }

        public Vector vectorRejection(Vector v)
        {
            return sub(vectorProjection(v));
        }

        public override string ToString()
        {
            return "{" + x + ", " + y + "}";
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;

        bool usedBoost = false;

        int frame = -1;
        int lastX = -1;
        int lastY = -1;
        int oppLastX = -1;
        int oppLastY = -1;
        int lastShield = 0;

        List<Tuple<int, int>> track = new List<Tuple<int, int>>();

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int nextX = int.Parse(inputs[2]); // x position of the next check point
            int nextY = int.Parse(inputs[3]); // y position of the next check point
            int nextDist = int.Parse(inputs[4]); // distance to the next checkpoint
            int nextAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
            inputs = Console.ReadLine().Split(' ');
            int oppX = int.Parse(inputs[0]);
            int oppY = int.Parse(inputs[1]);

            int thrustX = nextX;
            int thrustY = nextY;

            Tuple<int, int> checkpoint = new Tuple<int, int>(nextX, nextY);
            if (!track.Contains(checkpoint))
            {
                track.Add(checkpoint);
            }

            string trackString = "";
            foreach (Tuple<int, int> cp in track)
            {
                trackString += cp + ", ";
            }
            Console.Error.WriteLine(trackString);

            Vector oppPos = new Vector(oppX - x, oppY - y);
            Vector oppSpeed = new Vector(0, 0);
            Vector speed = new Vector(0, 0);
            if (frame >= 0)
            {
                speed = new Vector(x - lastX, y - lastY);
                oppSpeed = new Vector(oppX - oppLastX, oppY - oppLastY);
                lastX = x;
                lastY = y;
                oppLastX = oppX;
                oppLastY = oppY;
            }
            frame++;

            int thrust = 0;
            if (Math.Abs(nextAngle) >= 70)
            {
                thrust = 0;
            }
            else
            {
                thrust = 100;
            }

            const int slowDown = 1000;
            if (nextDist < slowDown && Math.Abs(nextAngle) < 20)
            {
                thrust = 100 * nextDist / slowDown;
            }


            string thrustString;

            if (willCollide(speed, oppSpeed, oppPos))
            {
                thrustString = "SHIELD";
                lastShield = frame;
            }
            else if (!usedBoost && Math.Abs(nextAngle) < 5 && nextDist > 5000 /*&& speed.length() > 300*/)
            {
                thrustString = "BOOST";
                usedBoost = true;
            }
            else
            {
                thrust = Math.Min(100, Math.Max(0, thrust));
                thrustString = "" + thrust;
            }

            if (Math.Abs(nextAngle) <= 90)
            {
                Vector targetVector = new Vector(nextX - x, nextY - y);
                Vector rejection = speed.vectorRejection(targetVector);
                Console.Error.WriteLine("Speed: " + speed.ToString());
                Console.Error.WriteLine("Target: " + targetVector.ToString());
                Console.Error.WriteLine("Projection: " + speed.vectorProjection(targetVector).ToString());
                Console.Error.WriteLine("Rejection: " + speed.vectorRejection(targetVector).ToString());
                thrustX -= (int)rejection.x * 2;
                thrustY -= (int)rejection.y * 2;
            }


            Console.WriteLine(thrustX + " " + thrustY + " " + thrustString);
        }
    }

    const int COLLISION_THRESHOLD = 1000;
    static bool willCollide(Vector speed, Vector oppSpeed, Vector oppPos)
    {
        return speed.sub(oppPos.add(oppSpeed)).length() < COLLISION_THRESHOLD;
    }
}