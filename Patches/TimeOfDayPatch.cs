using Unity.Netcode;

namespace BuyRateSettings.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal static class TimeOfDayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TimeOfDay.SetBuyingRateForDay))]
    public static void SetBuyRate()
    {
        if ((Configuration.Config.Synced && !NetworkManager.Singleton.IsHost) || NetworkManager.Singleton.IsHost)
        {
            BuyRateRefresher.Refresh();
        }
    }
}
