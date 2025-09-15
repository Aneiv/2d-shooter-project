using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LanLobbyManager : MonoBehaviour
{
    public Mirror.NetworkManager manager;
    public static LanLobbyManager Instance { get; private set; }

    public TMP_InputField ipInput;       //host ip input
    public TextMeshProUGUI hostIpText;
    public TextMeshProUGUI backlogText;
    private bool tryingToConnect = true;
    //HOST
    public void StartHost()
    {
        manager.StartHost();
        string localIP = GetLocalIPAddress();
        hostIpText.text = "Your IP: " + localIP;
    }

    //CLIENT
    public void JoinGame()
    {
        manager.networkAddress = ipInput.text;
        backlogText.text = "Connecting...";
        manager.StartClient();
        StartCoroutine(CheckConnection());
    }
    private void Awake()
    {
        //singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) // main menu scene index
        {
            StartCoroutine(RefreshWhenReady());
        }
    }
    public void ResetNetworkSettings()
    {
        if (manager != null)
        {
            // Stop current host or client if active
            if (Mirror.NetworkServer.active || Mirror.NetworkClient.isConnected || Mirror.NetworkClient.isConnecting)
            {
                manager.StopHost();   
                manager.StopClient(); 
            }

            // Reset networkAddress and ports
            manager.networkAddress = "localhost";
            manager.GetComponent<TelepathyTransport>().port = 7777; //TelepathyTransport
        }

        // Reset lokalnych zmiennych UI
        ipInput.text = "";
        backlogText.text = "";
        tryingToConnect = true;
    }

    private IEnumerator RefreshWhenReady()
    {
        //wait for ui to load
        yield return new WaitUntil(() => GameObject.Find("Canvas/LobbyMenu") != null);
        Instance.RefreshReferences();
        Instance.ResetNetworkSettings();
    }
    private void RefreshReferences()
    {
        ipInput = GameObject.Find("Canvas/LobbyMenu/HostIp/IPinput")?.GetComponent<TMP_InputField>();
        hostIpText = GameObject.Find("Canvas/LobbyMenu/MyIPText/IPText")?.GetComponent<TextMeshProUGUI>();
        backlogText = GameObject.Find("Canvas/LobbyMenu/BacklogText/Text")?.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (manager == null || backlogText == null) return;

        if (Mirror.NetworkServer.active && Mirror.NetworkClient.isConnected)
        {
            backlogText.text = "Running as Host (Server + Client)";
        }
        else if (Mirror.NetworkServer.active && !Mirror.NetworkClient.isConnected)
        {
            backlogText.text = "Running as Dedicated Server";
        }
        else if (Mirror.NetworkClient.isConnected && !Mirror.NetworkServer.active)
        {
            backlogText.text = "Connected as Client";
        }
        else if (Mirror.NetworkClient.isConnecting)
        {
            backlogText.text = "Connecting to server...";
        }
        else
        {
            backlogText.text = "Not connected";
        }
    }
    private IEnumerator CheckConnection()
    {
        float timeout = 5f; //time for timeout
        float timer = 0f;

        while (tryingToConnect && timer < timeout)
        {
            if (Mirror.NetworkClient.isConnected)
            {
                backlogText.text = "Connected succesfully!";
                tryingToConnect = false;
                yield break;
            }

            if (!Mirror.NetworkClient.isConnecting && !Mirror.NetworkClient.isConnected)
            {
                backlogText.text = "Couldn't connect!";
                tryingToConnect = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (tryingToConnect)
        {
            backlogText.text = "Couldn't connect (timeout)!";
            tryingToConnect = false;
        }
    }
    //get local host IP
    string GetLocalIPAddress()
    {
        string localIP = "Can't find IP address";
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
        }
        catch { }
        return localIP;
    }

}
