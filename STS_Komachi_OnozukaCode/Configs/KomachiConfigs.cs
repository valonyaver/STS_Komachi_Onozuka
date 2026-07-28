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
    }
}
