using System;
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

        List<Tuple<int, int>> track = new List<Tuple<int, int>>();

        bool firstLap = true;
        int lap = 1;
        Tuple<int, int> checkpoint = null;
        Tuple<int, int> nextCheckpoint = null;
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

            bool newCheckpoint = false;
            if (checkpoint != null)
            {
                newCheckpoint = checkpoint.Item1 != nextX || checkpoint.Item2 != nextY;
            }
            checkpoint = new Tuple<int, int>(nextX, nextY);
            int checkpointIndex = -1;
            int nextCheckpointIndex = -1;
            if (firstLap){
                if (!track.Contains(checkpoint))
                {
                    track.Add(checkpoint);
                }
                else
                {
                    if (track.IndexOf(checkpoint) == 0 && track.Count() > 1)
                    {
                        firstLap = false;
                        lap++;
                    }
                }
            }
            else
            {
                checkpointIndex = track.IndexOf(checkpoint);
                nextCheckpointIndex = (checkpointIndex + 1) % track.Count();
                nextCheckpoint = track[nextCheckpointIndex];
                if (newCheckpoint && checkpointIndex == 0)
                {
                    lap++;
                }
            }

            Console.Error.WriteLine("First lap: " + firstLap);
            Console.Error.WriteLine("Checkpoint: " + (checkpointIndex+1) + " Next: " + (nextCheckpointIndex+1));
            Console.Error.WriteLine("Lap: " + lap);

            string trackString = "";
            foreach (Tuple<int, int> cp in track)
            {
                trackString += cp + ", ";
            }
            Console.Error.WriteLine(trackString);

            Vector oppPos = new Vector(oppX - x, oppY - y);
            Vector oppSpeed = new Vector(0, 0);
            Vector speed = new Vector(0, 0);
            Vector targetVector = new Vector(nextX - x, nextY - y);

            if (frame >= 0)
            {
                speed = new Vector(x - lastX, y - lastY);
                oppSpeed = new Vector(oppX - oppLastX, oppY - oppLastY);
                lastX = x;
                lastY = y;
                oppLastX = oppX;
                oppLastY = oppY;
            }
            else
            {
                frame++;
                Console.WriteLine(nextX + " " + nextY + " BOOST");
                continue;
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

            const int slowDown = 2000;
            if (nextDist < slowDown && Math.Abs(nextAngle) < 20)
            {
                thrust = 100 * nextDist / slowDown;
            }

            if (!firstLap && !(lap == 3 && checkpointIndex == track.Count()-1) && 
                oppPos.length() > 2000 && targetVector.length() < 2000 && 
                speed.vectorRejection(targetVector).length() < 500
                && new Vector(nextCheckpoint.Item1 - nextX, nextCheckpoint.Item2 - nextY).scalar(targetVector) < 0)
            {
                Console.Error.WriteLine("Coasting at angle " + nextAngle);
                Console.WriteLine(nextCheckpoint.Item1 + " " + nextCheckpoint.Item2 + " 0");
                continue;
            }


            string thrustString;

            if (willCollide(speed, oppSpeed, oppPos))
            {
                thrustString = "SHIELD";
            }
            else
            {
                thrust = Math.Min(100, Math.Max(0, thrust));
                thrustString = "" + thrust;
            }

            if (Math.Abs(nextAngle) <= 90)
            {
                Vector rejection = speed.vectorRejection(targetVector);
                Console.Error.WriteLine("Speed: " + speed.ToString());
                Console.Error.WriteLine("Target: " + targetVector.ToString());
                Console.Error.WriteLine("Projection: " + speed.vectorProjection(targetVector).ToString());
                Console.Error.WriteLine("Rejection: " + speed.vectorRejection(targetVector).ToString());
                thrustX -= (int)rejection.x * 4;
                thrustY -= (int)rejection.y * 4;
            }


            Console.WriteLine(thrustX + " " + thrustY + " " + thrustString);
        }
    }

    const int COLLISION_THRESHOLD = 1000;
    static bool willCollide(Vector speed, Vector oppSpeed, Vector oppPos)
    {
        return speed.scalar(oppSpeed) < 0.2 && speed.sub(oppPos.add(oppSpeed)).length() < COLLISION_THRESHOLD;
    }
}