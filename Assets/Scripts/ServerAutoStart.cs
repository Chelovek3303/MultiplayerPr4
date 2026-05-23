using FishNet;
using UnityEngine;

public class ServerAutoStart : MonoBehaviour
{
    private void Start()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("[Server] Headless mode detected. Automatically starting server transport...");
            
            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.StartConnection();
            }
            else
            {
                Debug.LogError("[Server] ServerManager not found! Make sure NetworkManager is in the scene.");
            }
        }
    }
}
