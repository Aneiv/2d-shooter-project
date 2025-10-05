using Mirror;
using Mirror.Discovery;
using System.Net;
using UnityEngine;

public class CustomNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    protected override DiscoveryRequest GetRequest()
    {
        return new DiscoveryRequest();
    }

    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        int numberOfMaxPlayers = MyNetworkManager.singleton.maxConnections;
        int numberOfCurrentPlayers = NetworkServer.connections.Count;

        bool isLimitOfPlayerIsReached = numberOfCurrentPlayers >= numberOfMaxPlayers;
        bool isGameStarted = LanLobbyManager.Instance.gameIsStarted;

        bool isOpen = !isLimitOfPlayerIsReached && !isGameStarted;

        return new DiscoveryResponse
        {
            serverId = ServerId,
            ip = LanLobbyManager.Instance.GetLocalIPAddress(),
            serverName = LanLobbyManager.Instance.GetPlayerName(),
            maxPlayers = 2,
            currentPlayers = numberOfCurrentPlayers,
            isOpen = isOpen
        };
    }

    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
        response.EndPoint = endpoint;
        OnServerFound.Invoke(response);
    }
}
