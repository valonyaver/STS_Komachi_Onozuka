using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics
{
    public class YamasLectureBook : STS_Komachi_OnozukaRelic, IOnReleasedListener
    {
        public override RelicRarity Rarity => RelicRarity.Shop;
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [
                HoverTipFactory.FromKeyword(KomachiKeywords.Release)
            ];

        public bool _usedThisTurn;

        public bool UsedThisTurn
        {
            get
            {
                return _usedThisTurn;
            }
            set
            {
                AssertMutable();
                _usedThisTurn = value;
            }
        }

        public async Task OnReleased(PlayerChoiceContext choiceContext, ReleaseArgs args)
        {
            if (UsedThisTurn) return;
            if (args.creature != Owner.Creature || !args.Successful) return;

            // Use the Intended amounts, so it works with Eiki's free release
            int guidedTriggerAmount = Mathf.CeilToInt(args.IntendedGuidedReleaseAmount/2f);
            int divineTriggerAmount = Mathf.CeilToInt(args.IntendedDivineReleaseAmount/2f);

            if (guidedTriggerAmount > 0)
            {
                await PowerCmd.Apply<GuidedSpiritPower>(choiceContext, Owner.Creature, guidedTriggerAmount, Owner.Creature, null);
            }
            if (divineTriggerAmount > 0)
            {
                await PowerCmd.Apply<DivineSpiritPower>(choiceContext, Owner.Creature, divineTriggerAmount, Owner.Creature, null);
            }
            Flash();
            UsedThisTurn = true;
        }

        public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (!participants.Contains(base.Owner.Creature))
            {
                return Task.CompletedTask;
            }

            UsedThisTurn = false;
            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            UsedThisTurn = false;
            return Task.CompletedTask;
        }
    }
}
