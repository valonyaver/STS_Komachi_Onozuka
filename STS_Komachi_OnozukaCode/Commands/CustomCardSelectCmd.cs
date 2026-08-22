using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands
{
    // It's mid
    public static class CustomCardSelectCmd
    {
        public static async Task<CardModel?> FromTargetedCreatureScreen(
            PlayerChoiceContext context,
            IReadOnlyList<CardModel> cards,
            Player player,
            Creature? targetCreature,
            bool canSkip = false)
        {
            if (cards.Count > 5)
            {
                throw new ArgumentException("Only works with 5 or fewer cards", nameof(cards));
            }

            if (cards.Count == 0)
            {
                CardSelectCmd.ReportSoftlock();
                return null;
            }

            CardSelectCmd.UndoEndTurnIfNecessary(player);
            CardModel? result;

            if (CardSelectCmd.Selector != null)
            {
                result = (await CardSelectCmd.Selector.GetSelectedCards(cards, 0, 1)).FirstOrDefault();
            }
            else
            {
                uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
                await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.None);

                if (CardSelectCmd.ShouldSelectLocalCard(player))
                {
                    if (CardSelectCmd.LocalSelector != null)
                    {
                        result = (await CardSelectCmd.LocalSelector.GetSelectedCards(cards, 0, 1)).FirstOrDefault();
                    }
                    else
                    {
                        NPlayerHand.Instance?.CancelAllCardPlay();

                        // 1. Show native screen without custom class cast
                        NChooseACardSelectionScreen? screen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip);

                        if (screen != null)
                        {
                            // 2. Reposition the screen's card container over the targeted creature
                            RepositionScreenOverCreature(screen, targetCreature, cards.Count);

                            // Callable.From(() => HideAllOverlayBackstops(screen)).CallDeferred();

                            if (LocalContext.IsMe(player))
                            {
                                foreach (CardModel card in cards)
                                {
                                    SaveManager.Instance.MarkCardAsSeen(card);
                                }
                            }

                            result = (await screen.CardsSelected()).FirstOrDefault();
                        }
                        else
                        {
                            result = cards.FirstOrDefault(); // Fallback for TestMode
                        }

                        int selectedIndex = cards.IndexOf(result);
                        PlayerChoiceResult choiceResult = PlayerChoiceResult.FromIndex(selectedIndex);
                        RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, choiceResult);
                    }
                }
                else
                {
                    int index = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsIndex();
                    result = (index < 0) ? null : cards[index];
                }

                await context.SignalPlayerChoiceEnded();
            }

            CardSelectCmd.LogChoice(player, new[] { result });
            return result;
        }

        /// <summary>
        /// Adjusts the screen to remove dark overlays/banners and places 
        /// choices directly in the MIDDLE of the creature's hitbox.
        /// </summary>
        private static void RepositionScreenOverCreature(NChooseACardSelectionScreen screen, Creature? targetCreature, int cardCount)
        {
            // 1. Hide dark overlay
            // Callable.From(() => HideAllOverlayBackstops(screen)).CallDeferred();


            // 2. Hide top banner
            if (screen._banner != null) screen._banner.Visible = false;

            // 3. Kill original game tween
            screen._cardTween?.Kill();

            NCreature? creatureNode = targetCreature?.GetCreatureNode();
            if (creatureNode == null || !GodotObject.IsInstanceValid(creatureNode)) return;

            // 4. Center over Creature Hitbox Middle
            Control hitbox = creatureNode.Hitbox;
            Vector2 center = hitbox.GlobalPosition + (hitbox.Size * 0.5f);

            // 5. Scale & Spacing
            float scale = cardCount > 3 ? 0.8f : 0.9f;
            float spacing = cardCount > 3 ? 220f : 280f;

            float totalWidth = (cardCount - 1) * spacing;
            float halfWidth = (totalWidth * 0.5f) + (250f * scale * 0.5f) + 30f;
            Vector2 screenSize = screen.GetViewportRect().Size;

            // Clamp center so choices stay on-screen
            center.X = Mathf.Clamp(center.X, halfWidth, screenSize.X - halfWidth);
            center.Y = Mathf.Clamp(center.Y, 150f, screenSize.Y - 150f);

            screen._cardRow.GlobalPosition = center;

            // 6. CREATE SMOOTH FAN-OUT ANIMATION (SCALING INNER NCARD NODE)
            var holders = screen._cardRow.GetChildren().OfType<NGridCardHolder>().ToList();
            Vector2 startOffset = new Vector2(-totalWidth * 0.5f, 0f);

            screen._cardTween = screen.CreateTween().SetParallel();

            for (int i = 0; i < holders.Count; i++)
            {
                NGridCardHolder holder = holders[i];

                // --- FIX HOVER SIZE POP ---
                // Scale the inner NCard child so the holder's hover animation scales proportionally!
                NCard? nCard = holder.GetChildren().OfType<NCard>().FirstOrDefault();
                if (nCard != null)
                {
                    nCard.PivotOffset = nCard.Size * 0.5f;
                    nCard.Scale = Vector2.One * scale;
                }

                Vector2 targetCardPos = startOffset + new Vector2(i * spacing, 0f);

                // Start cards collapsed at center
                holder.Position = Vector2.Zero;

                // Smoothly animate card sliding outwards to its target position
                screen._cardTween.TweenProperty(holder, "position", targetCardPos, 0.45)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Expo);

                // Smoothly fade card in from black
                screen._cardTween.TweenProperty(holder, "modulate", Colors.White, 0.6)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Cubic)
                    .From(Colors.Black);
            }
        }


        /// <summary>
        /// Hides all dark backstops/overlays inside NOverlayStack except the card selection screen itself.
        /// </summary>
        private static void HideAllOverlayBackstops(NChooseACardSelectionScreen screen)
        {
            if (NOverlayStack.Instance == null) return;

            foreach (Node child in NOverlayStack.Instance.GetChildren())
            {
                if (child != screen && child is CanvasItem canvasItem)
                {
                    canvasItem.Visible = false;
                }
            }
        }
    }
}
