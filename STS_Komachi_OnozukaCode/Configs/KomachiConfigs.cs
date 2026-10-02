using BaseLib.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Configs
{
    internal class KomachiConfigs : SimpleModConfig
    {
        [ConfigHoverTip]
        public static bool UseDairiPortrait { get; set; } = false;
        [ConfigHoverTip]
        public static bool SkipDanmaku { get; set; } = false;
        [ConfigSection("DamagePreviewSettings")]
        [ConfigHoverTip]
        public static bool ShowNoDistanceEnemyDamagePreview { get; set; } = false;
        [ConfigHoverTip]
        public static bool ShowNoDistanceCardDamagePreview { get; set; } = false;
    }
}
