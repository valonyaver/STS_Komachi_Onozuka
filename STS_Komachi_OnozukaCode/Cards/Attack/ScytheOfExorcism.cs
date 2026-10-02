using BaseLib.Utils;
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
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Abilities;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class ScytheOfExorcism : STS_Komachi_OnozukaCard
    {
        public ScytheOfExorcism()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
            WithDamage(14, 4);
            WithKeyword(KomachiKeywords.Detonate);

            // Spirits needed
            WithPower<VengefulSpiritPower>(nameof(Value1), 6);
            WithTip(new TooltipSource((c) =>
                HoverTipFactory.FromCard<SpiderLily>(true)));

            // Release cost
            // WithVar(nameof(ReleaseCost), 4, -1);
            // WithKeyword(KomachiKeywords.Release);
            // WithTip(typeof(ArtifactPower));
        }

        //public override int? GetVengefulSpiritStacksApplied(Creature target)
        //{
        //    if (ReleaseCmd.CanReleaseSpirits(Owner.Creature, ReleaseCost))
        //    {
        //        return Value1;
        //    }
        //    return 0;
        //}

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).
                FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
                .Execute(choiceContext); 
            
            var detonate = await DetonateCmd.Target(choiceContext, cardPlay.Target, this);
            if (detonate?.TotalCountedAmount >= Value1 && CombatState != null)
            {
                CardModel lily = CombatState.CreateCard<SpiderLily>(Owner);
                CardCmd.Upgrade(lily);
                await CardPileCmd.AddGeneratedCardToCombat(lily, PileType.Hand, Owner);
            }
        }

        public override List<DanmakuPiece> patterns => [

            ];
    }
}
