using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;








/// <summary>
///Singleton class used for syncing spawned object position on local player chunks
/// And deleting server objects from local chunk server object list
/// </summary>

public class NetworkWorldManager : Mirror.NetworkBehaviour
{
    public static NetworkWorldManager Instance;

    private void Awake()
    {
        Instance = this;
    }
        
    /// <summary>
    /// Class used to spawn server objects on local player chunks
    /// </summary>
    /// <param name="objToSpawn"></param>
    /// <param name="spawnPos"></param>
    /// <param name="parentChunkName"></param>
    /// <param name="chunkObject"></param>
    [Server]
    public void SpawnServerObject(GameObject objToSpawn, Vector3 spawnPos, string parentChunkName, EndlessTerrain.TerrainChunk chunkObject)
    {
        if (parentChunkName == null || objToSpawn == null)
            return;

        GameObject spawnObj = Instantiate(objToSpawn, spawnPos, Quaternion.identity);
            
        var serverObject = spawnObj.GetComponent<ServerManagedObject>();
        if (serverObject != null)
            serverObject.parentName = parentChunkName;

        if (chunkObject != null)
        {
            if (chunkObject.serverManagedObjects == null)
                chunkObject.serverManagedObjects = new List<GameObject>();

            chunkObject.serverManagedObjects.Add(spawnObj);
        }
            
        NetworkServer.Spawn(spawnObj);
    }

    /// <summary>
    /// Class used to clear spawned server objects
    /// that disappeared from view
    /// </summary>
    /// <param name="serverObjects"></param>
    [Server]
    public void ClearChunkServerObjects(List<GameObject> serverObjects)
    {
        if (serverObjects == null)
            return;

        foreach (var obj in serverObjects.ToList())
        {
            if (obj != null)
                NetworkServer.Destroy(obj);

            serverObjects.Clear();
        }
    }
}
