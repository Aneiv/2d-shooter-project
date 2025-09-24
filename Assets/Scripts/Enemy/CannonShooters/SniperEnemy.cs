
using Mirror;
using UnityEngine;

public class SniperEnemy : EnemyShoot
{
    public float maxRandomShootingDelay = 2f;
    [SyncVar] private float finalShootingDelay;

    [SerializeField]
    private GameObject cannon;

    [Server]
    public override void Start()
    {
        base.Start();
        finalShootingDelay = Random.value * maxRandomShootingDelay;
        timer = finalShootingDelay;
    }

    [Server]
    void FixedUpdate()
    {
        if (!waiting)
        {
            if (timer <= 0f)
            {
                var sniperCannon = cannon.GetComponent<EnemyCannonShoot>();
                var followSprite = cannon.GetComponent<FollowSprite>();
                if (sniperCannon != null && followSprite != null)
                {
                    followSprite.StartFollow();
                    sniperCannon.ReadyToShoot();
                }
            }

            timer -= Time.deltaTime;
        }
    }
}

