using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GasSimulation.Objects;

namespace GasSimulation.Extensions
{
    public static class ParticleListExtensions
    {
        public static List<Particle> DeepCopy(this List<Particle> list)
        {
            var newList = new List<Particle>(list.Count);
            foreach (var item in list)
                newList.Add(item.GetCopy());
            return newList;
        }
    }
}
