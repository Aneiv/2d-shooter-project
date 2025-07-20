using UnityEngine;

public class OrbitalMLRSEnemyShoot : MonoBehaviour, IEnemy
{
    private bool waiting = true;
    public float maxRandomShootingDelay = 2f;
    private float finalShootingDelay;

    private float timer = 0f;

    public GameObject frontCannon;
    public GameObject backCannon;

    private void Start()
    {
        finalShootingDelay = Random.value * maxRandomShootingDelay;
        timer = finalShootingDelay;
    }

    void FixedUpdate()
    {
        if (!waiting)
        {
            if (timer <= 0f)
            {
                var frontRocketLauncher = frontCannon.GetComponent<RocketLauncher>();
                if (frontRocketLauncher != null)
                {
                    frontRocketLauncher.ReadyToShoot();
                }

                var backRocketLauncher = backCannon.GetComponent<RocketLauncher>();
                if (backRocketLauncher != null)
                {
                    backRocketLauncher.ReadyToShoot();
                }
            }

            timer -= Time.deltaTime;
        }
    }

    public void OnArrival()
    {
        waiting = false;
    }
}
