using UnityEngine;

public class OrbitalMLRSEnemyShoot : MonoBehaviour
{
    public float startShootingDelay = 3f;
    public float maxRandomShootingDelay = 2f;
    private float finalShootingDelay;

    private float timer = 0f;

    public GameObject frontCannon;
    public GameObject backCannon;

    private void Start()
    {
        finalShootingDelay = startShootingDelay + Random.value * maxRandomShootingDelay;
        timer = finalShootingDelay;
    }

    void FixedUpdate()
    {
        if (timer <= 0f) {
            var frontRocketLauncher = frontCannon.GetComponent<RocketLauncher>();
            if (frontRocketLauncher != null)
            {
                frontRocketLauncher.ReadyToShoot();
            }

            var backRocketLauncher = backCannon.GetComponent<RocketLauncher>();
            if(backRocketLauncher != null)
            {
                backRocketLauncher.ReadyToShoot();
            }
        }

        timer -= Time.deltaTime;
    }
}
