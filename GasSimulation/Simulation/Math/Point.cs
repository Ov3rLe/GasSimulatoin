using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasSimulation.Simulation
{
    public struct Point
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        static public Point operator +(Point p, Vector v)
        {
            return new Point(p.X + v.Vx, p.Y + v.Vy);
        }

        static public Point operator -(Point p, Vector v)
        {
            return new Point(p.X - v.Vx, p.Y - v.Vy);
        }

        static public Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        static public Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        static public explicit operator Vector(Point p) => new Vector(p.X, p.Y);
    }
}
