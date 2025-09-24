using Mirror;
using System.Collections;
using UnityEngine;

public class RocketLauncher : EnemyCannonShoot
{
    [Header("Bullet")]
    public GameObject enemyBullet;

    [Header("Shooting")]
    public float rocketSpawnDelay = 0.2f;
    public int numberOfRocketInSalvo = 3;

    [Header("Rotation")]
    public float rotationDuration = 0.5f;
    public float rotationAngleOfReloading = -90f;
    public float rotationAngleOfReadyToShot = 60f;

    [Server]
    private void FixedUpdate()
    {
        if (!waiting)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                reloadTimer = reloadDelay;

                GetRandomPlayerTarget();
                StartCoroutine(SpawnRocketsCoroutine());
            }
        }
    }

    IEnumerator SpawnRocketsCoroutine()
    {
        waiting = true;

        // rotate cannon to AngleOfReadyToShot
        yield return StartCoroutine(RotateToAngle(rotationAngleOfReadyToShot, rotationDuration));
        yield return new WaitForSeconds(rotationDuration);

        // shoot rockets
        for (int i = 0; i < numberOfRocketInSalvo; i++)
        {
            SpawnRocket();
            yield return new WaitForSeconds(rocketSpawnDelay);
        }

        // rotate cannon to AngleOfReloading
        yield return StartCoroutine(RotateToAngle(rotationAngleOfReloading, rotationDuration));

        waiting = false;
    }

    IEnumerator RotateToAngle(float targetAngle, float duration)
    {
        float startAngle = transform.eulerAngles.z;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float dt = time / duration;
            float angle = Mathf.Lerp(startAngle, targetAngle, dt);
            transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        firePoint.rotation = Quaternion.Euler(0, 0, targetAngle);
    }

    [Server]
    void SpawnRocket()
    {
        float angleInDegrees = firePoint.eulerAngles.z;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        GameObject Bullet = Instantiate(enemyBullet, firePoint.position, firePoint.rotation);
        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<LightRocketBulletCollision>().Init(this.gameObject);

        var rocket = Bullet.GetComponent<LightRocketBulletMovement>();
        rocket.target = targetPlayer; //give player position to bullet when spawned
        Mirror.NetworkServer.Spawn(Bullet);
    }
}

