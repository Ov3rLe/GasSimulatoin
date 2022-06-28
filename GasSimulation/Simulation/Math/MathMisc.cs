using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasSimulation.Simulation
{
    public enum Norm
    {
        None,
        Vertical,
        Horizontal,
    }

    static public class MathMisc
    {
        static public int Sign(double n) => n >= 0 ? 1 : -1;

        static public double PointToPointDistance(Point p1, Point p2)
            => Math.Sqrt(
                (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));

        static public double PointToLineDistance(Point p, StraightLine l)
        {
            if (l.A != 0 || l.B != 0)
                return Math.Abs(l.A * p.X + l.B * p.Y + l.C) /
                            Math.Sqrt(l.A * l.A + l.B * l.B);
            return -1;
        }
    }
}
