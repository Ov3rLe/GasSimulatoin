using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace GasSimulation.Simulation
{
    public enum Direction
    {
        Left,
        Up_left,
        Up,
        Up_right,
        Right,
        Down_right,
        Down,
        Down_left,
        Static,
        Error
    }

    public struct Vector
    {
        public Direction Direction
        {
            get
            {
                if (Vx == 0)
                {
                    if (Vy == 0)
                        return Direction.Static;

                    if (Vy > 0)
                        return Direction.Down;

                    if (Vy < 0)
                        return Direction.Up;
                }

                if (Vx > 0)
                {
                    if (Vy == 0)
                        return Direction.Right;

                    if (Vy < 0)
                        return Direction.Up_right;

                    if (Vy > 0)
                        return Direction.Down_right;
                }

                if (Vx < 0)
                {
                    if (Vy == 0)
                        return Direction.Left;

                    if (Vy < 0)
                        return Direction.Up_left;

                    if (Vy > 0)
                        return Direction.Down_left;
                }

                return Direction.Error;
            }
        }

        public double Length { get { return Math.Sqrt(Vx * Vx + Vy * Vy); } }

        public double Vx;
        public double Vy;

        public Vector(double vx, double vy)
        {
            Vx = vx; Vy = vy;
        }

        public void Normalise()
        {
            double len = Length;
            Vx /= len; Vy /= len;
        }

        public Vector GetNormal()
        {
            if (Vx == 0 && Vy == 0)
                return new Vector(0, 0);
            if (Vx == 0)
                return new Vector(1, 0);
            if (Vy == 0)
                return new Vector(0, 1);

            //new Vector(1/-Vx, 1/Vy);
            //new Vector(1, -Vx/Vy);
            return new Vector(Vy / Vx, -1);
        }

        public void Reflect(Norm n)
        {
            switch(n)
            {
                case Norm.Vertical:
                    Vy *= -1;
                    break;

                case Norm.Horizontal:
                    Vx *= -1;
                    break;
            }
        }

        static public double operator *(Vector v1, Vector v2)
        {
            return v1.Vx * v2.Vx + v1.Vy * v2.Vy;
        }

        static public Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.Vx + v2.Vx, v1.Vy + v2.Vy);
        }

        static public Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.Vx - v2.Vx, v1.Vy - v2.Vy);
        }

        static public Vector operator -(Vector v)
        {
            return new Vector(-v.Vx, -v.Vy);
        }

        static public Vector operator *(Vector v, double n)
        {
            return new Vector(v.Vx * n, v.Vy * n);
        }

        static public Vector operator /(Vector v, double n)
        {
            return new Vector(v.Vx / n, v.Vy / n);
        }

        static public Vector operator *(Vector v, int n)
        {
            return new Vector(v.Vx * n, v.Vy * n);
        }

        static public Vector operator /(Vector v, int n)
        {
            return new Vector(v.Vx / n, v.Vy / n);
        }

        static public explicit operator Point(Vector v) => new Point(v.Vx, v.Vy);

        static public explicit operator System.Windows.Vector(Vector v) 
            => new System.Windows.Vector(v.Vx, v.Vy);
    }
}
