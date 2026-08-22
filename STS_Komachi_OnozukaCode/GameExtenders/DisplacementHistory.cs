using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.GameExtenders
{
    public class DisplacementEntry : CombatHistoryEntry
    {
        public Creature Target { get; }
        public int OldLevel { get; }
        public int NewLevel { get; }
        public Creature? Applier { get; }
        public CardModel? CardSource { get; }

        public int Change => NewLevel - OldLevel;
        public int ChangeAbs => Math.Abs(Change);

        public override string Description =>
            $"{GetId(Target)} displaced from {OldLevel} to {NewLevel}" +
            (Applier != null ? $" by {GetId(Applier)}" : "");

        public DisplacementEntry(Creature target, int oldLevel, int newLevel, Creature? applier, CardModel? cardSource,
            int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
            : base(applier ?? target, roundNumber, currentSide, history, players)
        {
            Target = target;
            OldLevel = oldLevel;
            NewLevel = newLevel;
            Applier = applier;
            CardSource = cardSource;
        }

        private static string GetId(Creature creature) =>
            creature.IsPlayer ? creature.Player.Character.Id.Entry : creature.Monster.Id.Entry;
    }
}
