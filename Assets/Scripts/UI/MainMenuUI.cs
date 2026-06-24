using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using FishNet;
public class MainMenuUI : MonoBehaviour
{
    
    [SerializeField]private TextMeshProUGUI steamID;
    [SerializeField]private TextMeshProUGUI steamDisplayName;
    [SerializeField]private Button hostButton;
    [SerializeField]private Button clientButton;
    [SerializeField] private TMP_InputField steamAddress;
    
    private P2PConnectionBehaviour _connectionManager;
    void Start()
    {
        _connectionManager = InstanceFinder.NetworkManager.GetComponent<P2PConnectionBehaviour>(); 
        
        // Connects UI to functionality.
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

    private void OnClickClient()
    {
        if (_connectionManager.ClientConnect(steamAddress.text))
        {
            clientButton.gameObject.SetActive(false);
        }
    }
    private void OnClickHost()
    {
        if (_connectionManager.HostConnect())
        {
            clientButton.gameObject.SetActive(false);
            hostButton.gameObject.SetActive(false);
        }
    }
        
}
