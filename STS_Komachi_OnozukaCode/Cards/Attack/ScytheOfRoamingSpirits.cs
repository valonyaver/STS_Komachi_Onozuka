using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Powers.Spirits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack
{
      
    public class ScytheOfRoamingSpirits : STS_Komachi_OnozukaCard
    {
        public ScytheOfRoamingSpirits()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
            // Damage increased with spirits
            WithPower<VengefulSpiritPower>(nameof(Value1), 1, upgrade: 1);
            // WithKeywords(CardKeyword.Exhaust, CardKeyword.Retain);

            WithCalculatedDamage(14, 
                static (card, target) =>
                (target?.GetPowerAmount<VengefulSpiritPower>() ?? 0) * card.DynamicVars[nameof(Value1)].BaseValue, 
                upgrade:3);
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            decimal spiritAmount = cardPlay.Target.GetPowerAmount<VengefulSpiritPower>();

            var damageResult = await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
                                                .FromCard(this, cardPlay)
                                                .Targeting(cardPlay.Target)
                                                .WithHitFx("vfx/vfx_attack_slash")
                                                .Execute(choiceContext);

            bool killedTarget = damageResult.Results
                .SelectMany(r => r)
                .Any(r => r.WasTargetKilled);

            if (killedTarget && spiritAmount > 0 && CombatState != null)
            {
                foreach (Creature enemy in CombatState.Enemies)
                {
                    if (enemy == cardPlay.Target || enemy.IsDead)
                    {
                        continue;
                    }

                    await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, spiritAmount, Owner.Creature, this);
                }
            }
        }

        public override List<DanmakuPiece> patterns => [

            ];
    }
}
