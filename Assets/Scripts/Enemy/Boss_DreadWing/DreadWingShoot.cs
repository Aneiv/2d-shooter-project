
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreadWingShoot : EnemyShoot
{

    [Header("Third Phase")]
    public GameObject[] firePoints;
    public GameObject enemyBullet;
    public float bulletSpeed;
    public float initialShootingDelay;
    public float bulletSpawnDelay;
    public int numberOfBulletInBurst;

    private DreadWingEnemy DreadWingEnemy;
    public int numberOfChosenFP;

    public override void Start()
    {
        base.Start();
        DreadWingEnemy = GetComponent<DreadWingEnemy>();
    }
    public void StartShootingFromBelowDeck()
    {
        if (DreadWingEnemy.IsAlive())
        {
            StartCoroutine(ShootFromBelowDeckCoroutine());
        }
    }
    IEnumerator ShootFromBelowDeckCoroutine()
    {
        
        List<Transform> chosenFirePoints = ChooseFirePoints();

        yield return new WaitForSeconds(initialShootingDelay);

        for (int i = 0; i < numberOfBulletInBurst; i++)
        {
            if (DreadWingEnemy.IsAlive())
            {
                foreach (Transform firePoint in chosenFirePoints)
                {
                    SpawnBullet(firePoint);
                }
                yield return new WaitForSeconds(bulletSpawnDelay);
            }
        }
        StartShootingFromBelowDeck(); // loop
    }

    List<Transform> ChooseFirePoints()
    {
        int firePointsLength = firePoints.Length;
        List<int> chosenIndexes = new List<int>();
        while (chosenIndexes.Count < numberOfChosenFP && chosenIndexes.Count < firePointsLength)
        {
            int randIndex = UnityEngine.Random.Range(0, firePointsLength);
            if (!chosenIndexes.Contains(randIndex))
            {
                chosenIndexes.Add(randIndex);
            }
        }

        List<Transform> chosenFirePoints = new List<Transform>();
        foreach (int index in chosenIndexes)
        {
            chosenFirePoints.Add(firePoints[index].transform);
        }
        return chosenFirePoints;
    }

    void SpawnBullet(Transform firePoint)
    {
        float angleInDegrees = firePoint.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Quaternion bulletRotation = Quaternion.Euler(0f, 0f, angleInDegrees);
        GameObject Bullet = Instantiate(enemyBullet, firePoint.position, bulletRotation);

        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rbBullet = Bullet.GetComponent<Rigidbody2D>();
        rbBullet.linearVelocity = direction.normalized * bulletSpeed;
    }
}

