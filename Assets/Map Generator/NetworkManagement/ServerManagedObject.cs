using System.Collections;
using Mirror;
using UnityEngine;


/// <summary>
/// Class used to set certain server object parent chunk
/// Deletes object if chunk wasn't found
/// </summary>
public class ServerManagedObject:NetworkBehaviour
{
    public static bool IsServerManaged = true;
    [SyncVar] public string parentName; //chunk name
    private bool isParentSet = false;
    

    public override void OnStartClient()
    {
        base.OnStartClient();
        TrySetParent();
        if (!isParentSet)
            StartCoroutine(TrySetParentLater());
    }

    private void TrySetParent()
    {
        if (string.IsNullOrEmpty(parentName)) return;

        GameObject parent = GameObject.Find(parentName);
        if (parent != null)
        {
            transform.SetParent(parent.transform, worldPositionStays: true);
            isParentSet = true;
        }
    }

    private IEnumerator TrySetParentLater()
    {
        int retries = 10;
        while (!isParentSet && retries-- > 0)
        {
            yield return new WaitForSeconds(0.1f);
            TrySetParent();
        }
        //when parent could not be found
        if (!isParentSet)
        {
            Destroy(gameObject);
        }
    }


    [Server]
    public void DestroyNetworkObject(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }
}
