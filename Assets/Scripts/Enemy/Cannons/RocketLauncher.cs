using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class RocketLauncher : MonoBehaviour, IShootReady
{
    public GameObject enemyBullet;
    public Transform firePoint;
    //target player location
    private Transform targetPlayer;

    public float reloadDelay = 6f;
    public float rotationDuration = 0.5f;
    public float rocketSpawnDelay = 0.2f;

    private bool isWaitingForShot = true;
    private float timer = 0f;

    public int numberOfRocketInSalvo = 3;

    public float rotationAngleOfReloading = -90f;
    public float rotationAngleOfReadyToShot = 60f;
    private GameObject bulletsContainer;

    void Start()
    {
        bulletsContainer = GameObject.Find("BulletsContainer");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }
    }

    private void FixedUpdate()
    {
        if (!isWaitingForShot)
        {
            timer -= Time.deltaTime;
            if(timer <= 0f)
            {
                timer = reloadDelay;

                StartCoroutine(SpawnRocketsCoroutine());
            }
        }
    }

    public void ReadyToShoot()
    {
        isWaitingForShot = false;
    }

    IEnumerator SpawnRocketsCoroutine()
    {
        isWaitingForShot = true;

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

        isWaitingForShot = false;
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
    }

}
