using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpacecraftCarrierSpawner : MonoBehaviour
{
    public GameObject[] enemiesPrefabs;

    public GameObject spawnPointObj;
    public GameObject midPointLeftObj;
    public GameObject midPointRightObj;
    public GameObject[] positionsObj;

    public float spawnDelay = 0.5f;
    public float checkForAliveEnemiesDelay = 5f;
    private int enemiesCount;
    private float targetAngleDeg = 180f; //final destined rotation angle
    private GameObject enemiesContainer;
    public float animationDuration;

    private GameObject waveManager; // to tell NextWaveTrigger that enemies were spawned
    private SpacecraftCarrierEnemy SpacecraftCarrierEnemy;
    private Coroutine SpiralMovement;
    void Start()
    {
        enemiesContainer = GameObject.FindGameObjectWithTag("EnemiesContainer");
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        SpacecraftCarrierEnemy = GetComponent<SpacecraftCarrierEnemy>();
    }

    public void StartSpawningEnemies()
    {
        StartCoroutine(CheckForAliveEnemiesLoop());
    }

    IEnumerator CheckForAliveEnemiesLoop()
    {
        while (SpacecraftCarrierEnemy.IsAlive() && SpiralMovement == null)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            //Debug.Log($"Enemies globally: {enemies.Length}");
            if( enemies.Length <= 2) // 2 because enemies has 2 "Enemy" tag
            {
                SpiralMovement = StartCoroutine(SpiralMovementCoroutine());
            }

            yield return new WaitForSeconds(checkForAliveEnemiesDelay);
        }
    }

    void EnemiesWereSpawned()
    {
        SpiralMovement = null;
    }
    public IEnumerator SpiralMovementCoroutine()
    {
        Vector2 spawnPoint = spawnPointObj.transform.position;
        List<Vector2> randomPositions = GetRandomUniquePositions();
        enemiesCount = randomPositions.Count;

        bool goLeft = false;

        foreach(Vector2 endPoint in randomPositions) {
            Vector2 midPoint = goLeft ? midPointLeftObj.transform.position : midPointRightObj.transform.position;
            goLeft = !goLeft;

            // spawn enemy
            int enemyIndex = UnityEngine.Random.Range(0, enemiesPrefabs.Length);
            GameObject ship = Instantiate(enemiesPrefabs[enemyIndex], spawnPoint, Quaternion.identity);
            ship.transform.parent = enemiesContainer.transform; //make enemy child of 'EnemiesContainer'
            //rotate ship to correct value
            ship.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            if (waveManager.gameObject.TryGetComponent<NextWaveTrigger>(out var newWaveTrigger))
            {
                newWaveTrigger.AddToEnemyCounter(1);
            }

            //idle animation play at random delay for every ship
            var shipAnim = ship.GetComponent<Animator>();
            var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
            float randomOffset = UnityEngine.Random.Range(0f, 1f);
            shipAnimator.Play("Idle", 0, randomOffset);

            //movement animation start
            DOVirtual.Float(0f, 1f, animationDuration, (t) =>
            {
                //position on Bezier curve
                Vector2 pos = QuadraticBezier(spawnPoint, midPoint, endPoint, t);
                ship.transform.position = pos;

                if (t >= 0.998f)
                {
                    ship.transform.position = endPoint;
                    ship.transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
                    return;
                }
                
                Vector2 futurePos = QuadraticBezier(spawnPoint, midPoint, endPoint, Mathf.Min(t + 0.01f, 1f));
                Vector2 dir = (futurePos - pos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                ship.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90f offset
            })
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                IEnemy enemyInterface = ship.GetComponentInChildren<IEnemy>();
                if (enemyInterface != null)
                {
                    enemyInterface.OnArrival();
                }
            });
            yield return new WaitForSeconds(spawnDelay);
        }
        EnemiesWereSpawned();
    }
    //additional function for Bezier curve calculation
    Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(ab, bc, t);
    }

    List<Vector2> GetRandomUniquePositions()
    {
        int posLength = Random.Range(positionsObj.Length / 2, positionsObj.Length);

        List<GameObject> sourceList = new List<GameObject>(positionsObj);
        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < posLength; i++)
        {
            int randomIndex = Random.Range(0, sourceList.Count);
            result.Add(sourceList[randomIndex].transform.position);
            sourceList.RemoveAt(randomIndex);
        }

        return result;
    }
}
