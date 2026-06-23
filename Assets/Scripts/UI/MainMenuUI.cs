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
    }

    public void OnClickClient()
    {
        InstanceFinder.ClientManager.StartConnection();
    }

    public void OnClickHost()
    {
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
    }
}
