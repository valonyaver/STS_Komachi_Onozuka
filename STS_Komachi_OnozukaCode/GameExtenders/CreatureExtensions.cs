using MegaCrit.Sts2.Core.Entities.Creatures;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras
{
    public static class CreatureExtensions
    {
        /// <summary>
        /// Returns a creature's current Distance level, or DefaultLevel (3) if it has no DistancePower yet.
        /// </summary>
        public static int GetDistanceLevel(this Creature creature)
        {
            return creature.GetPower<DistancePower>()?.Amount ?? DistancePower.DefaultLevel;
        }
    }
}
