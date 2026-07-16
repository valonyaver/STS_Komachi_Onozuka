using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Other;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens
{

    [Pool(typeof(TokenCardPool))]
    public class LycorisRadiata : STS_Komachi_OnozukaCard
    {
        public LycorisRadiata() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
        {
            WithDamage(7, 2);
            WithCalculatedVar("PoisonPower", 3, PoisonScaling, 1);
            WithTip(typeof(PoisonPower));
            // Damage increase
            WithVar(nameof(Value1), 2, 1);
            // Poison increase
            WithVar(nameof(Value2), 1, 1);
        }

        public static decimal PoisonScaling(CardModel cardSource, Creature target)
        {
            LycorisRadiataPower power = cardSource.Owner.Creature.GetPower<LycorisRadiataPower>();
            if (power == null) return 0;
            return power.PoisonAmount;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
                .Execute(choiceContext);
            await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);

            await PowerCmd.Apply<LycorisRadiataPower>(choiceContext, Owner.Creature, Value1, Owner.Creature, this);

            LycorisRadiataPower power = Owner.Creature.GetPower<LycorisRadiataPower>();
            if (power == null) return;
            power.PoisonAmount += Value2;
        }
    }
}
