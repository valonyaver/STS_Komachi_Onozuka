using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.GameExtenders;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class WindOfPassingAway : STS_Komachi_OnozukaCard, IOnDistanceChangedListener
    {
        public WindOfPassingAway()
            : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
        {
            WithDamage(9, 3);
            // hit count
            WithVar(nameof(Value1), 2);
            WithTip(typeof(DistancePower));
            WithTip(KomachiKeywords.Displace);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).WithHitCount(Value1)
                .TargetingAllOpponents(base.CombatState)
                .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
                .Execute(choiceContext);
        }

        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card != this) return Task.CompletedTask;
            if (base.IsClone) return Task.CompletedTask;

            int amount = CombatManager.Instance.History.Entries
                .OfType<DisplacementEntry>()
                .Where(e => e.HappenedThisTurn(base.CombatState))
                .Sum(e => e.ChangeAbs);
            ReduceCostBy(amount);
            return Task.CompletedTask;
        }

        public Task OnDistanceChanged(PlayerChoiceContext choiceContext, DistanceChangedEventArgs args)
        {
            ReduceCostBy(args.ChangeAbs);
            return Task.CompletedTask;
        }

        public void ReduceCostBy(int amount)
        {
            base.EnergyCost.AddThisTurn(-amount);
        }
    }
}
