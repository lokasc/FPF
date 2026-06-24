using UnityEngine;
using Steamworks;

public class SteamHelperFunctions : MonoBehaviour
{
    public static SteamHelperFunctions Instance { get; private set; }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    public void UnlockAchivement(int id)
    {
        if (SteamClient.IsValid) {
            Steamworks.Data.Achievement ach = new Steamworks.Data.Achievement(id.ToString());
            if (ach.Name != null) {
                ach.Trigger();
            }
        }
    }
}
