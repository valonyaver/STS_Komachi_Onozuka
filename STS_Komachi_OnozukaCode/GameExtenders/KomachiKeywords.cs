using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Extensions
{
public class KomachiKeywords
    {
        [CustomEnum(null)]
        [KeywordProperties(0)]
        public static CardKeyword Displace;

        [CustomEnum(null)]
        [KeywordProperties(0)]
        public static CardKeyword Detonate;

        [CustomEnum(null)]
        [KeywordProperties(0)]
        public static CardKeyword Release;

        [CustomEnum(null)]
        [KeywordProperties(0)]
        public static CardKeyword Barrier;

        [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword Replenish;

        /// <summary>
        /// Screw this game for not having a copy keyword bruh
        /// </summary>
        [CustomEnum, KeywordProperties(AutoKeywordPosition.After)]
        public static CardKeyword Clone;
    }
}
