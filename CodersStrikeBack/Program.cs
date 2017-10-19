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

        public int angleTo(Vector v)
        {
            return (int)(180 / Math.PI * Math.Acos(scalar(v) / length() / v.length()));
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

            lap = 0;
        }

        public void update(int x, int y, int vX, int vY, int angle, int nextCheckpointId)
        {
            position = new Vector(x, y);
            speed = new Vector(vX, vY);
            this.angle = angle;

            if (nextCheckpointId != this.nextCheckpointId && nextCheckpointId == 1)
            {
                lap++;
            }
            Console.Error.WriteLine("Pod " + id + " is at lap " + lap);
            this.nextCheckpointId = nextCheckpointId;
        }

        public string calculateThrust(List<Pod> pods, Track track)
        {
            if (id == 0)
            {
                return calculateSpeedyThrust(pods, track);
            }
            else
            {
                return calculateSpeedyThrust(pods, track);
            }
        }

        string calculateSpeedyThrust(List<Pod> pods, Track track)
        {
            int cX = track.getX(nextCheckpointId);
            int cY = track.getY(nextCheckpointId);

            int nCX = track.getX((nextCheckpointId + 1) % track.length());
            int nCY = track.getY((nextCheckpointId + 1) % track.length());
            Vector cpVector = new Vector(cX - position.x, cY - position.y);
            Vector nextCpVector = new Vector(nCX - position.x, nCY - position.y);

            Vector faceVector = new Vector(Math.Cos(angle * Math.PI / 180), Math.Sin(angle * Math.PI / 180));


            

            if (!usedBoost && id == 0)
            {
                usedBoost = true;
                return cX + " " + cY + " BOOST";
            }

            int thrust = 0;
            Vector thrustVector = null;
            if (cpVector.length() > 1000 || (lap == track.laps && nextCheckpointId == track.length() - 1))
            {
                thrustVector = cpVector;
            }
            else
            {
                thrustVector = nextCpVector;
            }

            for (int i = 2; i < 4; i++)
            {
                if (willCollide(pods[i]))
                {
                    return thrustVector.x + " " + thrustVector.y + " SHIELD";
                }
            }

            int nextAngle = thrustVector.angleTo(faceVector);


            if (Math.Abs(nextAngle) >= 90)
            {
                thrust = 0;
            }
            else if (Math.Abs(nextAngle) >= 70)
            {
                thrust = 50;
            }
            else
            {
                thrust = 100;
            }

            /*
            const int slowDown = 2000;
            if (thrustVector.length() < slowDown && Math.Abs(nextAngle) < 20)
            {
                thrust = (int)(100 * thrustVector.length() / slowDown);
            }*/

            string message = " " + thrust;


            if (Math.Abs(nextAngle) <= 90)
            {
                Vector rejection = speed.vectorRejection(thrustVector);
                //Console.Error.WriteLine("Speed: " + speed.ToString());
                //Console.Error.WriteLine("Target: " + targetVector.ToString());
                //Console.Error.WriteLine("Projection: " + speed.vectorProjection(targetVector).ToString());
                //Console.Error.WriteLine("Rejection: " + speed.vectorRejection(targetVector).ToString());
                thrustVector = thrustVector.sub(rejection.mul(4)); 
            }

            if (thrustVector == nextCpVector)
            {
                message += " SLIDIN'";
            }

            return (int)(position.x+thrustVector.x) + " " + (int)(position.y+thrustVector.y) + " " + thrust;
        }

        string calculateSluggerThrust(List<Pod> pods, List<Tuple<int, int>> checkpoints)
        {
            int e1Progress = getProgress(pods[2]);
            int e2Progress = getProgress(pods[3]);

            Pod pod = e1Progress >= e2Progress ? pods[2] : pods[3];
            Vector podV = pod.position.sub(position);
            for (int i = 2; i < 4; i++)
            {
                if (willCollide(pods[i]))
                {
                    return "0 0 SHIELD";
                }
            }
            if (podV.length() > 3000)
            {
                return checkpoints[pod.nextCheckpointId].Item1 + " " + checkpoints[pod.nextCheckpointId].Item2 + " 100";
            }
            else
            {
                return (pod.position.x + pod.speed.x) + " " + (pod.position.y + pod.speed.y) + " 100";
            }

        }

        int getProgress(Pod pod)
        {
            return pod.lap * 10 + pod.nextCheckpointId;
        }

        const int COLLISION_THRESHOLD = 800;
        bool willCollide(Pod pod)
        {
            return position.add(speed).sub(pod.position.add(pod.speed)).length() < COLLISION_THRESHOLD;
        }

        public Vector position { get; set; }
        public Vector speed { get; set; }
        public int angle { get; set; }
        public int nextCheckpointId { get; set; }

        public int team { get; set; }
        public int id { get; set; }

        private bool usedBoost { get; set; }

        private int lap { get; set; }
    }

    private class Track
    {
        public List<Tuple<int, int>> checkpoints
        {
            get;
            private set;
        }

        public int laps
        {
            get;
            private set;
        }

        public Track(int laps)
        {
            this.laps = laps;
            checkpoints = new List<Tuple<int, int>>();
        }

        public void addCheckpoint(int x, int y)
        {
            checkpoints.Add(new Tuple<int, int>(x, y));
        }

        public int getX(int checkpoint)
        {
            return checkpoints[checkpoint].Item1;
        }

        public int getY(int checkpoint)
        {
            return checkpoints[checkpoint].Item2;
        }

        public int length()
        {
            return checkpoints.Count();
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;

        int laps = int.Parse(Console.ReadLine());
        int checkpoints = int.Parse(Console.ReadLine());
        Track track = new Track(laps);
        for (int i = 0; i < checkpoints; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int cX = int.Parse(inputs[0]);
            int cY = int.Parse(inputs[1]);
            track.addCheckpoint(cX, cY);
        }

        Track optimizedTrack = new Track(laps);
        for (int i = 0; i < checkpoints; i++)
        {
            int id1 = (i + checkpoints - 1) % checkpoints;
            int id2 = (i + 1) % checkpoints;

            Vector v1 = new Vector(track.getX(id1) - track.getX(i), track.getY(id1) - track.getY(i));
            Vector v2 = new Vector(track.getX(id2) - track.getX(i), track.getY(id2) - track.getY(i));

            Vector v = v1.normalize().add(v2.normalize()).normalize().mul(300);

            optimizedTrack.addCheckpoint(track.getX(i) + (int)v.x, track.getY(i) + (int)v.y);
        }

        List<Pod> pods = new List<Pod>();
        for (int i = 0; i < 4; i++)
        {
            pods.Add(new Pod(i, i / 2 + 1));
        }

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
                Console.WriteLine(pods[i].calculateThrust(pods, optimizedTrack));
            }
        }
    }
}