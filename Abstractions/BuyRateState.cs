namespace BuyRateSettings.Abstractions;

internal sealed class BuyRateState
{
    public static BuyRateState Value { get; } = new();

    public int RefreshCount { get; set; }
}
