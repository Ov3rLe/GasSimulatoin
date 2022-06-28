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

using System.Threading;

using GasSimulation.Objects;

namespace GasSimulation.Visuals
{
    public partial class CanvasField : Canvas
    {
        private CancellationTokenSource UpdateCancellation;
        private Thread ContainerUpdaterThread;
        private Container Container;

        // Initial settings
        public bool ShowTrajectory = true;
        public bool ShowVelocity = true;
        public bool HideNotTracked = false;
        public bool BigParticle = false;

        public int ParticleAmount = 400;
        public int SizeScatter = 1;
        public int MaxSpeed = 5;
        public double SizeMultiplyer = 20;

        public CanvasField()
        {
            base.SizeChanged += (o, e) =>
            {
                Container = new Container(
                    e.NewSize.Width,  e.NewSize.Height);
                ResetField();
            };

            TrackedParticles = new List<Ellipse>();
            UpdateCancellation = new CancellationTokenSource();
        }

        public void ResetField()
        {
            Container.Reset();
            Container.SpawnParticles(
                ParticleAmount,
                SizeScatter,
                MaxSpeed,
                BigParticle,
                SizeMultiplyer);   
            
            InitialiseField();
            Update();
        }

        public void StartContainerUpdate()
        {
            ContainerUpdaterThread = new Thread(() =>
            {
                while (true)
                {
                    Container.Update();
                    if (UpdateCancellation.IsCancellationRequested)
                        return;
                }
            });

            ContainerUpdaterThread.Start();
        }

        public void StopContainerUpdate()
        {
            UpdateCancellation.Cancel();
            ContainerUpdaterThread?.Join();
            Container.RestoreLatestState();
            Refresh();

            // Reset cancellation
            UpdateCancellation = new CancellationTokenSource();
        }

        public void Refresh()
        {
            _update(false);
        }

        public void Update()
        {
            if (Container.FrameReady)
				_update(true);
		}

        public void UpdateFrame()
        {
            Container.Update();
            _update(true);
        }

        private void _update(bool nextFrame)
        {
            Container.ContainerFrame currentState
				= nextFrame 
					? Container.NextFrame() 
					: Container.CurrentFrame();

            List <Particle> curParticleList = currentState.ParticleList;
            for (int pListPos = 0; pListPos < curParticleList.Count; ++pListPos)
            {
                Particle p = curParticleList[pListPos];

                // First n children of canvas are always ellipses,
                // becuase once they're put on canvas they're never removed
                Ellipse pEllipse = (Ellipse)Children[pListPos];

                UpdateAppearance(p, pEllipse);

                SetTop(pEllipse, p.Center.Y - p.R);
                SetLeft(pEllipse, p.Center.X - p.R);

                pEllipse.Fill = HideNotTracked ? Brushes.Black : Brushes.White;

                if (p.IsTracked)
                    pEllipse.Fill = Brushes.Red;
            }

            UpdateParticleInfo(currentState.ParticleList);
            UpdatePerformanceInfo(currentState.PerformanceInfo);
        }

        private void InitialiseField()
        {
            TrackedParticles.Clear();
            Children.Clear();
            int count = 0;
            foreach (Particle p in Container)
            {
                Ellipse pEllipse = GetParticleEllipse(p);
                Children.Add(pEllipse);

                pEllipse.MouseDown += Tracking;
                pEllipse.Resources[PResourceKey.ListPos] = count++;

                SetTop(pEllipse, p.Center.Y - p.R);
                SetLeft(pEllipse, p.Center.X - p.R);
            }
        }

        private Ellipse GetParticleEllipse(Particle p)
        {
            return new Ellipse()
            {
                Width = p.R * 2, Height = p.R * 2,
                Fill = p.IsTracked ? Brushes.Red 
                    : Brushes.White,
            };
        }
    }
}