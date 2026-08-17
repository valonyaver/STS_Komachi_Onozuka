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
using MegaCrit.Sts2.Core.ValueProps;
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

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    public class GrudgeOfTheDead : STS_Komachi_OnozukaCard
    {
        public GrudgeOfTheDead()
            : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
        {
            // Spirits applied
            WithPower<VengefulSpiritPower>(nameof(Value1), 4, 3);
            WithPower<StrengthPower>(1);
            WithTip(KomachiKeywords.Detonate);
        }

        public override int? GetVengefulSpiritStacksApplied(Creature target)
        {
            return Value1;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<GrudgeOfTheDeadPower>(choiceContext, Owner.Creature,
                1, Owner.Creature, this);

            if (CombatState == null) return;
            IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
            foreach (Creature enemy in enemies)
            {
                await PowerCmd.Apply<VengefulSpiritPower>(choiceContext, enemy, base.DynamicVars[nameof(Value1)].BaseValue, base.Owner.Creature, this);
                VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_slash");
            }

            if (CombatState.HittableEnemies.Where(e => e.HasPower<VengefulSpiritPower>()).Count() <= 0) return;

            CardModel defuseOption = CombatState.CreateCard<DetonateToken>(Owner);
            ((DetonateToken)defuseOption).PreventsDetonation = true;
            CardModel detonateOption = CombatState.CreateCard<DetonateToken>(Owner);

            CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                new List<CardModel> { defuseOption, detonateOption },
                Owner,
                canSkip: false);

            var chosenOption = chosen as DetonateToken;
            if (chosenOption != null && !chosenOption.PreventsDetonation)
            {
                foreach(var enemy in CombatState.HittableEnemies)
                {
                    await DetonateCmd.Target(choiceContext, enemy, this);
                }
            }
        }
    }
}
