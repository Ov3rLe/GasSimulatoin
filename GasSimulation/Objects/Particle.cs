using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GasSimulation.Simulation;

namespace GasSimulation.Objects
{
    public class Particle
    {
        // A line equation representing current trajectory
        public StraightLine TrajectoryLine;
        
        // Center coordinates
        public Point Center;

        // Velocity vector
        public Vector V;

        // Radius and mass
        public double R;
        public double M;

        public bool IsTracked;

        public Particle(Point center, Vector v, double r)
        {
            M = Math.PI * r * r;
            V = v;
            R = r;
            Center = center;
            UpdateTrajectoryLine();
        }

        public Particle(Point center, double r) : this(center, new Vector(0,0), r) { }

        public void UpdateTrajectoryLine() => TrajectoryLine = new StraightLine(Center, V);

        public Particle GetCopy() => new Particle(Center, V, R) { IsTracked = IsTracked, };
           
        public override string ToString()
        {
            return
                $"<" +
                $"X: {Center.X:000.000}; " +
                $"Y: {Center.Y:000.000}; " +
                $"Vx: {V.Vx:+000.000;-000.000;000.000}; " +
                $"Vy: {V.Vy:+000.000;-000.000;000.000}>\n" +
                $"(R: {R} " +
                $"M: {M:#.000})";
        }
    }
}
