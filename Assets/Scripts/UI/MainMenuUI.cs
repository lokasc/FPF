using UnityEngine;
using FishNet;
using UnityEngine.UI;
using TMPro;
using Steamworks;
public class MainMenuUI : MonoBehaviour
{
    
    [SerializeField]private TextMeshProUGUI steamID;
    [SerializeField]private TextMeshProUGUI steamDisplayName;
    [SerializeField]private Button hostButton;
    [SerializeField]private Button clientButton;
    [SerializeField] private TMP_InputField steamAddress;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // need the guard   
        clientButton.onClick.AddListener(OnClickClient);
        hostButton.onClick.AddListener(OnClickHost);

        if (!SteamClient.IsValid)
        {
            steamID.text += "Steam not connected or not using Steam Transport";
            return;
        } 
        steamID.text += " " + SteamClient.SteamId.ToString();
        steamDisplayName.text += " " + SteamClient.Name.ToString();
        
        SteamHelperFunctions.Instance.UnlockAchivement(10);
        
    }

    public void OnClickClient()
    {
        if (InstanceFinder.NetworkManager == null) return;
        string input = steamAddress.text.Trim();
        if (!ulong.TryParse(input, out ulong rawId) || !((SteamId)rawId).IsValid)
            return;
        
        InstanceFinder.TransportManager.Transport.SetClientAddress(input);
        InstanceFinder.ClientManager.StartConnection();
        clientButton.gameObject.SetActive(false);
    }

    public void OnClickHost()
    {
        if (InstanceFinder.NetworkManager == null) return;
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}
