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

    class Pod
    {
        public Pod(int id, int team)
        {
            position = null;
            speed = null;
            this.angle = 0;
            this.nextCheckpointId = 0;

            this.team = team;
            this.id = id;

            usedBoost = false;
        }

        public void update(int x, int y, int vX, int vY, int angle, int nextCheckpointId)
        {
            position = new Vector(x, y);
            speed = new Vector(vX, vY);
            this.angle = angle;
            this.nextCheckpointId = nextCheckpointId;
        }

        public string calculateThrust(List<Pod> pods, List<Tuple<int, int>> checkpoints)
        {
            int cX = checkpoints[nextCheckpointId].Item1;
            int cY = checkpoints[nextCheckpointId].Item2;

            Vector targetVector = new Vector(cX - position.x, cY - position.y);
            Vector faceVector = new Vector(Math.Cos(angle * Math.PI / 180), Math.Sin(angle * Math.PI / 180));

            int nextAngle = (int)(180 / Math.PI * Math.Acos(targetVector.scalar(faceVector) / targetVector.length() / faceVector.length()));

            if (!usedBoost)
            {
                usedBoost = true;
                return cX + " " + cY + " BOOST";
            }

            int thrust = 0;
            int thrustX = cX;
            int thrustY = cY;
            if (Math.Abs(nextAngle) >= 70)
            {
                thrust = 0;
            }
            else
            {
                thrust = 100;
            }

            const int slowDown = 2000;
            if (targetVector.length() < slowDown && Math.Abs(nextAngle) < 20)
            {
                thrust = (int)(100 * targetVector.length() / slowDown);
            }

            int closest = 25000;
            for (int i = 0; i < 4; i++)
            {
                if (i == id) continue;
                closest = (int)Math.Min(position.sub(pods[i].position).length(), closest);
            }

            //TODO: Check for last checkpoint
            if (/*!firstLap && !(lap == 3 && checkpointIndex == track.Count() - 1) &&*/
                closest > 2000 && targetVector.length() < 2000 &&
                speed.vectorRejection(targetVector).length() < 500)
            {
                int nextId = (nextCheckpointId + 1) % checkpoints.Count();
                Vector nextTarget = new Vector(checkpoints[nextId].Item1 - cX, checkpoints[nextId].Item2 - cY);
                if (nextTarget.scalar(targetVector) < 0)
                {
                    Console.Error.WriteLine("Pod " + id + " Coasting at angle " + nextAngle);
                    return checkpoints[nextId].Item1 + " " + checkpoints[nextId].Item2 + " 0";
                }
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

            return thrustX + " " + thrustY + " " + thrust;
        }

        public Vector position { get; set; }
        public Vector speed { get; set; }
        public int angle { get; set; }
        public int nextCheckpointId { get; set; }

        public int team { get; set; }
        public int id { get; set; }

        private bool usedBoost { get; set; }
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

        int laps = int.Parse(Console.ReadLine());
        int checkpoints = int.Parse(Console.ReadLine());
        for (int i = 0; i < checkpoints; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int cX = int.Parse(inputs[0]);
            int cY = int.Parse(inputs[1]);
            track.Add(new Tuple<int, int>(cX, cY));
        }

        int lap = 1;
        Tuple<int, int> checkpoint = null;
        Tuple<int, int> nextCheckpoint = null;

        List<Pod> pods = new List<Pod>();
        for (int i = 0; i < 4; i++)
        {
            pods.Add(new Pod(i, i / 2 + 1));
        }
        // game loop
        while (true)
        {
            for (int i = 0; i < 4; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                pods[i].update(
                    int.Parse(inputs[0]),
                    int.Parse(inputs[1]),
                    int.Parse(inputs[2]),
                    int.Parse(inputs[3]),
                    int.Parse(inputs[4]),
                    int.Parse(inputs[5]));
            }

            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine(pods[i].calculateThrust(pods, track));
            }
        }
    }

    const int COLLISION_THRESHOLD = 1000;
    static bool willCollide(Vector speed, Vector oppSpeed, Vector oppPos)
    {
        return speed.scalar(oppSpeed) < 0.2 && speed.sub(oppPos.add(oppSpeed)).length() < COLLISION_THRESHOLD;
    }
}