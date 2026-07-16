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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
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
    public class ShortLifeExpectancy : STS_Komachi_OnozukaCard
    {
        public ShortLifeExpectancy()
            : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
            WithCalculatedDamage(0, GetEnemyHP);
            // Normal Release cost
            WithVar(nameof(Value1), 10, -2);
            // Boss+Elite Release cost
            WithVar(nameof(Value2), 20, -4);
            WithKeyword(KomachiKeywords.Release);
            WithKeyword(CardKeyword.Exhaust);

            WithVar(nameof(ReleaseCost), 10, -2);
        }

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                // Probably change later to check for secondary enemies
                if (IsEliteRoom())
                {
                    return ReleaseCmd.CanReleaseSpirits(Owner.Creature, Value2);
                }
                else return ReleaseCmd.CanReleaseSpirits(Owner.Creature, Value1);
            }

        }


        public static decimal GetEnemyHP(CardModel card, Creature? creature)
        {
            if (creature == null) return 0;
            return creature.CurrentHp / 2;
        }


        public bool IsEliteRoom()
        {
            if (Owner.RunState == null) return false;
            if (Owner.RunState.CurrentRoom == null) return false;

            var room = Owner.RunState.CurrentRoom.RoomType;
            bool isElite = (Owner.RunState.CurrentRoom.RoomType == RoomType.Elite 
                            || Owner.RunState.CurrentRoom.RoomType == RoomType.Boss);
            return isElite;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
            if (CombatState == null) return;


            var isElite = IsEliteRoom() && !cardPlay.Target.IsSecondaryEnemy;
            MainFile.LogMessage($"Is the player in an elite room? {isElite}. The current room is {Owner.RunState.CurrentRoom.RoomType}");
            var releaseCost = isElite ? Value2 : Value1;

            // For extra description purposes
            ReleaseCost = releaseCost;

            CardModel? chosen = await ReleaseCmd.ChooseRelease(choiceContext, this, releaseCost);

            if (ReleaseCmd.ChoseRelease(chosen))
            {
                await ReleaseCmd.Release(choiceContext, Owner.Creature, releaseCost, this);

                await DamageCmd.Attack(GetEnemyHP(this, cardPlay.Target)).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
                .Execute(choiceContext);
            }
        }
    }
}
