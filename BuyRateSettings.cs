using BepInEx.Configuration;
using BepInEx.Logging;
using BuyRateSettings.Patches;

namespace BuyRateSettings
{
    [BepInPlugin(GeneratedPluginInfo.Identifier, GeneratedPluginInfo.Name, GeneratedPluginInfo.Version)]
    public sealed class BuyRateModifier : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(GeneratedPluginInfo.Identifier);

        // Variables
        public static ConfigEntry<bool> minMaxToggle = default!;
        public static ConfigEntry<float> minRate = default!;
        public static ConfigEntry<float> maxRate = default!;

        public static ConfigEntry<bool> randomRateToggle = default!;

        public static ConfigEntry<bool> lastDayToggle = default!;
        public static ConfigEntry<float> lastDayRangeChance = default!;
        public static ConfigEntry<float> lastDayMinRate = default!;
        public static ConfigEntry<float> lastDayMaxRate = default!;

        public static ConfigEntry<bool> jackpotToggle = default!;
        public static ConfigEntry<bool> jackpotToggleLD = default!;
        public static ConfigEntry<double> jackpotChance = default!;
        public static ConfigEntry<float> jackpotMinRate = default!;
        public static ConfigEntry<float> jackpotMaxRate = default!;

        public static ConfigEntry<bool> jackpotAlertToggle = default!;
        public static ConfigEntry<bool> buyRateAlertToggle = default!;
        public static ConfigEntry<float> alertDelaySeconds = default!;

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
            minMaxToggle = Config.Bind("Min/Max Rate", "Min/Max Enabled", true, "Guarantees Company buy rates within the set minimum/maximum buy rate values.");
            minRate = Config.Bind("Min/Max Rate", "Minimum Rate", 0.2f, "The minimum rate the Company will buy your scrap for (0.2 = 20%).");
            maxRate = Config.Bind("Min/Max Rate", "Maximum Rate", 1.2f, "The maximum rate the Company will buy your scrap for (1.2 = 120%).");

            randomRateToggle = Config.Bind("Random Rate", "Random Rate Enabled", true, "Randomize the daily buy rate within the minimum/maximum that is set.");

            lastDayToggle = Config.Bind("Last Day Rate", "Last Day Rate Enabled", true, "Guarantees a specified buy rate range on the last day of the deadline.\nIf you want a specific rate, set both the Last Day Min/Max to the same values.\nI recommended having this on while using random rate, so you aren't screwed from a bad roll on the last day.");
            lastDayRangeChance = Config.Bind("Last Day Rate", "Last Day Random Chance", 0.3f, "The chance for the last day rate to be randomized within the 'Last Day Min/Max' range instead of being the default 100% (0.3 = 30%)\n1.0 = Always randomized, 0.0 = Never randomized (always the default 100% rate)");
            lastDayMinRate = Config.Bind("Last Day Rate", "Last Day Minimum Rate", 1.0f, "The minimum rate to occur on the last day of the deadline (1.0 = 100%).");
            lastDayMaxRate = Config.Bind("Last Day Rate", "Last Day Maximum Rate", 1.2f, "The maximum rate to occur on the last day of the deadline (1.2 = 120%).");

            jackpotToggle = Config.Bind("Jackpot Rate", "Jackpot Rate Enabled", true, "Enables a chance of rolling the jackpot rate within the minimum/maximum that is set.");
            jackpotToggleLD = Config.Bind("Jackpot Rate", "Jackpot Rate ONLY On Last Day", true, "Only allow jackpot rates on the last day of the deadline.");
            jackpotChance = Config.Bind("Jackpot Rate", "Jackpot Chance", .01, "The chance of rolling a jackpot rate (0.01 = 1%).");
            jackpotMinRate = Config.Bind("Jackpot Rate", "Jackpot Minimum Rate", 1.5f, "The minimum jackpot rate (1.5 = 150%).");
            jackpotMaxRate = Config.Bind("Jackpot Rate", "Jackpot Maximum Rate", 3f, "The maximum jackpot rate (3.0 = 300%).");

            buyRateAlertToggle = Config.Bind("Alerts", "Buy Rate Alerts Enabled", true, "Shows a yellow message on screen with the new daily buy rate.");
            jackpotAlertToggle = Config.Bind("Alerts", "Jackpot Alerts Enabled", true, "Shows a red message on screen with the jackpot buy rate (will show as SCRAP EMERGENCY).");
            alertDelaySeconds = Config.Bind("Alerts", "Alert Delay", 3f, "Number of seconds to wait to display the alert.\nTo prevent overlapping with other mods like BetterEXP and DiscountAlerts, I recommend 8+ seconds.");


            // Patching & Logging
            mls = BepInEx.Logging.Logger.CreateLogSource(GeneratedPluginInfo.Identifier);

            mls.LogInfo("Patching the Company's buy rates....");

            harmony.PatchAll(typeof(BuyRateModifier));
            harmony.PatchAll(typeof(TimeOfDayPatch));

            mls.LogInfo("The Company's buy rates have been patched!");
        }
    }
}
