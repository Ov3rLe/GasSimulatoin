using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasSimulation.Simulation
{
    public struct StraightLine
    {
        public enum StraightLineType
        {
            Vertical,
            Horizontal,
            Arbitrary,
            NotLine,
        }

        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }

        public StraightLineType LineType
        {
            get
            {
                if (A == 0 && B == 0)
                    return StraightLineType.NotLine;

                if (B == 0)
                    return StraightLineType.Vertical;

                if (A == 0)
                    return StraightLineType.Horizontal;

                return StraightLineType.Arbitrary;
            }
        }

        public StraightLine(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        public StraightLine(Point p, Vector v)
        {
            A = v.Vy;
            B = -v.Vx;
            C = v.Vx * p.Y - v.Vy * p.X;
        }

        public double GetX(double y)
        {
            if (A != 0)
                return ((-B) * y - C) / A;
            return double.NaN;
        }

        public double GetY(double x)
        {
            if (B != 0)
                return ((-A) * x - C) / B;
            return double.NaN;
        }
    }
}
