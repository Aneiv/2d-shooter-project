using Mirror;
using System.Collections;
using UnityEngine;

public class EnemyShoot : Mirror.NetworkBehaviour, IEnemy
{
    protected bool waiting = true;
    protected float timer = 0f;
    protected GameObject bulletsContainer;

    virtual public void Start()
    {
        bulletsContainer = GameObject.Find("BulletsContainer");
    }
    public void OnArrival()
    {
        waiting = false;
    }
}
