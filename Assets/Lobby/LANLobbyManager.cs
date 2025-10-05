using Mirror;
using Mirror.Discovery;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanLobbyManager : MonoBehaviour
{
    public MyNetworkManager manager;
    public static LanLobbyManager Instance { get; private set; }

    public TMP_InputField ipInput;       //host ip input
    public TMP_InputField clientNameInput;
    public TMP_InputField hostNameInput;
    public TextMeshProUGUI hostIpText;
    public TextMeshProUGUI backlogTextHost;
    public TextMeshProUGUI backlogTextClient;
    private bool tryingToConnect = true;
    public bool gameIsStarted = false;
    //HOST
    public void StartHost()
    {
        gameIsStarted = false;
        string playerName = GetPlayerNameFromInput(hostNameInput);
        hostNameInput.text = playerName; // set specific player name after hosting

        manager.StartHost();
        string localIP = GetLocalIPAddress();
        hostIpText.text = localIP;

        // share serwer info 
        var discovery = FindFirstObjectByType<CustomNetworkDiscovery>();
        discovery.AdvertiseServer();
    }

    //CLIENT
    public void JoinGame()
    {
        manager.networkAddress = ipInput.text;
        if (string.IsNullOrWhiteSpace(ipInput.text))
        {
            return;
        }
        SetBacklogText("all", "Connecting...");
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
            ResetNetworkSettings();
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
        tryingToConnect = true;
        gameIsStarted = false;

        // Reset lokalnych zmiennych UI
        ipInput.text = "";
        hostIpText.text = "";
        SetBacklogText("all", "");

        clientNameInput.text = "";
        clientNameInput.readOnly = false;

        hostNameInput.text = "";
        hostNameInput.readOnly = false;
    }

    public void SetUIReferences(CanvasReferences ui)
    {
        ipInput = ui.ipInput;

        clientNameInput = ui.clientNameInput;
        clientNameInput.readOnly = false;

        hostNameInput = ui.hostNameInput;
        hostNameInput.readOnly = false;

        hostIpText = ui.hostIpText;
        backlogTextHost = ui.backlogTextHost;
        backlogTextClient = ui.backlogTextClient;
    }


    void FixedUpdate()
    {
        if (manager == null || backlogTextHost == null || backlogTextClient == null) return;

        if (Mirror.NetworkServer.active && Mirror.NetworkClient.isConnected)
        {
            SetBacklogText("host", "Running as Host (Server + Client)");
        }
        else if (Mirror.NetworkServer.active && !Mirror.NetworkClient.isConnected)
        {
            SetBacklogText("host", "Running as Dedicated Server");
        }
        else if (Mirror.NetworkClient.isConnected && !Mirror.NetworkServer.active)
        {
            SetBacklogText("client", "Connected as Client");
        }
        else if (Mirror.NetworkClient.isConnecting)
        {
            SetBacklogText("all", "Connecting to server...");
        }
        else
        {
            SetBacklogText("all", "Not connected");
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
                SetBacklogText("all", "Connected succesfully!");
                tryingToConnect = false;
                yield break;
            }

            if (!Mirror.NetworkClient.isConnecting && !Mirror.NetworkClient.isConnected)
            {
                SetBacklogText("all", "Couldn't connect!");
                tryingToConnect = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (tryingToConnect)
        {
            SetBacklogText("all", "Couldn't connect (timeout)!");
            tryingToConnect = false;
        }
    }
    //get local host IP
    public string GetLocalIPAddress()
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

    public string GetPlayerName()
    {
        if (Mirror.NetworkServer.active && Mirror.NetworkClient.isConnected) // running as host
        {
            return GetPlayerNameFromInput(hostNameInput);
        }
        else if (Mirror.NetworkClient.isConnected && !Mirror.NetworkServer.active) // running as client
        {
            return GetPlayerNameFromInput(clientNameInput);
        }

        return "???";
    }

    private string GetPlayerNameFromInput(TMP_InputField inputField)
    {
        inputField.readOnly = true; // lock input

        //default name
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            string defaultPlayerName = "Player" + Random.Range(1000, 9999);
            inputField.text = defaultPlayerName;

            return defaultPlayerName;
        }

        return inputField.text;
    }

    private void SetBacklogText(string type, string text)
    {
        if(type == "host")
        {
            backlogTextHost.text = text;
        }else if(type == "client")
        {
            backlogTextClient.text = text;
        }
        else
        {
            backlogTextHost.text = text;
            backlogTextClient.text = text;
        }
    }
}
