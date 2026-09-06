using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Distance;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
    public class GhostrickShot : STS_Komachi_OnozukaCard
    {
        public GhostrickShot()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
        {
            // Honestly just a triangle anti tech
            WithDamage(2);
            // Single target apply
            WithPower<VengefulSpiritPower>(nameof(Value1), 8, 2);
            // On exhaust aoe apply
            WithPower<VengefulSpiritPower>(nameof(Value2), 2, 1);

            WithKeyword(CardKeyword.Ethereal);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;
            //var container = cardPlay.Target.GetVfxContainer();
            //if (container != null)
            //{
            //    await DanmakuCmd.FireAndWaitForHit(
            //            patterns,
            //            Owner.Creature,
            //            [cardPlay.Target],
            //            cardPlay.Target.GetVfxContainer(),
            //            timeoutSeconds: 2);
            //}
            //await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            //.WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            //.WithDanmaku(patterns)
            //.Execute(choiceContext);

            await PowerCmd.Apply<VengefulSpiritPower>(
                choiceContext, cardPlay.Target,
                Value1, Owner.Creature, this);

        }

        public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
        {
            if (card == this)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(
                choiceContext, CombatState.HittableEnemies,
                Value2, Owner.Creature, this);
            }
        }

        public override List<DanmakuPiece> patterns => 
            [
            
            ];
    }
}
