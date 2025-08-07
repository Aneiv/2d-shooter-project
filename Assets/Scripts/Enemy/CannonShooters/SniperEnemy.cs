
using UnityEngine;

public class SniperEnemy : EnemyShoot
{
    public float maxRandomShootingDelay = 2f;
    private float finalShootingDelay;

    [SerializeField]
    private GameObject cannon;

    public override void Start()
    {
        base.Start();
        finalShootingDelay = Random.value * maxRandomShootingDelay;
        timer = finalShootingDelay;
    }

    void FixedUpdate()
    {
        if (!waiting)
        {
            if (timer <= 0f)
            {
                var sniperCannon = cannon.GetComponent<SniperCannon>();
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

