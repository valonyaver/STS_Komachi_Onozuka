using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class ExchangeLife : STS_Komachi_OnozukaCard
    {
        public override bool CanBeGeneratedInCombat => false;
        public ExchangeLife()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.AnyEnemy)
        {
            WithEnergy(1,1);
            // Self damage.
            WithVar(nameof(Value1), 2);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            if (cardPlay.Target.CurrentHp <= Owner.Creature.MaxHp)
            {
                int targetOriginalHp = cardPlay.Target.CurrentHp;
                int ownerOriginalHp = Owner.Creature.CurrentHp;

                // Calculate absolute changes needed to achieve the switch
                int damageToTarget = targetOriginalHp - ownerOriginalHp;
                int healToOwner = targetOriginalHp - ownerOriginalHp;

                if (damageToTarget > 0)
                {
                    await CreatureCmd.Damage(choiceContext, cardPlay.Target, damageToTarget, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
                }
                // If you play this with higher life, for some reason
                else if (damageToTarget < 0)
                {
                    await CreatureCmd.Heal(cardPlay.Target, Math.Abs(damageToTarget));
                }

                if (healToOwner > 0)
                {
                    await CreatureCmd.Heal(Owner.Creature, healToOwner);
                }
                // If you play this with higher life, for some reason
                else if (healToOwner < 0)
                {
                    await CreatureCmd.Damage(choiceContext, Owner.Creature, Math.Abs(healToOwner), ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
                }
                if (DeckVersion != null)
                {
                    await CardPileCmd.RemoveFromDeck(this.DeckVersion);
                }
                
            }
        }

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            await base.AfterCardDrawn(choiceContext, card, fromHandDraw);
            if (card == this)
            {
                await CreatureCmd.Damage(choiceContext, Owner.Creature, Value1, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, null);
                
                await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            }
        }
    }
}
