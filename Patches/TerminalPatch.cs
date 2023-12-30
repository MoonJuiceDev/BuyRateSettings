using BuyRateSettings.Abstractions;
using UnityEngine;

namespace BuyRateSettings.Patches;

[HarmonyPatch( typeof( Terminal ) )]
internal static class TerminalPatch
{
    [HarmonyPatch( "Awake" )]
    [HarmonyPostfix]
    public static void OnPostAwake( Terminal __instance )
    {
        // Add keywords
        __instance.terminalNodes.allKeywords = [
            .. __instance.terminalNodes.allKeywords,
            RefreshBuyRateKeyword()];

        // Add entry to Other menu
        var node = __instance.terminalNodes.allKeywords.First( keyword => keyword.word is "other" );
        node.specialKeywordResult.displayText = node.specialKeywordResult.displayText.TrimEnd()
            + "\n\n>REFRESH RATE\nRefresh the Company's market rate for scrap\n\n";

        static TerminalKeyword RefreshBuyRateKeyword( )
        {
            var node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = $"\nAre you sure you want to refresh the current Company buying rate?\n\nThis can be done {BuyRateModifier.refreshLimit.Value} TIME(S) PER DAY.\n\nPlease CONFIRM or DENY.\n\n";
            node.overrideOptions = true;
            node.terminalEvent = "refresh rate";
            node.terminalOptions = [ ConfirmedNoun(), DeniedNoun() ];

            var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            keyword.isVerb = false;
            keyword.specialKeywordResult = node;
            keyword.word = "refresh rate";

            return keyword;

            static CompatibleNoun ConfirmedNoun( )
            {
                var node = ScriptableObject.CreateInstance<TerminalNode>();
                node.clearPreviousText = true;
                node.displayText = "\nThe Company's buying rate has been refreshed.\n\n";
                node.terminalEvent = BuyRateTerminalEvents.RefreshConfirmed;

                var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                keyword.word = "confirm";

                return new()
                {
                    noun = keyword,
                    result = node,
                };
            }

            static CompatibleNoun DeniedNoun( )
            {
                var node = ScriptableObject.CreateInstance<TerminalNode>();
                node.clearPreviousText = false;
                node.displayText = "Cancelled\n";
                node.terminalEvent = BuyRateTerminalEvents.RefreshDenied;

                var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                keyword.word = "deny";

                return new()
                {
                    noun = keyword,
                    result = node,
                };
            }
        }
    }


    [HarmonyPatch( nameof( Terminal.LoadNewNode ) )]
    [HarmonyPrefix]
    public static void OnPreLoadNewNode( Terminal __instance, TerminalNode node )
    {
        int refreshLimit = BuyRateModifier.refreshLimit.Value;

        // if Confirmed & under limit
        if(node.terminalEvent is BuyRateTerminalEvents.RefreshConfirmed && BuyRateState.Value.RefreshCount < refreshLimit)
        {
            // Call refresher and increment count
            BuyRateRefresher.Refresh( true );
            BuyRateState.Value.RefreshCount++;

            int roundedRate = (int)Math.Round( StartOfRound.Instance.companyBuyingRate * 100 );
            node.displayText = "\nThe Company's buying rates have been updated.\n\nThe Company is currently buying scrap at " + roundedRate + "%\n\n";

            BuyRateModifier.mls.LogInfo( $"Manual buy rate refresh accepted: (count: {BuyRateState.Value.RefreshCount}, rate: {roundedRate})" );
        }
        // if Confirmed & over limit
        else if(node.terminalEvent is BuyRateTerminalEvents.RefreshConfirmed && BuyRateState.Value.RefreshCount >= refreshLimit)
        {
            node.displayText = "\nWe are unable to refresh the Company's buying rates at this time.\n\n";

            BuyRateModifier.mls.LogInfo( $"Manual buy rate refresh denied (count: {BuyRateState.Value.RefreshCount})" );
        }
    }

    private static class BuyRateTerminalEvents
    {
        public const string RefreshConfirmed = "refresh buy rate confirmed";

        public const string RefreshDenied = "refresh buy rate denied";
    }
}
