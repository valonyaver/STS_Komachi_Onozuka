using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Potions
{
    public class LilyWhitePotion : STS_Komachi_OnozukaPotion
    {
        public override PotionRarity Rarity => PotionRarity.Rare;

        public override PotionUsage Usage => PotionUsage.AnyTime;

        public override TargetType TargetType => TargetType.AnyPlayer;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(30), new EnergyVar(1)];
        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            AssertValidForTargetedPotion(target);
            NCombatRoom.Instance?.PlaySplashVfx(target, Colors.Red);
            await CreatureCmd.Heal(target, target.MaxHp * DynamicVars.Heal.BaseValue / 100m);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, target.Player);
        }
    }
}
