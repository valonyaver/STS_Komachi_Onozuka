using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions
{
    public class RemoveFromCombatCardModifier : CardModifier
    {
        public override void ModifyDescriptionPost(Creature? target, ref string description)
        {
            description += "\n" + GetLoc().GetFormattedText();
        }

        // Runs before the normal AfterCardPlayed hook, so this is guaranteed to fire
        // exactly once, right after the card's own OnPlay resolves.
        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.RemoveFromCombat(cardPlay.Card);
        }
    }
}
