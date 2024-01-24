using BuyRateSettings.Configuration;

namespace BuyRateSettings.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
public class GameNetworkManagerPatch
{
    // Start config sync when player joins lobby
    [HarmonyPostfix]
    [HarmonyPatch("Singleton_OnClientConnectedCallback")]
    public static void InitializeLocalPlayer()
    {
        if (Config.IsHost)
        {
            try
            {
                Config.MessageManager.RegisterNamedMessageHandler($"{GeneratedPluginInfo.Name}_OnRequestConfigSync", Config.OnRequestSync);
                Config.Synced = true;
            }
            catch (Exception e)
            {
                BuyRateModifier.mls.LogError(e);
            }

            return;
        }

        Config.Synced = false;
        Config.MessageManager.RegisterNamedMessageHandler($"{GeneratedPluginInfo.Name}_OnReceiveConfigSync", Config.OnReceiveSync);
        Config.RequestSync();
    }

    // Revert config sync when player leaves lobby
    [HarmonyPostfix]
    [HarmonyPatch("StartDisconnect")]
    public static void PlayerLeave()
    {
        Config.RevertSync();
    }
}
