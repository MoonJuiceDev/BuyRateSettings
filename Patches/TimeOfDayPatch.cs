using BuyRateSettings.Abstractions;
using UnityEngine;

namespace BuyRateSettings.Patches;

[HarmonyPatch( typeof( TimeOfDay ) )]
internal static class TimeOfDayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch( nameof( TimeOfDay.SetBuyingRateForDay ) )]
    public static void SetBuyRate( ) => BuyRateRefresher.Refresh( false );

    [HarmonyPostfix]
    [HarmonyPatch( nameof( TimeOfDay.SetBuyingRateForDay ) )]
    public static void LastDayRefreshBonus()
    {
        int refreshCountLDBonus = BuyRateModifier.refreshCountLDBonus.Value;
        float daysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;

        // Give last day refresh usage bonus
        if (refreshCountLDBonus > 0 && daysUntilDeadline <= 0)
        {
            BuyRateState.Value.RefreshCountDaily = (BuyRateState.Value.RefreshCountDaily - refreshCountLDBonus);
            BuyRateModifier.mls.LogInfo($"Gave last day bonus to RefreshCount-Daily (daily refresh count: {BuyRateState.Value.RefreshCountDaily}");

            BuyRateState.Value.RefreshCountQuota = (BuyRateState.Value.RefreshCountQuota - refreshCountLDBonus);
            BuyRateModifier.mls.LogInfo($"Gave last day bonus to RefreshCount-Quota (quota refresh count: {BuyRateState.Value.RefreshCountQuota}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch( nameof( TimeOfDay.SetNewProfitQuota ) )]
    public static void QuotaBuyRateReset()
    {
        BuyRateState.Value.RefreshCountQuota = 0;
        BuyRateModifier.mls.LogInfo($"Reset RefreshCount-Quota (quota refresh count: {BuyRateState.Value.RefreshCountQuota})");
    }
}
