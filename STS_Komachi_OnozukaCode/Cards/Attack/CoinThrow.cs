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

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class CoinThrow : STS_Komachi_OnozukaCard
    {
        public CoinThrow()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
        {
            WithDamage(10, 2);
            WithVar(new GoldVar(5));
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(base.CombatState)
            .WithDanmaku(patterns)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);

            await PlayerCmd.LoseGold(DynamicVars.Gold.BaseValue, Owner);

            if (IsUpgraded)
            {
                CardModel copy = CreateClone();
                copy.AddKeyword(CardKeyword.Exhaust);
                copy.AddKeyword(CardKeyword.Ethereal);
                await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            }
        }

        protected override void OnUpgrade()
        {
            WithTip(CardKeyword.Exhaust);
            WithTip(CardKeyword.Ethereal);
        }

        public override List<DanmakuPiece> patterns =>
            [
                new DanmakuPiece
                {
                    SpritePath = "coin.png".BulletImagePath(),
                    Group = 40,
                    GIntervalSeconds = 0.01f,
                    WayCount = new GrowthValue {Base = 1},
                    GAngle = new GrowthValue(20, -10f),
                    StartSpeed = 14f,
                    Scale = 0.5f,
                    X = 0,
                    LifeSeconds = 3f,
                    BulletColor = StsColors.gold,
                }
            ];
    }
}
