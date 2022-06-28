using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GasSimulation.Objects;
using static GasSimulation.Simulation.MathMisc;

namespace GasSimulation.Simulation
{
	static public partial class DynamicCollisionDetection
	{
		static private void GetSignedDistanceLeftWall
			(Particle p, out double Sc, out double Se)
		{
			Sc = p.Center.X;
			Se = p.Center.X + p.V.Vx;
		}

		static private void GetSignedDistanceTopWall
			(Particle p, out double Sc, out double Se)
		{
			Sc = p.Center.Y;
			Se = p.Center.Y + p.V.Vy;
		}

		static private void GetSignedDistanceRightWall
			(Particle p, double containerWidth, out double Sc, out double Se)
		{
			Sc = containerWidth - 1 - p.Center.X;
			Se = containerWidth - 1 - (p.Center.X + p.V.Vx);
		}

		static private void GetSignedDistanceBottomWall
			(Particle p, double containerHeight, out double Sc, out double Se)
		{
			Sc = containerHeight - 1 - p.Center.Y;
			Se = containerHeight - 1 - (p.Center.Y + p.V.Vy);
		}

		static private void GetWallCollisionPointsX
			(Particle p, double y1, double y2, out double x1, out double x2)
		{
			/* Line equation: (x - sign(Vx)*r - x0) / Vx = (y - sign(Vy)*r - y0) / Vy,
             * solved for x where Vx != 0 and Vy != 0.
             * 
             * Here we account for the radius of the particle, meaning we 
             * find the X'es where the nearest point of the particle
             * touches the wall */

			// Signed radius
			double Srx = Sign(p.V.Vx) * p.R;
			double Sry = Sign(p.V.Vy) * p.R;

			x1 = Srx + p.Center.X + (p.V.Vx / p.V.Vy) * (y1 - Sry - p.Center.Y);
			x2 = Srx + p.Center.X + (p.V.Vx / p.V.Vy) * (y2 - Sry - p.Center.Y);
		}

		static public CollisionInfo GetNearestCollision(Container c)
		{
			CollisionInfo currCollision = new CollisionInfo();
			CollisionInfo nearestCollision = new CollisionInfo()
			{
				CollisionType = CollisionType.NoCollision,
				TimeToCollision = double.PositiveInfinity
			};

			int count = 0;
			foreach (Particle p in c)
			{
				currCollision = GetWallCollisionInfo(p, c);

				if (currCollision.TimeToCollision < nearestCollision.TimeToCollision)
					nearestCollision = currCollision;

				currCollision = GetParticleCollisionInfo(p, c, count);

				if (currCollision.TimeToCollision < nearestCollision.TimeToCollision)
					nearestCollision = currCollision;

				++count;
			}

			return nearestCollision;
		}

		static public void ResponceWallCollision
			(double timeToCollision, WallCollisionInfo collisionInfo)
		{
			/* Wall collision responce - elastic bounce.
             * 
             * The velocity vector of a particle is reflected
             * around the plane's normal */

			collisionInfo.p.Center += collisionInfo.p.V * timeToCollision;
			collisionInfo.p.V.Reflect(collisionInfo.Norm);
			collisionInfo.p.UpdateTrajectoryLine();
		}

		static public void ResponceParticleCollision
			(double timeToCollision, ParticleCollisionInfo collisionInfo)
		{
			/* Particle collision responce - elastic bounce.
             * 
             * The colliding particles are moved to the spot 
             * of collision, then their velocity vectors change */

			Particle p1 = collisionInfo.p1;
			Particle p2 = collisionInfo.p2;

			// Move
			p1.Center += p1.V * timeToCollision;
			p2.Center += p2.V * timeToCollision;

			// Responce
			Vector x1 = (Vector)p1.Center;
			Vector x2 = (Vector)p2.Center;
			Vector nv1, nv2;

			nv1 = p1.V - (x1 - x2) * (2 * p2.M / (p1.M + p2.M)) *
				((p1.V - p2.V) * (x1 - x2) / ((x1 - x2).Length * (x1 - x2).Length));

			nv2 = p2.V - (x2 - x1) * (2 * p1.M / (p1.M + p2.M)) *
				((p2.V - p1.V) * (x2 - x1) / ((x2 - x1).Length * (x2 - x1).Length));

			p1.V = nv1;
			p2.V = nv2;

			p1.UpdateTrajectoryLine();
			p2.UpdateTrajectoryLine();
		}

		static private WallCollisionInfo GetWallCollisionInfo(Particle p, Container c)
		{
			/* Wall collision with a time > 1 means 
			 * no collision for the current frame */

			double Sc, Se;
			double GetCollisionTime() => (Sc - p.R) / (Sc - Se);

			WallCollisionInfo GetCollisionInfo(Norm n)
			{
				return new WallCollisionInfo()
				{
					CollisionType = CollisionType.Wall,
					TimeToCollision = GetCollisionTime(),

					Norm = n,
					p = p
				};
			}

			// Solve if one of the components is 0
			switch (p.V.Direction)
			{
				case Direction.Static:
					return new WallCollisionInfo()
					{
						CollisionType = CollisionType.NoCollision,
						TimeToCollision = double.PositiveInfinity,

						Norm = Norm.None,
						p = p
					};

				case Direction.Left:
					GetSignedDistanceLeftWall(p, out Sc, out Se);
					return GetCollisionInfo(Norm.Horizontal);

				case Direction.Up:
					GetSignedDistanceTopWall(p, out Sc, out Se);
					return GetCollisionInfo(Norm.Vertical);

				case Direction.Right:
					GetSignedDistanceRightWall(p, c.Width, out Sc, out Se);
					return GetCollisionInfo(Norm.Horizontal);

				case Direction.Down:
					GetSignedDistanceBottomWall(p, c.Height, out Sc, out Se);
					return GetCollisionInfo(Norm.Vertical);
			}

			// Solve if neither component is 0
			int w = c.Width - 1;
			int h = c.Height - 1;

			GetWallCollisionPointsX(p, 0, h, out double x1, out double x2);

			/* Knowing where the trajectory line crosses upper and bottom walls
             * tells us which wall the particle will bounce off of */

			// Left wall
			if ((x1 <= 0 || x2 <= 0) && p.V.Vx < 0)
			{
				GetSignedDistanceLeftWall(p, out Sc, out Se);
				return GetCollisionInfo(Norm.Horizontal);
			}

			// Top wall
			else if (x1 >= 0 && x1 <= w && p.V.Vy < 0)
			{
				GetSignedDistanceTopWall(p, out Sc, out Se);
				return GetCollisionInfo(Norm.Vertical);
			}

			// Right wall
			else if ((x1 >= w || x2 >= w) && p.V.Vx > 0)
			{
				GetSignedDistanceRightWall(p, c.Width, out Sc, out Se);
				return GetCollisionInfo(Norm.Horizontal);
			}

			// Bottom wall. In case something breaks, the actual check is 
			// (x2 >= 0 && x2 <= w && p.V.Vy > 0)
			else
			{
				GetSignedDistanceBottomWall(p, c.Height, out Sc, out Se);
				return GetCollisionInfo(Norm.Vertical);
			}
		}

		static private ParticleCollisionInfo GetParticleCollisionInfo
			(Particle p, Container c, int pListPos)
		{
			/* Returns nearest collision with a given particle */

			int pToBeSavedIndex = 0;
			var nearestCollision = new ParticleCollisionInfo()
			{
				CollisionType = CollisionType.NoCollision,
				TimeToCollision = double.PositiveInfinity,
			};

			/* Iteration starts with the given particle, instead of the beginning 
             * of the list, because the collisions with previous particles in the
             * list were already checked at this point */

			// pListPos + 1 because a particle cannot collide with itself
			for (int pIndex = pListPos + 1; pIndex < c.Count; ++pIndex)
			{
				Particle p2 = c[pIndex];

				/* Collision of two particles can be thought of a collision of
                 * a point and a stationary particle of radius p1.R + p2.R. 
                 * 
                 * To tell whether a point and a stationary particle collide, 
                 * we do 3 steps:
                 * 
                 *  1. Check if a particle lies on a trajectory line of a point.
                 *  2. Check if the velocity vector of a point is big enough
                 * to reach the particle.
                 *  3. Check if the point moves towards the particle along its
                 * trajectory. 
                 *
                 * If one of those conditinons fails, a collision cannot occur. */

				// Point's velocity 
				Vector Vab = p.V - p2.V;

				// Check if velocity vector of a stationary point is less 
				// than the distance between the two particles. If the 
				// distance is bigger, then a collision cannot occur
				if (PointToPointDistance(
					p.Center, p2.Center) - (p.R + p2.R) > Vab.Length)
					continue;

				// If trajectory line doesn't cross or touch the stationary particle,
				// then a collision cannot happen.
				if (PointToLineDistance(
					p2.Center, new StraightLine(p.Center, Vab)) > p.R + p2.R)
					continue;

				/* Distance vector is the vector which goes from the center
                 * of the particle to the center of the other particle.
                 * 
                 * We know that the both particles lie on a move trajectory,
                 * defined by Vab vector, so the only info left that we need
                 * is whether the points moves towards the particle or away
                 * from it. 
                 * 
                 * For that we take a dot product of the distance and Vab vectors. */

				// Negating the vector so it's correct for the canvas coordinate system
				Vector dVector = (Vector)p.Center - (Vector)p2.Center;
				if (Vab * (-dVector) <= 0)
					continue;

				// Finding time until collision
				double A = Vab * Vab;
				double B = dVector * Vab * 2;
				double C = dVector * dVector - (p.R + p2.R) * (p.R + p2.R);
				double Q = -(B + Sign(B) * Math.Sqrt(B * B - 4 * A * C)) / 2;

				double t0 = Q / A;
				double t1 = C / Q;

				// We don't need to check the roots for being in interval [0, 1],
				// because at this point we're certain the particles will collide
				double t = Math.Min(t0, t1);
				if (t < nearestCollision.TimeToCollision)
				{
					pToBeSavedIndex = pIndex;
					
					nearestCollision.CollisionType = CollisionType.Particle;
					nearestCollision.TimeToCollision = t;
				}
			}

			nearestCollision.p1 = p;
			nearestCollision.p2 = c[pToBeSavedIndex];

			return nearestCollision;
		}
	}
}