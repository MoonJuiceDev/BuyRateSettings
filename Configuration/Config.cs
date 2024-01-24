using BepInEx.Configuration;
using Unity.Collections;
using Unity.Netcode;

namespace BuyRateSettings.Configuration;

[Serializable]
public class Config : SyncedInstance<Config>
{
    private static ConfigFile cfg;

    public static ConfigEntry<bool>? minMaxToggleEntry { get; private set; }
    public static ConfigEntry<float>? minRateEntry { get; private set; }
    public static ConfigEntry<float>? maxRateEntry { get; private set; }

    public static ConfigEntry<bool>? randomRateToggleEntry { get; private set; }

    public static ConfigEntry<bool>? lastDayToggleEntry { get; private set; }
    public static ConfigEntry<float>? lastDayRangeChanceEntry { get; private set; }
    public static ConfigEntry<float>? lastDayMinRateEntry { get; private set; }
    public static ConfigEntry<float>? lastDayMaxRateEntry { get; private set; }

    public static ConfigEntry<bool>? jackpotToggleEntry { get; private set; }
    public static ConfigEntry<bool>? jackpotToggleLDEntry { get; private set; }
    public static ConfigEntry<double>? jackpotChanceEntry { get; private set; }
    public static ConfigEntry<float>? jackpotMinRateEntry { get; private set; }
    public static ConfigEntry<float>? jackpotMaxRateEntry { get; private set; }

    public static ConfigEntry<bool>? jackpotAlertToggleEntry { get; private set; }
    public static ConfigEntry<bool>? buyRateAlertToggleEntry { get; private set; }
    public static ConfigEntry<float>? alertDelaySecondsEntry { get; private set; }


    public bool minMaxToggle { get; private set; }
    public float minRate { get; private set; }
    public float maxRate { get; private set; }

    public bool randomRateToggle { get; private set; }

    public bool lastDayToggle { get; private set; }
    public float lastDayRangeChance { get; private set; }
    public float lastDayMinRate { get; private set; }
    public float lastDayMaxRate { get; private set; }
    public float daysUntilDeadline { get; private set; }

    public bool jackpotToggle { get; private set; }
    public bool jackpotToggleLD { get; private set; }
    public double jackpotChance { get; private set; }
    public float jackpotMinRate { get; private set; }
    public float jackpotMaxRate { get; private set; }

    public bool jackpotAlertToggle { get; private set; }
    public bool buyRateAlertToggle { get; private set; }

    public float alertDelaySeconds { get; private set; }

    public Config(ConfigFile config)
    {
        cfg = config;
        InitInstance(this);
        Reload();
    }

    public void Reload(bool setValues = true)
    {
        minMaxToggleEntry = cfg.Bind("Min/Max Rate", "Min/Max Enabled", true, "Guarantees Company buy rates within the set minimum/maximum buy rate values.");
        minRateEntry = cfg.Bind("Min/Max Rate", "Minimum Rate", 0.2f, "The minimum rate the Company will buy your scrap for (0.2 = 20%).");
        maxRateEntry = cfg.Bind("Min/Max Rate", "Maximum Rate", 1.2f, "The maximum rate the Company will buy your scrap for (1.2 = 120%).");

        randomRateToggleEntry = cfg.Bind("Random Rate", "Random Rate Enabled", true, "Randomize the daily buy rate within the minimum/maximum that is set.");

        lastDayToggleEntry = cfg.Bind("Last Day Rate", "Last Day Rate Enabled", true, "Guarantees a specified buy rate range on the last day of the deadline.\nIf you want a specific rate, set both the Last Day Min/Max to the same values.\nI recommended having this on while using random rate, so you aren't screwed from a bad roll on the last day.");
        lastDayRangeChanceEntry = cfg.Bind("Last Day Rate", "Last Day Random Chance", 0.3f, "The chance for the last day rate to be randomized within the 'Last Day Min/Max' range instead of being the default 100% (0.3 = 30%)\n1.0 = Always randomized, 0.0 = Never randomized (always the default 100% rate)");
        lastDayMinRateEntry = cfg.Bind("Last Day Rate", "Last Day Minimum Rate", 1.0f, "The minimum rate to occur on the last day of the deadline (1.0 = 100%).");
        lastDayMaxRateEntry = cfg.Bind("Last Day Rate", "Last Day Maximum Rate", 1.2f, "The maximum rate to occur on the last day of the deadline (1.2 = 120%).");

        jackpotToggleEntry = cfg.Bind("Jackpot Rate", "Jackpot Rate Enabled", true, "Enables a chance of rolling the jackpot rate within the minimum/maximum that is set.");
        jackpotToggleLDEntry = cfg.Bind("Jackpot Rate", "Jackpot Rate ONLY On Last Day", true, "Only allow jackpot rates on the last day of the deadline.");
        jackpotChanceEntry = cfg.Bind("Jackpot Rate", "Jackpot Chance", .01, "The chance of rolling a jackpot rate (0.01 = 1%).");
        jackpotMinRateEntry = cfg.Bind("Jackpot Rate", "Jackpot Minimum Rate", 1.5f, "The minimum jackpot rate (1.5 = 150%).");
        jackpotMaxRateEntry = cfg.Bind("Jackpot Rate", "Jackpot Maximum Rate", 3f, "The maximum jackpot rate (3.0 = 300%).");

        buyRateAlertToggleEntry = cfg.Bind("Alerts", "Buy Rate Alerts Enabled", true, "Shows a yellow message on screen with the new daily buy rate.\nCLIENT SIDED, DOES NOT SYNC WITH HOST");
        jackpotAlertToggleEntry = cfg.Bind("Alerts", "Jackpot Alerts Enabled", true, "Shows a red message on screen with the jackpot buy rate (will show as SCRAP EMERGENCY).\nCLIENT SIDED, DOES NOT SYNC WITH HOST");
        alertDelaySecondsEntry = cfg.Bind("Alerts", "Alert Delay", 3f, "Number of seconds to wait to display the alert.\nTo prevent overlapping with other mods like BetterEXP and DiscountAlerts, I recommend 8+ seconds.");

        if (setValues)
        {
            minMaxToggle = minMaxToggleEntry.Value;
            minRate = minRateEntry.Value;
            maxRate = maxRateEntry.Value;

            randomRateToggle = randomRateToggleEntry.Value;

            lastDayToggle = lastDayToggleEntry.Value;
            lastDayRangeChance = lastDayRangeChanceEntry.Value;
            lastDayMinRate = lastDayMinRateEntry.Value;
            lastDayMaxRate = lastDayMaxRateEntry.Value;

            jackpotToggle = jackpotToggleEntry.Value;
            jackpotToggleLD = jackpotToggleLDEntry.Value;
            jackpotChance = jackpotChanceEntry.Value;
            jackpotMinRate = jackpotMinRateEntry.Value;
            jackpotMaxRate = jackpotMaxRateEntry.Value;

            buyRateAlertToggle = buyRateAlertToggleEntry.Value;
            jackpotAlertToggle = jackpotAlertToggleEntry.Value;
            alertDelaySeconds = alertDelaySecondsEntry.Value;
        }
    }

    // Config sync stuff
    public static void RequestSync()
    {
        if (!IsClient) return;

        using FastBufferWriter stream = new(IntSize, Allocator.Temp);
        MessageManager.SendNamedMessage($"{GeneratedPluginInfo.Name}_OnRequestConfigSync", 0uL, stream);
    }

    public static void OnRequestSync(ulong clientId, FastBufferReader _)
    {
        if (!IsHost) return;

        BuyRateModifier.mls.LogInfo($"Config sync request received from client: {clientId}");

        byte[] array = SerializeToBytes(Instance);
        int value = array.Length;

        using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

        try
        {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            MessageManager.SendNamedMessage($"{GeneratedPluginInfo.Name}_OnReceiveConfigSync", clientId, stream);
        }
        catch (Exception e)
        {
            BuyRateModifier.mls.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
        }
    }

    public static void OnReceiveSync(ulong _, FastBufferReader reader)
    {
        if (!reader.TryBeginRead(IntSize))
        {
            BuyRateModifier.mls.LogError("Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if (!reader.TryBeginRead(val))
        {
            BuyRateModifier.mls.LogError("Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        SyncInstance(data);

        BuyRateModifier.mls.LogInfo("Successfully synced config with host.");

        BuyRateRefresher.Refresh();

        BuyRateModifier.mls.LogInfo("Successfully refreshed post-sync.");
    }
}
