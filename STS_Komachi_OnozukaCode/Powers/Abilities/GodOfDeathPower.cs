using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Minions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{ 
    public class GodOfDeathPower : STS_Komachi_OnozukaPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
            new BoolVar(nameof(HasRevived), false),
            new IntVar(nameof(Value1), 50)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(KomachiKeywords.Barrier)];

        public CardModel applyingCard;

        public bool HasRevived
        {
            get
            {
                return ((BoolVar)DynamicVars[nameof(HasRevived)]).BoolVal;
            }
            set
            {
                ((BoolVar)DynamicVars[nameof(HasRevived)]).BoolVal = value;
                if (value) DynamicVars[nameof(HasRevived)].BaseValue = 1;
                else DynamicVars[nameof(HasRevived)].BaseValue = 0;
            }
        }

        public override bool ShouldDie(Creature creature)
        {
            if (Owner != creature || HasRevived)
            {
                return true;
            }
            return false;
        }

        public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            if (cardSource == null) return;
            if (cardSource.DeckVersion != null)
            {
                applyingCard = cardSource.DeckVersion;
            }
            HasRevived = false;
        }

        public override async Task AfterPreventingDeath(Creature creature)
        {
            HasRevived = true;
            InvokeDisplayAmountChanged();
            await CreatureCmd.Heal(creature, Math.Max((decimal)creature.MaxHp * Value1/100m, 1m));
            await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), creature, 1, Owner, null);
            if (applyingCard != null && Owner.Player.Deck.Cards.Contains(applyingCard))
            {
                await CardPileCmd.RemoveFromDeck(applyingCard, false);
            }
        }

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (creature.Side == CombatSide.Enemy)
            {
                if (creature.IsSecondaryEnemy)
                {
                    await DivineSpiritCmd.Summon(choiceContext, Owner.Player, Amount, this);
                }
                else if (creature.IsPrimaryEnemy)
                {
                    await CreatureCmd.Heal(Owner, Math.Max(Amount, 1m));
                }
            }
        }
    }
}
