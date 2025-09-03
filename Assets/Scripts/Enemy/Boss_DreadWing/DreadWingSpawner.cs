using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Net;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class DreadWingSpawner : MonoBehaviour
{
    [Header("Third Phase")]
    private DreadWingEnemy DreadWingEnemy;
    private GameObject enemiesContainer;
    private GameObject waveManager; // to tell NextWaveTrigger that enemies
    public GameObject enemyPrefab;

    public float animationDuration;

    public float minSpawnDelay;
    public float maxSpawnDelay;
    private float spawnDelay;

    private float spawnYPos = 8f;   // first Y position
    private float destYPos = 3f;

    private float minXPos = -2.5f;
    private float maxXPos = 2.5f;

    void Start()
    {
        enemiesContainer = GameObject.FindGameObjectWithTag("EnemiesContainer");
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        DreadWingEnemy = GetComponent<DreadWingEnemy>();
    }


    public void StartSpawningEnemies()
    {
        if (DreadWingEnemy.IsAlive())
        {
            spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            StartCoroutine(SpawnEnemiesCoroutine());
        }
    }

    IEnumerator SpawnEnemiesCoroutine()
    {
        Vector2 spawnPoint = new Vector2(Random.Range(minXPos, maxXPos), spawnYPos);
        Vector2 endPoint = new Vector2(Random.Range(minXPos, maxXPos), destYPos);
        Vector2 midPoint = new Vector2(endPoint.x, destYPos + 0.35f);

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
        enemy.transform.parent = enemiesContainer.transform; //make enemy child of 'EnemiesContainer'
        enemy.transform.rotation = Quaternion.Euler(0f, 0f, 180f);//rotate ship to correct value

        if (waveManager.gameObject.TryGetComponent<NextWaveTrigger>(out var newWaveTrigger))
        {
            newWaveTrigger.AddToEnemyCounter(1);
        }

        //movement animation start
        DOVirtual.Float(0f, 1f, animationDuration, (t) => { 
            //position on Bezier curve
            Vector2 pos = BezierCurve.Quadratic(spawnPoint, midPoint, endPoint, t);
            enemy.transform.position = pos;
            if (t >= 0.998f) { 
                enemy.transform.position = endPoint;
                enemy.transform.rotation = Quaternion.Euler(0, 0, 180f);
                return; 
            } 
            Vector2 futurePos = BezierCurve.Quadratic(spawnPoint, midPoint, endPoint, Mathf.Min(t + 0.01f, 1f));
            Vector2 dir = (futurePos - pos).normalized; 
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; 
            enemy.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90f offset
                                                                            }) 
            .SetEase(Ease.Linear)
            .OnComplete(() => { 
                Enemy enemyInstance = enemy.GetComponentInChildren<Enemy>();
                if (enemyInstance != null) {
                    enemyInstance.OnArrival(); 
                }
            });

            yield return new WaitForSeconds(spawnDelay);
        StartSpawningEnemies();
    }
}