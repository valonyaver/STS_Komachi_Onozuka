using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Configs
{

    public class PrintCardsConsoleCmd : AbstractConsoleCmd
    {
        public override string CmdName => "printcards";

        public override string Args => "[character]";

        public override string Description => "Prints all cards for a specific character pool.";

        public override bool IsNetworked => false;

        public override CmdResult Process(Player? issuingPlayer, string[] args)
        {
            if (args.Length > 0)
            {
                string charName = args[0];
                var character = ModelDb.AllCharacters
                    .FirstOrDefault(c => c.GetType().Name.Equals(charName, StringComparison.OrdinalIgnoreCase));

                if (character != null)
                {
                    var sb = new StringBuilder();

                    foreach (var card in character.CardPool.AllCards)
                    {
                        sb.AppendLine();
                        string cost = card.EnergyCost.CostsX ? "X" : card.EnergyCost.Canonical.ToString();
                        sb.AppendLine($"{card.Title}, {cost} cost, {card.Rarity}: {card.GetDescriptionForPile(PileType.Hand)}");
                    }

                    // Print the entire constructed string once
                    MainFile.LogMessage(sb.ToString().TrimEnd());

                    return new CmdResult(true);
                }
            }

            return new CmdResult(false);
        }

        // Provides Tab-completion candidates in the console
        public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
        {
            if (args.Length == 1)
            {
                // Suggest character class names (e.g., Ironclad, Silent, Regent, etc.)
                var characterNames = ModelDb.AllCharacters.Select(c => c.GetType().Name);
                return CompleteArgument(characterNames, Array.Empty<string>(), args[0]);
            }

            return base.GetArgumentCompletions(player, args);
        }
    }
}
