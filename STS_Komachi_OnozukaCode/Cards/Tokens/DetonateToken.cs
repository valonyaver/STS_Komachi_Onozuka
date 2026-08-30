using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens
{
    [Pool(typeof(TokenCardPool))]
    public class DetonateToken : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;
        public DetonateToken() : base(0, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
        {
            WithKeywords(CardKeyword.Retain, CardKeyword.Exhaust, KomachiKeywords.Detonate);
            WithVar(new IntVar(nameof(PreventsDetonation), 1));
        }
        public bool PreventsDetonation
        {
            get => DynamicVars[nameof(PreventsDetonation)].BaseValue != 1m;
            set => DynamicVars[nameof(PreventsDetonation)].BaseValue = value ? 2m : 1m;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var target = cardPlay.Target;
            if (target!= null)
            {
                await DetonateCmd.Target(choiceContext, target, this);
            }
            if (IsUpgraded)
            {
                await CardPileCmd.Draw(choiceContext, 1, Owner);
            }
        }

        // It just makes you draw when played
        protected override void OnUpgrade()
        {
            base.OnUpgrade();
        }

        public static async Task<CardModel?> GetDetonateOption(
        PlayerChoiceContext choiceContext,
        ICombatState combatState,
        Player player,
        Creature target,
        CardModel? sourceCard = null)
        {
            if (!target.HasPower<VengefulSpiritPower>()) return null;

            DetonateToken defuseOption = combatState.CreateCard<DetonateToken>(player);
            defuseOption.PreventsDetonation = true;
            defuseOption.RemoveKeyword(CardKeyword.Exhaust);
            defuseOption.RemoveKeyword(CardKeyword.Retain);


            DetonateToken detonateOption = combatState.CreateCard<DetonateToken>(player);
            detonateOption.RemoveKeyword(CardKeyword.Exhaust);
            detonateOption.RemoveKeyword(CardKeyword.Retain);
            if (sourceCard is STS_Komachi_OnozukaCard komachiCard)
            {
                detonateOption.AltDescription = 1;
                var extraText = komachiCard.RawExtraDescription1.GetFormattedText();
                detonateOption.ExtraDescription1 = extraText;
            }

            CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                new List<CardModel> { defuseOption, detonateOption },
                player,
                canSkip: false);

            return chosen;
        }
    }
}
