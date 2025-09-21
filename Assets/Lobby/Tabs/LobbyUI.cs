using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

[Obsolete("Use CanvasReference instead.")]
public class LobbyUI : MonoBehaviour
{
    void Start()
    {
        RefreshReferences();
    }
    private void RefreshReferences()
    {
        var hostButton = GameObject.Find("Canvas/HostMenu/HostButton")?.GetComponent<Button>();
        var joinButton = GameObject.Find("Canvas/ClientMenu/JoinButton")?.GetComponent<Button>();
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
