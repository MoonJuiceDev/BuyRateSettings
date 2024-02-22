using UnityEngine;
using BuyRateSettings.Configuration;

namespace BuyRateSettings;

public static class BuyRateRefresher
{
    public static void Refresh()
    {
        // Variables
        float price = StartOfRound.Instance.companyBuyingRate;
        double rateSeed = StartOfRound.Instance.randomMapSeed * 3 + 99; // +99 because starting seed is 0, which causes a jackpot on day 1 // *3 just so the rateSeed is diff from the mapSeed
        double rateSeedRemainder = rateSeed % 100 / 100;

        bool minMaxToggle = Config.Instance.minMaxToggle;
        float minRate = Config.Instance.minRate;
        float maxRate = Config.Instance.maxRate;

        bool randomRateToggle = Config.Instance.randomRateToggle;

        bool lastDayToggle = Config.Instance.lastDayToggle;
        float lastDayRangeChance = Config.Instance.lastDayRangeChance;
        float lastDayMinRate = Config.Instance.lastDayMinRate;
        float lastDayMaxRate = Config.Instance.lastDayMaxRate;
        float daysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;

        bool jackpotToggle = Config.Instance.jackpotToggle;
        bool jackpotToggleLD = Config.Instance.jackpotToggleLD;
        double jackpotChance = Config.Instance.jackpotChance;
        float jackpotMinRate = Config.Instance.jackpotMinRate;
        float jackpotMaxRate = Config.Instance.jackpotMaxRate;
        bool jackpotHit = false;

        bool jackpotAlertToggle = Config.Default.jackpotAlertToggle; // Client sided
        bool buyRateAlertToggle = Config.Default.buyRateAlertToggle; // Client sided

        float alertDelaySeconds = Config.Instance.alertDelaySeconds;
        float rateDelayTimeSeconds = 3; // Hardcoded to set the rate a 2nd time 3s later to prevent being overwritten by other mods


        //
        // START OF RATE CALCULATIONS (this all works, don't touch it)
        //
        // This a series of else-if statements that follows an order of priority:
        //
        // Jackpot on last day
        // Jackpot on any day
        // Last day
        // Random
        // Min/Max
        // Vanilla

        BuyRateModifier.mls.LogInfo($"Days left: {TimeOfDay.Instance.daysUntilDeadline}");
        BuyRateModifier.mls.LogInfo($"Initial buying rate (pre-calculation): {StartOfRound.Instance.companyBuyingRate}");
        BuyRateModifier.mls.LogInfo($"Jackpot chance: {jackpotChance}");
        BuyRateModifier.mls.LogInfo($"Map seed: {StartOfRound.Instance.randomMapSeed}");
        BuyRateModifier.mls.LogInfo($"Rate seed: {rateSeed}");
        BuyRateModifier.mls.LogInfo($"Rate seed remainder: {rateSeedRemainder}");


        // Check for jackpot on last day and roll
        if (jackpotToggle && rateSeedRemainder <= jackpotChance && jackpotToggleLD && daysUntilDeadline == 0)
        {
            // Last day jackpot range
            if (jackpotMinRate != jackpotMaxRate)
            {
                price = Next() * (jackpotMaxRate - jackpotMinRate) + jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - LAST DAY ONLY (RANGED) (unrounded rate: {price})");
            }

            // Last day jackpot no range
            else
            {
                price = jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - LAST DAY ONLY (NOT RANGED) (unrounded rate: {price})");
            }
        }

        // Jackpot on any day and roll
        else if (jackpotToggle && rateSeedRemainder <= jackpotChance && jackpotToggleLD == false)
        {
            // Any day jackpot ranged
            if (jackpotMinRate != jackpotMaxRate)
            {
                price = Next() * (jackpotMaxRate - jackpotMinRate) + jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - ANY DAY (RANGED) (unrounded rate: {price})");
            }

            // Any day jackpot no range
            else
            {
                price = jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - ANY DAY (NOT RANGED) (unrounded rate: {price})");
            }
        }

        // Check/set last day rate range
        else if (lastDayToggle && daysUntilDeadline == 0)
        {
            // Last day random range hit
            if (lastDayMinRate != lastDayMaxRate && rateSeedRemainder <= lastDayRangeChance)
            {
                price = Next() * (lastDayMaxRate - lastDayMinRate) + lastDayMinRate;

                BuyRateModifier.mls.LogInfo($"Last day rate (ranged-hit) picked (unrounded rate: {price})");
            }

            // Last day random range not hit (default to 100%)
            else if (lastDayMinRate != lastDayMaxRate && rateSeedRemainder > lastDayRangeChance)
            {
                price = 1f;
                BuyRateModifier.mls.LogInfo($"Last day rate (ranged-nohit) picked (unrounded rate: {price})");
            }

            // Last day no range
            else
            {
                price = lastDayMinRate;
                BuyRateModifier.mls.LogInfo($"Last day rate (not ranged) picked (unrounded rate: {price})");
            }
        }

        // Check/set random rate
        else if (randomRateToggle && minMaxToggle)
        {
            price = Next() * (maxRate - minRate) + minRate;

            BuyRateModifier.mls.LogInfo($"Random Rate picked (unrounded rate: {price})");
        }

        // Set minimum rate
        else if (minMaxToggle && StartOfRound.Instance.companyBuyingRate <= minRate)
        {
            price = minRate;
            BuyRateModifier.mls.LogInfo($"Min rate picked (unrounded rate: {price})");
        }

        // Set maximum rate
        else if (minMaxToggle && StartOfRound.Instance.companyBuyingRate >= maxRate)
        {
            price = maxRate;
            BuyRateModifier.mls.LogInfo($"Max rate picked (unrounded rate: {price})");
        }

        // Default definition (vanilla)
        else
        {
            price = StartOfRound.Instance.companyBuyingRate;
            BuyRateModifier.mls.LogInfo($"Vanilla rate picked (unrounded rate: {price})");
        }


        //
        // END OF RATE CALCULATIONS
        //


        // Round price for cleaner text
        int priceRounded = (int)Math.Round(price * 100);

        // Set buy rate immediately & start coroutine for delayed set
        StartOfRound.Instance.companyBuyingRate = price;
        BuyRateModifier.mls.LogInfo("Set Company buy rate to: " + priceRounded + "%");
        TimeOfDay.Instance.StartCoroutine(BuyRateSetter(rateDelayTimeSeconds, price, priceRounded)); // Reassigns the buy rate a 2nd time in case a new deadline forced a vanilla calculation

        // Send buy rate alerts and/or set price
        if (jackpotAlertToggle && jackpotHit)
        {
            TimeOfDay.Instance.StartCoroutine(BuyRateAlertJackpot(alertDelaySeconds, priceRounded));
        }
        else if (buyRateAlertToggle)
        {
            TimeOfDay.Instance.StartCoroutine(BuyRateAlertNormal(alertDelaySeconds, priceRounded));
        }

        // RNG algorithm
        float Next()
        {
            System.Random random = new(StartOfRound.Instance.randomMapSeed);
            return (float)random.NextDouble();
        }
    }

    private static IEnumerator BuyRateSetter(float rateDelayTimeSeconds, float buyRate, int buyRateRounded)
    {
        yield return new WaitForSeconds(rateDelayTimeSeconds);
        StartOfRound.Instance.companyBuyingRate = buyRate;
        BuyRateModifier.mls.LogInfo("Set Company buy rate to " + buyRateRounded + "% (delayed)");
    }

    private static IEnumerator BuyRateAlertNormal(float alertDelayTimeSeconds, int buyRateRounded)
    {
        yield return new WaitForSecondsRealtime(alertDelayTimeSeconds);
        HUDManager.Instance.DisplayTip("New Scrap Rate", "\n* Buying rates have changed to " + buyRateRounded + "%", false, false, "LC_JackpotTip2");
        BuyRateModifier.mls.LogInfo($"Sent normal alert. Rate: {buyRateRounded}");
    }

    private static IEnumerator BuyRateAlertJackpot(float alertDelayTimeSeconds, int buyRateRounded)
    {
        yield return new WaitForSecondsRealtime(alertDelayTimeSeconds);
        HUDManager.Instance.DisplayTip("<color=#ffc526>SCRAP EMERGENCY</color>", "<color=#fcbf17>\n* Buying rates have soared to " + buyRateRounded + "%</color>", true, false, "LC_JackpotTip1");
        HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.globalNotificationSFX);
        BuyRateModifier.mls.LogInfo($"Sent jackpot alert. Rate: {buyRateRounded}");
    }
}
