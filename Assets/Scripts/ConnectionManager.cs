using UnityEngine;
using Steamworks;
using FishNet;

public class P2PConnectionBehaviour : MonoBehaviour
{
    public bool ClientConnect(string input)
    {
        if (InstanceFinder.NetworkManager == null) return false;
        input = input.Trim();
        
        // If we're using other transport methods -> i.e. tugboat.
        if (!SteamClient.IsValid)
        {
            InstanceFinder.ClientManager.StartConnection();
        }
        else
        {
            if (!ulong.TryParse(input, out ulong rawId) || !((SteamId)rawId).IsValid)
                return false;
            InstanceFinder.TransportManager.Transport.SetClientAddress(input);
            InstanceFinder.ClientManager.StartConnection();
        }

        return true;
    }

    // Returns based on whether connection was successful.
    public bool HostConnect()
    {
        if (InstanceFinder.NetworkManager == null) return false;
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
        return true;
    }

    private void OnApplicationQuit()
    {
        try
        {
            SteamClient.Shutdown();
        }
        catch
        {
            
        }
    }
}
