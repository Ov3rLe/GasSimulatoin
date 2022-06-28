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

namespace GasSimulation.Visuals
{
    public partial class CanvasField : Canvas
    {
        public delegate void InfoUpdateHnadlet(InfoUpdateEventArgs e);

        public class InfoUpdateEventArgs
        {
            public string Text;
        }

        public event InfoUpdateHnadlet ParticleInfoUpdateEvent;

        public event InfoUpdateHnadlet PerformanceInfoUpdateEvent;

        private void UpdateParticleInfo(List<Particle> pList)
        {
            var args = new InfoUpdateEventArgs();

            int count = 0;
            foreach (Ellipse pEllipse in TrackedParticles)
            {
                Particle p = pList[(int)pEllipse.Resources[PResourceKey.ListPos]];
                args.Text = args.Text + $"\np{count++}: " + p.ToString() + '\n';

                // Only 3 particles can be shown
                if (count == 3)
                    break;
            }

            ParticleInfoUpdateEvent(args);
        }

        private void UpdatePerformanceInfo(Container.PerformanceInfo pi)
        {
            PerformanceInfoUpdateEvent(new InfoUpdateEventArgs
            {
                Text = $"Collisions/Frame: {pi.CollisionsPerFrame}, " +
                        $"Frames in queue: {pi.FramesInQueue}"
            });
        }
    }
}
