
using UnityEngine;

public class OrbitalMLRSEnemy : EnemyShoot
{
    public float maxRandomShootingDelay = 2f;
    private float finalShootingDelay;

    [SerializeField]
    private GameObject frontCannon;
    [SerializeField]
    private GameObject backCannon;
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
                var frontRocketLauncher = frontCannon.GetComponent<EnemyCannonShoot>();
                var backRocketLauncher = backCannon.GetComponent<EnemyCannonShoot>();
                if (frontRocketLauncher != null && backRocketLauncher != null)
                {
                    frontRocketLauncher.ReadyToShoot();
                    backRocketLauncher.ReadyToShoot();
                }
            }

            timer -= Time.deltaTime;
        }
    }
}

