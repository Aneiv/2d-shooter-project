using Mirror;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Mirror.NetworkManager manager;

    private void Start()
    {
        manager = FindFirstObjectByType<Mirror.NetworkManager>();
    }
    //self-host
    public void PlaySingleplayer()
    {
        manager.StartHost();
        manager.maxConnections = 0;

        var settings = GameDefaultSettings.Instance;
        settings.isSinglePlayerMode = true;
        if (Mirror.NetworkServer.active && Mirror.NetworkClient.isConnected)
        {
            manager.ServerChangeScene("MainGameScene");
        }

    }
    public void PlayMultiplayer()
    {
        var settings = GameDefaultSettings.Instance;
        settings.isSinglePlayerMode = false;
        if (Mirror.NetworkServer.active && Mirror.NetworkClient.isConnected)
        {
            manager.ServerChangeScene("MainGameScene");
        }

    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
