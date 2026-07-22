using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MinionLib.Minion;
using MinionLib.Powers;
using STS_Komachi_Onozuka.BaseLibAdapters;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Minions
{
    public sealed class DivineSpiritMinion : CustomMinionModel
    {
        public override float HpBarSizeReduction => 30f;
        public override int MinInitialHp => 1;
        public override int MaxInitialHp => 1;
        protected override string VisualsPath => "minion/divine_spirit.tscn".ScenePath();
        // Does not work at all, for some reason
        //public override string? CustomVisualPath => "minion/divine_spirit.tscn".ScenePath();
        public override async Task OnSummon(
            PlayerChoiceContext choiceContext,
            Player owner,
            MinionSummonOptions options)
        {
            var self = owner.PlayerCombatState?.Pets.FirstOrDefault(p => ReferenceEquals(p.Monster, this));
            if (self == null) return;

            if (options.MaxHp is decimal maxHp)
                await CreatureCmd.SetMaxAndCurrentHp(self, maxHp);

            await PowerCmd.Apply<SpiritBarrierPower>(choiceContext, self, 1m, owner.Creature, options.Source);
        }
    }
}
