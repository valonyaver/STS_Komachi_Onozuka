using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using System.Threading;
using System.Threading.Tasks;


namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku.Nodes
{
	public enum DarkenOverlayStyle
	{
		Expand,
		Fade,
	}

	public partial class NDanmakuDarkenOverlay : Node2D
	{
		public static readonly string scenePath = "danmaku/darkenOverlay/darkenOverlay.tscn".ScenePath();

		public CancellationTokenSource? _cts;

		public static float _introDuration => 1.75f;
		public static float _outroDuration => 0.4f;

		ColorRect? _rect;
		ShaderMaterial? _mat;

		float _lifetimeSeconds;
		Color _darkenColor;
		DarkenOverlayStyle _style;
		Creature? _originCreature;
		Vector2? _originOverride;

		public static NDanmakuDarkenOverlay? Create(
			float lifetimeSeconds,
			Creature? originCreature = null,
			Vector2? originOverride = null,
			Color? tint = null,
			DarkenOverlayStyle style = DarkenOverlayStyle.Fade)
		{
			if (TestMode.IsOn)
			{
				return null;
			}

			NDanmakuDarkenOverlay overlay = PreloadManager.Cache.GetScene(scenePath).Instantiate<NDanmakuDarkenOverlay>(PackedScene.GenEditState.Disabled);
			overlay.Initialize(lifetimeSeconds, originCreature, originOverride, tint ?? new Color(0f, 0f, 0f, 0.5f), style);
			return overlay;
		}

		public void Initialize(float lifetimeSeconds, Creature? originCreature, Vector2? originOverride, Color darkenColor, DarkenOverlayStyle style)
		{
			_lifetimeSeconds = lifetimeSeconds;
			_originCreature = originCreature;
			_originOverride = originOverride;
			_darkenColor = darkenColor;
			_style = style;
		}

		public override void _Ready()
		{
			_rect = GetNode<ColorRect>("Rect");
			_rect.Color = new Color(1, 1, 1, 1);

			// --- NEW CODE: Make the drawing rect massive ---
			// Because this is just a canvas for your shader, making it huge 
			// guarantees it covers the camera's view no matter what.
			_rect.Size = new Vector2(16000, 9000);
			_rect.Position = new Vector2(-8000, -4500);
			// -----------------------------------------------

			_mat = _rect.Material as ShaderMaterial;
			_mat?.SetShaderParameter("darken_color", _darkenColor);

			UpdateOriginPosition();
			TaskHelper.RunSafely(PlaySequence());
		}

		public override void _Process(double delta)
		{
			// Only needs to keep tracking if following a moving creature; a fixed
			// originOverride never changes, so this is a no-op in that case.
			if (_originCreature != null)
			{
				UpdateOriginPosition();
			}
		}

		public override void _ExitTree()
		{
			_cts?.Cancel();
		}

		public async Task PlaySequence()
		{
			_cts = new CancellationTokenSource();

			switch (_style)
			{
				case DarkenOverlayStyle.Expand:
					await PlayExpandIntro();
					break;
				case DarkenOverlayStyle.Fade:
					await PlayFadeIntro();
					break;
			}

			float holdDuration = Mathf.Max(0f, _lifetimeSeconds - _introDuration - _outroDuration);
			await Cmd.Wait(holdDuration, _cts.Token);

			switch (_style)
			{
				case DarkenOverlayStyle.Expand:
					await PlayExpandOutro();
					break;
				case DarkenOverlayStyle.Fade:
					await PlayFadeOutro();
					break;
			}

			this.QueueFreeSafely();
		}

		// ---- Expand style: growing ring from the origin point ----

		async Task PlayExpandIntro()
		{
			_mat?.SetShaderParameter("intensity", 1f);
			_mat?.SetShaderParameter("front_radius", 0f);

			float hugeRadius = GetViewport().GetVisibleRect().Size.Length();
			Tween tween = GetTree().CreateTween();
			tween.TweenMethod(Callable.From<float>(SetFrontRadius), 0f, hugeRadius, _introDuration)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			await Cmd.Wait(_introDuration, _cts!.Token);
		}

		async Task PlayExpandOutro()
		{
			float hugeRadius = GetViewport().GetVisibleRect().Size.Length();
			Tween tween = GetTree().CreateTween();
			tween.TweenMethod(Callable.From<float>(SetFrontRadius), hugeRadius, 0f, _outroDuration)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
			await Cmd.Wait(_outroDuration, _cts!.Token);
		}

		// ---- Fade style: darkness fades in/out uniformly ----

		async Task PlayFadeIntro()
		{
			float hugeRadius = GetViewport().GetVisibleRect().Size.Length();
			_mat?.SetShaderParameter("front_radius", hugeRadius);
			_mat?.SetShaderParameter("intensity", 0f);

			Tween tween = GetTree().CreateTween();
			tween.TweenMethod(Callable.From<float>(SetIntensity), 0f, 1f, _introDuration)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			await Cmd.Wait(_introDuration, _cts!.Token);
		}

		async Task PlayFadeOutro()
		{
			Tween tween = GetTree().CreateTween();
			tween.TweenMethod(Callable.From<float>(SetIntensity), 1f, 0f, _outroDuration)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
			await Cmd.Wait(_outroDuration, _cts!.Token);
		}

		void SetFrontRadius(float r) => _mat?.SetShaderParameter("front_radius", r);
		void SetIntensity(float v) => _mat?.SetShaderParameter("intensity", v);

		void UpdateOriginPosition()
		{
			if (_mat == null) return;
			_mat.SetShaderParameter("origin_screen_pos", GetOriginScreenPos());
		}

		Vector2 GetOriginScreenPos()
		{
			if (_originOverride.HasValue) return GetViewport().GetCanvasTransform() * _originOverride.Value;

			if (_originCreature != null && !_originCreature.IsDead)
			{
				NCreature? node = _originCreature.GetCreatureNode();
				if (node != null) return GetViewport().GetCanvasTransform() * node.VfxSpawnPosition;
			}

			// Fallback: screen center, so an expand never looks broken if origin data is missing
			Rect2 screen = GetViewport().GetVisibleRect();
			return GetViewport().GetCanvasTransform() * (screen.Size * 0.5f);
		}
	}
}
