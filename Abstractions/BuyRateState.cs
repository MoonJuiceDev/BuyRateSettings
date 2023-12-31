namespace BuyRateSettings.Abstractions;

internal sealed class BuyRateState
{
    public static BuyRateState Value { get; } = new();

    public int RefreshCountDaily { get; set; }
    public int RefreshCountQuota { get; set; }
}
