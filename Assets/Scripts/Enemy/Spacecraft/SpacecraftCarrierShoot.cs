using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class SpacecraftCarrierShoot : EnemyShoot
{
    private SpacecraftCarrierEnemy SpacecraftCarrierEnemy;
    public GameObject[] firePoints;
    public GameObject enemyBullet;
    private List<Transform> targetPlayers = new List<Transform>();
    private Coroutine SpawnBulletsRef;

    [SyncVar] float screenCenterYPos;

    public float bulletSpawnDelay = 0.5f;
    public float dectectionTime = 3f;
    public float bulletSpeed;

    private bool anyPlayerDetected = false;

    [Server]
    public override void Start()
    {
        base.Start();
        SpacecraftCarrierEnemy = GetComponent<SpacecraftCarrierEnemy>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            foreach (GameObject p in players) {
                if(p != null)
                    targetPlayers.Add(p.transform);
            }
        }

        Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        screenCenterYPos = screenCenter.y;

        timer = dectectionTime;
    }

    [Server]
    private void FixedUpdate()
    {
        if (waiting || !SpacecraftCarrierEnemy.IsAlive()) return;

        anyPlayerDetected = false;

        // detection of players
        foreach (Transform targetPlayer in targetPlayers)
        {
            if (targetPlayer == null) continue;

            if (!targetPlayer.GetComponent<Player>().isAlive) continue;

            if (targetPlayer.position.y >= screenCenterYPos)
            {
                anyPlayerDetected = true;
                break;
            }
        }
        SetTimer();
    }

    private void SetTimer()
    {
        if (anyPlayerDetected)
        {
            if (SpawnBulletsRef == null)
            {
                timer -= Time.fixedDeltaTime;

                if (timer <= 0f)
                {
                    SpawnBulletsRef = StartCoroutine(SpawnBulletsCoroutine());
                }
            }
        }
        else
        {
            timer = dectectionTime;
        }
    }

    [Server]
    private IEnumerator SpawnBulletsCoroutine()
    {
        waiting = true;
        foreach (GameObject firePointObj in firePoints) {
            Vector2 firePoint = firePointObj.transform.position;
            SpawnBullet(firePoint, 90f);
            SpawnBullet(firePoint, 270f);

            yield return new WaitForSeconds(bulletSpawnDelay);
        }
        waiting = false;
        SpawnBulletsRef = null;
    }

    [Server]
    private void SpawnBullet(Vector2 firePointPos, float angle)
    {
        float angleInDegrees = angle + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Quaternion rotation = Quaternion.Euler(0f, 0f, angleInDegrees);
        GameObject Bullet = Instantiate(enemyBullet, firePointPos, rotation);
        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Mirror.NetworkServer.Spawn(Bullet);
        RpcSetBulletVelocity(Bullet, direction);
    }

    [ClientRpc]
    void RpcSetBulletVelocity(GameObject bullet, Vector2 direction)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}
