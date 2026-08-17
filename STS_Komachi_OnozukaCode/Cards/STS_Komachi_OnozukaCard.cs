using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Tokens;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Danmaku;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using System.Runtime.InteropServices.Marshalling;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards
{
    /// <summary>
    /// This is the base class for your mod's cards, which is set up to load the card's images from your mod's resources.
    /// When creating a card, right click the Cards folder and create a new file with the Custom Card template.
    /// This will generate a class that extends this one.
    /// You can also just create the class manually; just make sure to inherit from this class.
    /// </summary>
    [Pool(typeof(STS_Komachi_OnozukaCardPool))]
    public abstract class STS_Komachi_OnozukaCard : ConstructedCardModel
    {
        protected STS_Komachi_OnozukaCard(int cost, CardType type, CardRarity rarity, TargetType target)
        : base(cost, type, rarity, target)
        {
            base.WithTip(new TooltipSource((CardModel card) =>
                new HoverTip(new LocString("static_hover_tips", "KOMACHI-ARTIST-TITLE"),
                             new LocString("cards", base.Id.Entry + ".artist"), null)));

            // Defaulting an alt description value of 0 for all cards.
            WithVar(nameof(AltDescription), 0);


            WithVar(new StringVar(nameof(ExtraDescription1), "Extra Description 1"));
            WithVar(new StringVar(nameof(ExtraDescription2), "Extra Description 2"));
        }
        public int Value1
        {
            get => DynamicVars[nameof(Value1)].IntValue;
            set
            {
                DynamicVars[nameof(Value1)].BaseValue = value;
            }
        }

        public int Value2
        {
            get => DynamicVars[nameof(Value2)].IntValue;
            set
            {
                DynamicVars[nameof(Value2)].BaseValue = value;
            }
        }

        public int Value3
        {
            get => DynamicVars[nameof(Value3)].IntValue;
            set
            {
                DynamicVars[nameof(Value3)].BaseValue = value;
            }
        }

        /// <summary>
        /// Should've probably made this sooner.
        /// </summary>
        public int ReleaseCost
        {
            get => DynamicVars[nameof(ReleaseCost)].IntValue;
            set
            {
                DynamicVars[nameof(ReleaseCost)].BaseValue = value;
            }
        }

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                if (DynamicVars.TryGetValue(nameof(ReleaseCost), out var releaseCost))
                {
                    return ReleaseCmd.CanReleaseSpirits(Owner.Creature, releaseCost.IntValue);
                }
                return false;
            }
        }

        /// <summary>
        /// Enables an alternate description for the card.
        /// Use the format `{AltDescription:choose(1|2|3):one|two|three|other}` to use. 
        /// </summary>
        public int AltDescription
        {
            get => DynamicVars[nameof(AltDescription)].IntValue;
            set
            {
                DynamicVars[nameof(AltDescription)].BaseValue = value;
            }
        }

        /// <summary>
        /// Additional text stuff. Used in tokens mostly.
        /// </summary>
        public LocString RawExtraDescription1
        {
            get
            {
                LocString locString = new LocString("cards", base.Id.Entry + ".extraDescription1");
                if (!locString.Exists())
                {
                    throw new InvalidOperationException($"No extraDescription1 for {base.Id}.");
                }

                DynamicVars.AddTo(locString);
                return locString;
            }
        }

        /// <summary>
        /// Additional text stuff2. Used in tokens mostly.
        /// </summary>
        public LocString RawExtraDescription2
        {
            get
            {
                LocString locString = new LocString("cards", base.Id.Entry + ".extraDescription2");
                if (!locString.Exists())
                {
                    throw new InvalidOperationException($"No extraDescription2 for {base.Id}.");
                }

                DynamicVars.AddTo(locString);
                return locString;
            }
        }


        /// <summary>
        /// Must be manually added set to raw to cards that use it.
        /// </summary>
        public string ExtraDescription1
        {
            get => ((StringVar)DynamicVars[nameof(ExtraDescription1)]).StringValue;
            set
            {
                ((StringVar)DynamicVars[nameof(ExtraDescription1)]).StringValue = value;
            }
        }

        /// <summary>
        /// Must be manually added set to raw to cards that use it.
        /// </summary>
        public string ExtraDescription2
        {
            get => ((StringVar)DynamicVars[nameof(ExtraDescription2)]).StringValue;
            set
            {
                ((StringVar)DynamicVars[nameof(ExtraDescription2)]).StringValue = value;
            }
        }

        /// <summary>
        /// Implements Replenish Logic
        /// </summary>
        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card != this) return;
            if (card.Keywords.Contains(KomachiKeywords.Replenish))
            {
                await CardPileCmd.Draw(choiceContext, Owner);
            }
        }

        /// <summary>
        /// Override to report the set of Displacements this card can apply to its target. 
        /// Used for cards that displace before dealing damage.
        /// Default null means this card doesn't displace its target, 
        /// so no preview or highlighting is computed for it.
        /// </summary>
        public virtual int[]? GetPossibleDisplacements() => null;

        /// <summary>
        /// Override to replace DistancePower's standard damage multiplier for a specific
        /// absolute Distance level, when THIS card is the one dealing the hit.
        /// Curently only used for scythe of final judgemento
        /// </summary>
        public virtual decimal? GetDistanceMultiplierOverride(int distanceLevel) => null;

        /// <summary>
        /// Reports the amount of Vengeful Spirits that this card applies, for preview of how much damage they will do.
        /// </summary>
        public virtual int? GetVengefulSpiritStacksApplied(Creature target) => null;

        /// <summary>
        /// Danmaku Patterns that this card uses.
        /// </summary>
        public virtual List<DanmakuPiece> patterns => [];
        //Image size:
        //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
        //Full art: 606x852
        public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

        //Smaller variants of card images for efficiency:
        //Smaller variant of fullart: 250x350
        //Smaller variant of normalart: 250x190

        //Uses card_portraits/card_name.png as image path. These should be smaller images.
        public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
        public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
        
    }
}