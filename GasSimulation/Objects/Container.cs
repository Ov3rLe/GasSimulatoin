using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GasSimulation.Simulation;
using GasSimulation.Extensions;
using static GasSimulation.Simulation.MathMisc;
using static GasSimulation.Simulation.DynamicCollisionDetection;

namespace GasSimulation.Objects
{
    public class Container : IEnumerable<Particle>
    {
        public struct PerformanceInfo
        {
            public int FramesInQueue;
            public int CollisionsPerFrame;
        }

        public class ContainerFrame
        {
            public List<Particle> ParticleList;
            public PerformanceInfo PerformanceInfo;
        }

		private struct Coords
		{
			public int x, y;
		}

		private ContainerFrame DefaultFrame
        {
            get
            {
                return new ContainerFrame()
                {
                    ParticleList = ParticleList.DeepCopy(),
                    PerformanceInfo = new PerformanceInfo()
                };
            }
        }

		private Queue<ContainerFrame> FrameQueue;
		private List<Particle> ParticleList;

		public readonly int Width;
		public readonly int Height;
		
        public int Count { get { return ParticleList.Count; } }
        public bool FrameReady { get { return FrameQueue.Count != 0; } }

        public Container(double width, double height)
        {
			Width = (int)width;
			Height = (int)height;

            ParticleList = new List<Particle>();
            FrameQueue = new Queue<ContainerFrame>();
        }

        public void Reset()
        {
            FrameQueue.Clear();
            ParticleList.Clear();
        }

        public void Update()
        {
            /* Queues the current 'ParticleList' for UI to update */

            /* In order for simulation to work accurately, the update works as follows:
             * for each particle all possible collisions are checked (walls and other
             * particles), the nearest (by time) collision is chosen and all particles 
             * are moved by this span of time, while the collision with shortest time
             * is handled.
             * 
             * Since we handle the nearest collision, other particles can be moved freely
             * by the same span, because their time span until collision is bigger.
             * 
             * (Though, in case there are multiple collisions happening at the same time,
             * something might go wrong, which is not accounted for currently. It's safe
             * to assume that the amount of such cases isn't tremendous and from the visual
             * point of view it's not gonna be as noticable, but the simulation is obviously
             * not accurate)
             * 
             * But if we were to do one update of the container and then updated 
             * the screen, each update would have showed different span, which looks
             * like the app stutters. For this reason we add 'timeElapsed' variable
             * which accumulates the total span of time by which all particles were
             * moved. Once the timeElapsed reached some limit, we let the screen get
             * updated.
             * 
             * Obviously it doesn't ensure that each update will show the same span,
             * we just assure that each span is of reasonable size and the difference
             * isn't noticable for the user. */

            // So it doesn't waste all memory for small amounts of particles
            if (FrameQueue.Count > 15)
                return;

            int collisionCount = 0;
            double timeElapsed = 0;
            while (timeElapsed < 1)
            {
				CollisionInfo nearestCollision = GetNearestCollision(this);

                if (nearestCollision.CollisionType == CollisionType.Wall)
                {
                    var wCollisionInfo = nearestCollision as WallCollisionInfo;

                    if (nearestCollision.TimeToCollision <= 1)
                    {
                        ResponceWallCollision(
							nearestCollision.TimeToCollision, wCollisionInfo);

                        foreach (Particle p in ParticleList)
                        {
                            if (ReferenceEquals(p, wCollisionInfo.p))
                                continue;

                            p.Center += p.V * nearestCollision.TimeToCollision;
                        }

                        ++collisionCount;
                    }

                    // If the nearest collision is to some wall and the time time is 
                    // bigger than 1, then no particle will collide on this frame
                    else
                    {
                        // Just move the particle along its velocity vector 
                        foreach (Particle p in ParticleList)
                            p.Center += p.V;

                        timeElapsed += 1;
                    }
                }

                else if (nearestCollision.CollisionType == CollisionType.Particle)
                {
					// No need for t <= 1 check, because the 'GetParticleCollisionInfo'
					// returns a list of *possible* collisions, so all t will be <= 1
					var pCollisionInfo = nearestCollision as ParticleCollisionInfo;

                    ResponceParticleCollision(
						nearestCollision.TimeToCollision, pCollisionInfo);

                    foreach (Particle p in ParticleList)
                    {
                        if (ReferenceEquals(p, pCollisionInfo.p1) ||
                            ReferenceEquals(p, pCollisionInfo.p2))
                            continue;

                        p.Center += p.V * nearestCollision.TimeToCollision;
                    }

                    ++collisionCount;
                }

                timeElapsed += nearestCollision.TimeToCollision;
            }

            FrameQueue.Enqueue(new ContainerFrame()
            {
                ParticleList = ParticleList.DeepCopy(),
                PerformanceInfo = new PerformanceInfo()
                {
                    FramesInQueue = FrameQueue.Count(),
                    CollisionsPerFrame = collisionCount
                }
            });
        }

		public ContainerFrame CurrentFrame() => FrameReady ? FrameQueue.Peek() : DefaultFrame;

		public ContainerFrame NextFrame() => FrameReady ? FrameQueue.Dequeue() : DefaultFrame;

        public void RestoreLatestState()
        {
            /* Returns container to its state after the last update */

            if (FrameReady)
            {
                ParticleList = FrameQueue.Peek().ParticleList.DeepCopy();
                FrameQueue.Clear();
            }
        }

        public void SpawnParticles(int amount, 
			int radiusScatter, int maxSpeed, bool bigParticle, double sizeMultiplyer)
        {
			Random rand = new Random();

			int max = maxSpeed;
			int min = -max;
			double fractVx;
            double fractVy;
            int r;

			if (bigParticle)
            {
                fractVx = maxSpeed == 0 ? 0 : rand.Next(-1000, 1001);
                fractVy = maxSpeed == 0 ? 0 : rand.Next(-1000, 1001);

				Coords coords = GetRandomPos(5 * sizeMultiplyer);
                ParticleList.Add(new Particle(new Point(coords.x, coords.y),
                    new Vector(
						rand.Next(min, max + 1) + fractVx / 1000,
						rand.Next(min, max + 1) + fractVy / 1000),
                    5 * sizeMultiplyer));
                --amount;
            }

            for (; amount > 0; --amount)
            {
                r = rand.Next(6 - radiusScatter, 6);
                fractVx = maxSpeed == 0 ? 0 : rand.Next(-1000, 1001);
                fractVy = maxSpeed == 0 ? 0 : rand.Next(-1000, 1001);

				Coords coords = GetRandomPos(r);
                ParticleList.Add(
                    new Particle(new Point(coords.x, coords.y),
                        new Vector(
							rand.Next(min, max + 1) + fractVx / 1000,
							rand.Next(min, max + 1) + fractVy / 1000),
                        r));
            }
        }

        private Coords GetRandomPos(double r)
        {
            bool AreOverlapped(Particle p1, Particle p2)
                => PointToPointDistance(p1.Center, p2.Center) <= p1.R + p2.R;

			Random rand = new Random();
			Coords coords = new Coords();

        GetNewCoords:
			coords.x = rand.Next(0, Width);
			coords.y = rand.Next(0, Height);

            for (double i = -r; i <= r; ++i)
            {
                for (double j = -r; j <= r; ++j)
                {
                    if (coords.y + i <= r + 1 || coords.y + i >= Height - r - 1 ||
						coords.x + j <= r + 1 || coords.x + j >= Width - r - 1)

                        goto GetNewCoords;
                }
            }

            foreach (Particle p in ParticleList)
            {
                if (AreOverlapped(new Particle(new Point(coords.x, coords.y), r), p))
                    goto GetNewCoords;
            }

            return coords;
        }

        public IEnumerator<Particle> GetEnumerator()
            => ((IEnumerable<Particle>)ParticleList).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable<Particle>)ParticleList).GetEnumerator();

        public Particle this[int i] { get { return ParticleList[i]; } }
    }
}