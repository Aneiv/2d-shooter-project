using Mirror;
using System;
using System.Net;

[System.Serializable]
public class DiscoveryRequest : NetworkMessage
{
}

[System.Serializable]
public class DiscoveryResponse : NetworkMessage
{
    public long serverId;
    public IPEndPoint EndPoint { get; set; }
    public Uri uri;

    public string serverName;
    public int currentPlayers;
    public int maxPlayers;
    public string ip;
}
