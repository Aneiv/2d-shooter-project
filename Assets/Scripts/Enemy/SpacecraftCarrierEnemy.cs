using UnityEngine;
using DG.Tweening;

public class SpacecraftCarrierEnemy : Enemy, IEnemy
{
    public int cannonCounter = 6;
    public GameObject[] cannonsObjs;
    public GameObject[] cannonContainers;
    private SpacecraftCarrierSpawner SpacecraftCarrierSpawner;


    protected override void Start()
    {
        base.Start();        
        isVulnerable = false;
        SpacecraftCarrierSpawner = GetComponent<SpacecraftCarrierSpawner>();
    }
    public void DestroyCannon()
    {
        cannonCounter--;
        if (cannonCounter <= 0) {
            isVulnerable = true;
        }
    }

    public void OnArrival()
    {
        foreach (GameObject obj in cannonsObjs)
        {
            if (obj.TryGetComponent<IShootReady>(out var cannon))
            {
                cannon.ReadyToShoot();
            }
        }
        foreach(GameObject container in cannonContainers)
        {
            if(container.TryGetComponent<FollowSprite>(out var followSprite))
            {
                followSprite.StartFollow();
            }
        }

        if(SpacecraftCarrierSpawner != null)
        {
            SpacecraftCarrierSpawner.StartSpawningEnemies();
        }
    }
}

