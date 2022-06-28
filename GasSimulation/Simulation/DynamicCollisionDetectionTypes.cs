using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GasSimulation.Objects;

namespace GasSimulation.Simulation
{
	static public partial class DynamicCollisionDetection
	{
		public enum CollisionType
		{
			Wall,
			Particle,
			NoCollision,
		}

		public class CollisionInfo
		{
			public double TimeToCollision;
			public CollisionType CollisionType;
		}

		public class WallCollisionInfo : CollisionInfo
		{
			public Norm Norm;
			public Particle p;
		}

		public class ParticleCollisionInfo : CollisionInfo
		{
			public Particle p1;
			public Particle p2;
		}
	}
}
