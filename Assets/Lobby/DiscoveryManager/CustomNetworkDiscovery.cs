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
        return new DiscoveryResponse
        {
            serverId = ServerId,
            ip = LanLobbyManager.Instance.GetLocalIPAddress(),
            serverName = LanLobbyManager.Instance.GetPlayerName(),
            maxPlayers = 2,
            currentPlayers = NetworkServer.connections.Count
        };
    }

    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
        response.EndPoint = endpoint;
        OnServerFound.Invoke(response);
    }
}
