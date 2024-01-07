namespace BuyRateSettings.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal static class TimeOfDayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TimeOfDay.SetBuyingRateForDay))]
    public static void SetBuyRate() => BuyRateRefresher.Refresh();
}
