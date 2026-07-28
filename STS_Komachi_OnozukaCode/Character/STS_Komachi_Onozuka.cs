using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Attack;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Cards.Basics;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Configs;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Relics;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character
{
      
    public class STS_Komachi_Onozuka : PlaceholderCharacterModel
    {
        public const string CharacterId = "STS_Komachi_Onozuka";

        public static readonly Color Color = new(255, 0, 0);

        public override Color NameColor => Color;
        public override Color MapDrawingColor => Color;
        public override CharacterGender Gender => CharacterGender.Feminine;
        public override int StartingHp => 74;

        public override IEnumerable<CardModel> StartingDeck => [
            ModelDb.Card<StrikeKomachi>(),
            ModelDb.Card<StrikeKomachi>(),
            ModelDb.Card<StrikeKomachi>(),
            ModelDb.Card<StrikeKomachi>(),
            ModelDb.Card<StrikeKomachi>(),
            ModelDb.Card<DefendKomachi>(),
            ModelDb.Card<DefendKomachi>(),
            ModelDb.Card<DefendKomachi>(),
            ModelDb.Card<DefendKomachi>(),
            ModelDb.Card<ShootAndMove>(),
            ModelDb.Card<HandyRetreat>()
        ];

        public override IReadOnlyList<RelicModel> StartingRelics =>
        [
            ModelDb.Relic<TheTitanic>()
        ];

        public override CardPoolModel CardPool => ModelDb.CardPool<STS_Komachi_OnozukaCardPool>();
        public override RelicPoolModel RelicPool => ModelDb.RelicPool<STS_Komachi_OnozukaRelicPool>();
        public override PotionPoolModel PotionPool => ModelDb.PotionPool<STS_Komachi_OnozukaPotionPool>();

        /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
            override all the other methods that define those assets. 
            These are just some of the simplest assets, given some placeholders to differentiate your character with. 
            You don't have to, but you're suggested to rename these images. */
        public override Control CustomIcon
        {
            get
            {
                var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
                icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                return icon;
            }
        }
        public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
        public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
        public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
        public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();

        /// <summary>
        /// Combat Model
        /// </summary>
        public override string CustomVisualPath => 
            KomachiConfigs.UseDairiPortrait ? "combat_model_dairi.tscn".ScenePath() 
            : "combat_model.tscn".ScenePath();
        /// <summary>
        /// Character select Background
        /// </summary>
        public override string CustomCharacterSelectBg => "select/character_select_bg_komachi.tscn".ScenePath();
        /// <summary>
        /// Merchant model
        /// </summary>
        public override string CustomMerchantAnimPath => "merchant/merchant_komachi.tscn".ScenePath();

        public override string CustomRestSiteAnimPath => "rest/rest_site_komachi.tscn".ScenePath();
        public override string CustomEnergyCounterPath => "energy/energy_counter_komachi.tscn".ScenePath();

        //public override NCreatureVisuals CreateCustomVisuals()
        //{
        //    BaseLib.Utils.CustomAnimation
        //}
    }
}