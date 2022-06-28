using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GasSimulation.Objects;
using GasSimulation.Simulation;

namespace GasSimulation.Visuals
{
    enum PResourceKey
    {
        ListPos,
        TrajectoryLine,
        VelocityVector,
    }

    public partial class CanvasField : Canvas
    {
        private List<Ellipse> TrackedParticles;

        private void UpdateAppearance(Particle p, Ellipse pEllipse)
        {
            if (p.IsTracked)
            {
                Children.Remove((UIElement)pEllipse.Resources[PResourceKey.TrajectoryLine]);
                Children.Remove((UIElement)pEllipse.Resources[PResourceKey.VelocityVector]);

                if (ShowTrajectory)
                {
                    Line tl = GetTrajectoryLine(p);
                    Children.Add(tl);
                    pEllipse.Resources[PResourceKey.TrajectoryLine] = tl;
                }

                if (ShowVelocity) 
                {
                    Polyline vl = GetVelocityVector(p);
                    Children.Add(vl);
                    pEllipse.Resources[PResourceKey.VelocityVector] = vl;
                }    

                pEllipse.Fill = Brushes.Red;
            }

            else { }
        }

        private Line GetTrajectoryLine(Particle p)
        {
            SolidColorBrush s = Brushes.Cyan;
            double x1;
            double x2;
            double y1 = 0;
            double y2 = Container.Height - 1;

            if (p.TrajectoryLine.LineType == StraightLine.StraightLineType.NotLine)
            {
                return new Line()
                {
                    Stroke = s,
                    X1 = p.Center.X, Y1 = p.Center.Y,
                    X2 = p.Center.X, Y2 = p.Center.Y,
                }; 
            }

            if (p.TrajectoryLine.LineType == StraightLine.StraightLineType.Horizontal)
            {
                x1 = 0;
                x2 = Container.Width - 1;
                y1 = p.TrajectoryLine.GetY(x1);
                y2 = p.TrajectoryLine.GetY(x2);
            }

            // Vertical or arbitrary line
            else
            {
                x1 = p.TrajectoryLine.GetX(y1);
                x2 = p.TrajectoryLine.GetX(y2);

                if (x1 > Container.Width) 
                {
                    x1 = Container.Width;
                    y1 = p.TrajectoryLine.GetY(x1);
                }

                if (x2 > Container.Width)
                {
                    x2 = Container.Width;
                    y2 = p.TrajectoryLine.GetY(x2);
                }

                if (x1 < 0)
                {
                    x1 = 0;
                    y1 = p.TrajectoryLine.GetY(x1);
                }

                if (x2 < 0)
                {
                    x2 = 0;
                    y2 = p.TrajectoryLine.GetY(x2);
                }
            }

            return new Line()
            {
                Stroke = s,
                X1 = x1, Y1 = y1,
                X2 = x2, Y2 = y2,
            }; 
        }

        private Polyline GetVelocityVector(Particle p)
        {
            System.Windows.Vector nv = new System.Windows.Vector(p.V.Vx, p.V.Vy);
            System.Windows.Vector normal = (System.Windows.Vector)p.V.GetNormal();
            nv.Normalize();
            normal.Normalize();

            double h = p.R / 10;  // arrow tip length
            double w = p.R / 10;  // arrow tip width

            System.Windows.Point p1 = new System.Windows.Point(p.Center.X, p.Center.Y);
            System.Windows.Point p2 = p1 + nv * (p.R + p.V.Length);
            System.Windows.Point p4 = p2;
            System.Windows.Point p6 = p2;

            System.Windows.Point O = p2 - nv * h;
            System.Windows.Point p3 = O + normal * w;
            System.Windows.Point p5 = O + normal * -w;

            return new Polyline()
            {
                StrokeThickness = 2,
                Stroke = Brushes.White,
                Points = new PointCollection() { p1, p2, p3, p4, p5, p6 },
            };
        }

        private void Tracking(object sender, MouseButtonEventArgs e)
        {
            // Making sure click happens when the update has been stopped
            if (ContainerUpdaterThread != null)
            {
                if (ContainerUpdaterThread.IsAlive)
                    return;
            }

            Ellipse pEllipse = sender as Ellipse;
            if (TrackedParticles.Contains(pEllipse))
            {
                Children.Remove((UIElement)pEllipse.Resources[PResourceKey.VelocityVector]);
                Children.Remove((UIElement)pEllipse.Resources[PResourceKey.TrajectoryLine]);
                TrackedParticles.Remove(pEllipse);

                pEllipse.Fill = Brushes.White;
                Container[(int)pEllipse.Resources[PResourceKey.ListPos]].IsTracked = false;
            }

            else
            {
                TrackedParticles.Add(pEllipse);
                Container[(int)pEllipse.Resources[PResourceKey.ListPos]].IsTracked = true;
            }

            Refresh();
        }
    }
}