using BaseLib.Config;
using BaseLib.Hooks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Character;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Configs;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Data;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.PowerPatches;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches.Previewers;
using System.Reflection;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode
{
    [ModInitializer(nameof(Initialize))]
    public partial class MainFile : Node
    {
        public const string ModId = "STS_Komachi_Onozuka"; //Used for resource filepath
        public const string ResPath = $"res://{ModId}";

        public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

        public static void Initialize()
        {
            ModConfigRegistry.Register(ModId, new KomachiConfigs());
            //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
            Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());


            ModManager.OnMetricsUpload += KomachiMetrics.OnMetricsUpload;


            Harmony harmony = new(ModId);
            // Harmony.DEBUG = true;
            harmony.PatchAll();
            Log.Info("[KomachiMod] Harmony PatchAll completed");
            try
            {
                // For some reason this patch in specific does not patch unless i do this.
                var patched = harmony.CreateClassProcessor(typeof(NPower_SecondAmountStyling_Patch)).Patch();
                foreach (var m in patched)
                    MainFile.LogMessage($"[PatchCheck] Actually patched: {m.DeclaringType}.{m.Name}");
            }
            catch (Exception ex)
            {
                MainFile.LogMessage($"[PatchCheck] EXCEPTION: {ex}");
            }
            try
            {
                // For some reason this patch in specific does not patch unless i do this.
                var patched = harmony.CreateClassProcessor(typeof(NPower_ThirdAmount_Patch)).Patch();
                foreach (var m in patched)
                    MainFile.LogMessage($"[PatchCheck] Actually patched: {m.DeclaringType}.{m.Name}");
            }
            catch (Exception ex)
            {
                MainFile.LogMessage($"[PatchCheck] EXCEPTION: {ex}");
            }

            HealthBarForecastRegistry.Register(ModId, "VengefulSpiritHoverPreview", new VengefulSpiritHoverForecastSource());

        }


        public static void PrintAllCards()
        {
            var character = ModelDb.CardPool<STS_Komachi_OnozukaCardPool>();
            var cards = character.AllCards;

            foreach (var card in cards)
            {
                LogMessage($"{card.Title}: {card.Description.LocEntryKey}");
            }
        }

        public static void LogMessage(string message)
        {
            Logger.LogMessage(MegaCrit.Sts2.Core.Logging.LogLevel.Info, message, 0);
        }

        public static string GetModVersion()
        {
            var mod = ModManager.GetLoadedMods().FirstOrDefault(m => m.manifest?.id == ModId);

            return mod?.manifest?.version ?? "unknown";
        }
    }
}
