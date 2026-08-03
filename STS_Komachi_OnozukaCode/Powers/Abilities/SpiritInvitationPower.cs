using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Minions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities
{ 
    public class SpiritInvitationPower : STS_Komachi_OnozukaPower, IOnReleasedListener
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(KomachiKeywords.Release)];
        public override Color AmountLabelColor => StsColors.blue;
        public async Task OnReleased(PlayerChoiceContext choiceContext, ReleaseArgs args)
        {
            if (args.creature != Owner || !args.Successful) return;

            // Use the Intended amounts, so it works with Eiki's free release
            int guidedTriggerAmount = args.IntendedGuidedReleaseAmount;
            int divineTriggerAmount = args.IntendedDivineReleaseAmount;

            // Attacks all enemies by the guided released * level
            if (guidedTriggerAmount > 0)
            {
                await CreatureCmd.Damage(choiceContext, 
                    CombatState.HittableEnemies, 
                    guidedTriggerAmount * Amount, 
                    ValueProp.Unpowered, 
                    Owner);
            }

            // Block and summon by divine release * level
            if (divineTriggerAmount > 0)
            {
                await CreatureCmd.GainBlock(Owner, divineTriggerAmount * Amount, ValueProp.Unpowered
                    , null);

                await DivineSpiritCmd.Summon(choiceContext, Owner.Player, divineTriggerAmount * Amount, this);
            }
        }
    }
}
