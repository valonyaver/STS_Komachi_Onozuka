using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extras
{
    public class ReplenishReactor : CustomSingletonModel
    {
        public ReplenishReactor() : base(HookType.Combat)
        {
        }


        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Keywords.Contains(KomachiKeywords.Replenish))
            {
                await CardPileCmd.Draw(choiceContext, card.Owner);
            }
        }
    }
}
