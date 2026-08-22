using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ICardSelector = MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.ICardSelector;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Selection;


public partial class NTargetedCreatureCardSelectScreen : Control, IOverlayScreen, IScreenContext, ICardSelector
{
    public const float BaseCardXSpacing = 240f; // Adjusted so 5 cards fit nicely
    public const ulong NoSelectionTimeMsec = 350uL;

    public NCommonBanner _banner;
    public Control _cardRow;
    public NChoiceSelectionSkipButton _skipButton;
    public NCombatPilesContainer _combatPiles;
    public Control _inspectPrompt;
    public NPeekButton _peekButton;

    public readonly TaskCompletionSource<IEnumerable<CardModel>> _completionSource = new();
    public ulong _openedTicks;
    public bool _canSkip;
    public Creature? _targetCreature;
    public IReadOnlyList<CardModel> _cards = Array.Empty<CardModel>();

    public Tween? _cardTween;
    public Tween? _fadeTween;

    public NetScreenType ScreenType => NetScreenType.CardSelection;
    public bool UseSharedBackstop => true;

    public Control DefaultFocusedControl
    {
        get
        {
            if (_peekButton.IsPeeking && NCombatRoom.Instance != null)
            {
                return NCombatRoom.Instance.DefaultFocusedControl;
            }

            var list = _cardRow.GetChildren().OfType<NGridCardHolder>().ToList();
            return list.Count > 0 ? list[list.Count / 2] : this;
        }
    }

    public override void _Ready()
    {
        _banner = GetNode<NCommonBanner>("Banner");
        _banner.label.SetTextAutoSize(new LocString("gameplay_ui", "CHOOSE_CARD_HEADER").GetRawText());
        _banner.AnimateIn();

        _cardRow = GetNode<Control>("CardRow");
        _combatPiles = GetNode<NCombatPilesContainer>("%CombatPiles");

        if (CombatManager.Instance.IsInProgress && _cards.Count > 0)
        {
            _combatPiles.Initialize(_cards.First().Owner);
        }

        _combatPiles.Disable();
        _combatPiles.Visible = false;
        _inspectPrompt = GetNode<Control>("%InspectPrompt");

        // --- Calculate Position Centered Over Target Creature ---
        Vector2 centerPos = GetSelectionCenterPosition();

        int cardCount = _cards.Count;
        float spacing = cardCount > 3 ? 240f : 340f; // Dynamic spacing for up to 5 cards
        Vector2 startOffset = new Vector2(-(cardCount - 1) * spacing * 0.5f, 0f);

        _cardTween = CreateTween().SetParallel();

        for (int i = 0; i < cardCount; i++)
        {
            CardModel card = _cards[i];
            NCard nCard = NCard.Create(card);
            NGridCardHolder holder = NGridCardHolder.Create(nCard);
            _cardRow.AddChildSafely(holder);

            holder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(SelectHolder));
            holder.Connect(NCardHolder.SignalName.AltPressed, Callable.From<NCardHolder>(OpenPreviewScreen));

            nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
            holder.Scale = cardCount > 3 ? holder.SmallScale * 0.85f : holder.SmallScale;

            // Slight arc curve for aesthetics
            float arcY = -Mathf.Pow(i - (cardCount - 1) / 2.0f, 2f) * 10f;
            Vector2 targetPos = centerPos + startOffset + new Vector2(i * spacing, arcY);

            _cardTween.TweenProperty(holder, "global_position", targetPos, 0.45)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Expo);

            _cardTween.TweenProperty(holder, "modulate", Colors.White, 0.45)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic)
                .From(Colors.Black);

            nCard.ActivateRewardScreenGlow();
        }

        // Setup Skip Button
        _skipButton = GetNode<NChoiceSelectionSkipButton>("SkipButton");
        if (_canSkip)
        {
            _skipButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnSkipButtonReleased));
            _skipButton.AnimateIn();
        }
        else
        {
            _skipButton.Disable();
            _skipButton.Visible = false;
        }

        // Setup Peek Button
        _peekButton = GetNode<NPeekButton>("%PeekButton");
        _peekButton.AddTargets(_banner, _cardRow, _skipButton, _inspectPrompt);
        _peekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(_ =>
        {
            if (_peekButton.IsPeeking)
            {
                MouseFilter = MouseFilterEnum.Ignore;
                _combatPiles.Visible = true;
                _combatPiles.Enable();
                _skipButton.Disable();
            }
            else
            {
                MouseFilter = MouseFilterEnum.Stop;
                _combatPiles.Visible = false;
                _combatPiles.Disable();
                if (_canSkip) _skipButton.Enable();
            }
        }));

        // Controller / Keyboard focus setup
        int childCount = _cardRow.GetChildCount();
        for (int j = 0; j < childCount; j++)
        {
            Control child = _cardRow.GetChild<Control>(j);
            child.FocusNeighborBottom = child.GetPath();
            child.FocusNeighborTop = child.GetPath();
            child.FocusNeighborLeft = (j > 0) ? _cardRow.GetChild(j - 1).GetPath() : _cardRow.GetChild(childCount - 1).GetPath();
            child.FocusNeighborRight = (j < childCount - 1) ? _cardRow.GetChild(j + 1).GetPath() : _cardRow.GetChild(0).GetPath();
        }

        UpdateControllerIcons();
        NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerIcons));
        NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerIcons));
        NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerIcons));
    }

    private Vector2 GetSelectionCenterPosition()
    {
        Vector2 screenSize = GetViewportRect().Size;
        NCreature? creatureNode = _targetCreature?.GetCreatureNode();

        if (creatureNode != null && GodotObject.IsInstanceValid(creatureNode))
        {
            Vector2 creatureTop = creatureNode.GetTopOfHitbox();
            Vector2 calculatedPos = creatureTop + new Vector2(0f, -160f);

            // Clamp so cards never spawn off-screen
            float halfWidth = (_cards.Count * BaseCardXSpacing) * 0.5f + 80f;
            calculatedPos.X = Mathf.Clamp(calculatedPos.X, halfWidth, screenSize.X - halfWidth);
            calculatedPos.Y = Mathf.Clamp(calculatedPos.Y, 180f, screenSize.Y - 180f);

            return calculatedPos;
        }

        // Default screen center fallback if creature node isn't present
        return screenSize * 0.5f;
    }

    public void SelectHolder(NCardHolder cardHolder)
    {
        if (Time.GetTicksMsec() - _openedTicks > NoSelectionTimeMsec)
        {
            _completionSource.SetResult(new[] { cardHolder.CardModel });
        }
    }

    public void OpenPreviewScreen(NCardHolder cardHolder)
    {
        NInspectCardScreen inspectCardScreen = NGame.Instance.GetInspectCardScreen();
        inspectCardScreen.Open(new List<CardModel> { cardHolder.CardModel }, 0);
    }

    public async Task<IEnumerable<CardModel>> CardsSelected()
    {
        IEnumerable<CardModel> result = await _completionSource.Task;
        NOverlayStack.Instance.Remove(this);
        return result;
    }

    public void OnSkipButtonReleased(NButton _)
    {
        _completionSource.SetResult(Array.Empty<CardModel>());
    }

    public void AfterOverlayOpened()
    {
        Modulate = Colors.Transparent;
        _openedTicks = Time.GetTicksMsec();
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(this, "modulate:a", 1f, 0.2);
    }

    public void AfterOverlayClosed()
    {
        _fadeTween?.Kill();
        _peekButton.SetPeeking(false);
        this.QueueFreeSafely();
    }

    public void AfterOverlayShown()
    {
        Visible = true;
        if (CombatManager.Instance.IsInProgress) _peekButton.Enable();
        if (_canSkip && !_peekButton.IsPeeking) _skipButton.Enable();
    }

    public void AfterOverlayHidden()
    {
        _peekButton.Disable();
        _skipButton.Disable();
        Visible = false;
    }

    public void UpdateControllerIcons()
    {
        _inspectPrompt.Modulate = NControllerManager.Instance.IsUsingDirectionalNavigation ? Colors.White : Colors.Transparent;
        _inspectPrompt.GetNode<NHotkeyIcon>("%HotkeyIcon").UpdateInput(MegaInput.confirm);
        _inspectPrompt.GetNode<MegaLabel>("%Label").SetTextAutoSize(new LocString("gameplay_ui", "TO_INSPECT_PROMPT").GetFormattedText());
    }

    public override void _ExitTree()
    {
        if (!_completionSource.Task.IsCompleted)
        {
            _completionSource.TrySetCanceled();
        }

        foreach (var item in _cardRow.GetChildren().OfType<NGridCardHolder>())
        {
            item.QueueFreeSafely();
        }
    }
}