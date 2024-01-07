using UnityEngine;

namespace BuyRateSettings;

public static class BuyRateRefresher
{
    public static void Refresh()
    {
        // Variables
        float price = StartOfRound.Instance.companyBuyingRate;

        bool minMaxToggle = BuyRateModifier.minMaxToggle.Value;
        float minRate = BuyRateModifier.minRate.Value;
        float maxRate = BuyRateModifier.maxRate.Value;

        bool randomRateToggle = BuyRateModifier.randomRateToggle.Value;

        bool lastDayToggle = BuyRateModifier.lastDayToggle.Value;
        float lastDayRangeChance = BuyRateModifier.lastDayRangeChance.Value;
        float lastDayMinRate = BuyRateModifier.lastDayMinRate.Value;
        float lastDayMaxRate = BuyRateModifier.lastDayMaxRate.Value;
        float daysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline;

        bool jackpotToggle = BuyRateModifier.jackpotToggle.Value;
        bool jackpotToggleLD = BuyRateModifier.jackpotToggleLD.Value;
        double jackpotChance = BuyRateModifier.jackpotChance.Value;
        float jackpotMinRate = BuyRateModifier.jackpotMinRate.Value;
        float jackpotMaxRate = BuyRateModifier.jackpotMaxRate.Value;
        bool jackpotHit = false;

        bool jackpotAlertToggle = BuyRateModifier.jackpotAlertToggle.Value;
        bool buyRateAlertToggle = BuyRateModifier.buyRateAlertToggle.Value;

        float alertDelaySeconds = BuyRateModifier.alertDelaySeconds.Value;
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


        // Check for jackpot on last day and roll
        if (jackpotToggle && DateTime.Now.Millisecond % 100 / 100.0 <= jackpotChance && jackpotToggleLD && daysUntilDeadline <= 0)
        {
            // Last day jackpot range
            if (jackpotMinRate != jackpotMaxRate)
            {
                price = Next() * (jackpotMaxRate - jackpotMinRate) + jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - LAST DAY ONLY (RANGED) (unrounded rate: {price}");
            }

            // Last day jackpot no range
            else
            {
                price = jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - LAST DAY ONLY (NOT RANGED) (unrounded rate: {price}");
            }
        }

        // Jackpot on any day and roll
        else if (jackpotToggle && DateTime.Now.Millisecond % 100 / 100.0 <= jackpotChance && jackpotToggleLD == false && daysUntilDeadline > 0)
        {
            // Any day jackpot ranged
            if (jackpotMinRate != jackpotMaxRate)
            {
                price = Next() * (jackpotMaxRate - jackpotMinRate) + jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - ANY DAY (RANGED) (unrounded rate: {price}");
            }

            // Any day jackpot no range
            else
            {
                price = jackpotMinRate;
                jackpotHit = true;

                BuyRateModifier.mls.LogInfo($"HIT THE JACKPOT - ANY DAY (NOT RANGED) (unrounded rate: {price}");
            }
        }

        // Check/set last day rate range
        else if (lastDayToggle && daysUntilDeadline <= 0)
        {
            // Last day random range hit
            if (lastDayMinRate != lastDayMaxRate && DateTime.Now.Millisecond % 100 / 100.0 <= lastDayRangeChance)
            {
                price = Next() * (lastDayMaxRate - lastDayMinRate) + lastDayMinRate;

                BuyRateModifier.mls.LogInfo($"Last day rate (ranged-hit) picked (unrounded rate: {price}");
            }

            // Last day random range not hit (default to 100%)
            else if (lastDayMinRate != lastDayMaxRate && DateTime.Now.Millisecond % 100 / 100.0 > lastDayRangeChance)
            {
                price = 1f;
                BuyRateModifier.mls.LogInfo($"Last day rate (ranged-nohit) picked (unrounded rate: {price}");
            }

            // Last day no range
            else
            {
                price = lastDayMinRate;
                BuyRateModifier.mls.LogInfo($"Last day rate (not ranged) picked (unrounded rate: {price}");
            }
        }

        // Check/set random rate
        else if (randomRateToggle && minMaxToggle)
        {
            price = Next() * (maxRate - minRate) + minRate;

            BuyRateModifier.mls.LogInfo($"Random Rate picked (unrounded rate: {price}");
        }

        // Set minimum rate
        else if (minMaxToggle && StartOfRound.Instance.companyBuyingRate <= minRate)
        {
            price = minRate;
            BuyRateModifier.mls.LogInfo($"Min rate picked (unrounded rate: {price}");
        }

        // Set maximum rate
        else if (minMaxToggle && StartOfRound.Instance.companyBuyingRate >= maxRate)
        {
            price = maxRate;
            BuyRateModifier.mls.LogInfo($"Max rate picked (unrounded rate: {price}");
        }

        // Default definition (vanilla)
        else
        {
            price = StartOfRound.Instance.companyBuyingRate;
            BuyRateModifier.mls.LogInfo($"Vanilla rate picked (unrounded rate: {price}");
        }


        //
        // END OF RATE CALCULATIONS
        //


        // Round price for cleaner text
        int priceRounded = (int)Math.Round(price * 100);

        // Set buy rate
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
        BuyRateModifier.mls.LogInfo("Sent normal alert");
    }

    private static IEnumerator BuyRateAlertJackpot(float alertDelayTimeSeconds, int buyRateRounded)
    {
        yield return new WaitForSecondsRealtime(alertDelayTimeSeconds);
        HUDManager.Instance.DisplayTip("<color=#ffc526>SCRAP EMERGENCY</color>", "<color=#fcbf17>\n* Buying rates have soared to " + buyRateRounded + "%</color>", true, false, "LC_JackpotTip1");
        HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.globalNotificationSFX);
        BuyRateModifier.mls.LogInfo("Sent jackpot alert");
    }
}
