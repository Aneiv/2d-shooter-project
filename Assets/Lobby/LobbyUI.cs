using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LobbyUI : MonoBehaviour
{
    void Start()
    {
        RefreshReferences();
    }
    private void RefreshReferences()
    {
        var hostButton = GameObject.Find("Canvas/LobbyMenu/HostButton")?.GetComponent<Button>();
        var joinButton = GameObject.Find("Canvas/LobbyMenu/JoinButton")?.GetComponent<Button>();
        var lobbyManager = FindFirstObjectByType<LanLobbyManager>();
        if (hostButton != null)
        {
            hostButton.onClick.RemoveAllListeners();
            hostButton.onClick.AddListener(lobbyManager.StartHost);
        }

        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(lobbyManager.JoinGame);
        }
    }
}
