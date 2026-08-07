using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens
{

    [Pool(typeof(TokenCardPool))]
    public class SpiderLily : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;
        public SpiderLily() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
        {
            WithKeywords(CardKeyword.Retain, CardKeyword.Exhaust, KomachiKeywords.Replenish);
            WithVar(nameof(Value1), 1, 2);
            // Make sure to sync with Riverside View
            WithPower<PoisonPower>(nameof(Value2), 2);
            WithEnergy(1, 1);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            await PowerCmd.Apply<SpiderLilyPower>(choiceContext, Owner.Creature, Value1, Owner.Creature, this);
            await PowerCmd.Apply<PoisonPower>(choiceContext, Owner.Creature, Value2, Owner.Creature, this);
        }
    }
}
