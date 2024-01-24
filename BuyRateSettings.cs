using BepInEx.Logging;
using BuyRateSettings.Patches;
using BuyRateSettings.Configuration;

namespace BuyRateSettings
{
    [BepInPlugin(GeneratedPluginInfo.Identifier, GeneratedPluginInfo.Name, GeneratedPluginInfo.Version)]
    public sealed class BuyRateModifier : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(GeneratedPluginInfo.Identifier);

        // Initialization
        public static Config BRconfig { get; private set; }

        private static BuyRateModifier Instance;

        public static ManualLogSource mls;

        // Run patch on Awake
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }


            // Create config file
            BRconfig = new(Config);


            // Patching & Logging
            mls = BepInEx.Logging.Logger.CreateLogSource(GeneratedPluginInfo.Identifier);

            mls.LogInfo("Loading buy rate patches...");

            harmony.PatchAll(typeof(BuyRateModifier));
            harmony.PatchAll(typeof(Config));
            harmony.PatchAll(typeof(TimeOfDayPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));

            mls.LogInfo("The Company's buy rates have been patched.");
        }
    }
}
