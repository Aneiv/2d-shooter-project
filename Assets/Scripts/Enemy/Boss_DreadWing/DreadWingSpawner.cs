using DG.Tweening;
using System.Collections;
using UnityEngine;
using Mirror;

public class DreadWingSpawner : Mirror.NetworkBehaviour
{
    [Header("Third Phase")]
    private DreadWingEnemy DreadWingEnemy;
    private GameObject enemiesContainer;
    private GameObject waveManager; // to tell NextWaveTrigger that enemies
    public GameObject enemyPrefab;

    public float animationDuration;

    public float minSpawnDelay;
    public float maxSpawnDelay;
    [SyncVar] private float spawnDelay;

    private const float spawnYPos = 8f;   // first Y position
    private const float destYPos = 3f;

    private const float minXPos = 1.5f;
    private const float maxXPos = 2.5f;

    [SyncVar] private bool spawnFromLeft = false;

    [Server]
    void Start()
    {
        enemiesContainer = GameObject.FindGameObjectWithTag("EnemiesContainer");
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        DreadWingEnemy = GetComponent<DreadWingEnemy>();
    }

    [Server]
    public void StartSpawningEnemies()
    {
        if (DreadWingEnemy.IsAlive())
        {
            spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            StartCoroutine(SpawnEnemiesCoroutine());
        }
    }

    [Server]
    IEnumerator SpawnEnemiesCoroutine()
    {
        float spawnXPos = Random.Range(minXPos, maxXPos);
        float endXPos = Random.Range(0f, minXPos);

        if (spawnFromLeft)
        {
            spawnXPos = -spawnXPos;
            endXPos = -endXPos;
        }
        spawnFromLeft = !spawnFromLeft;

        Vector2 spawnPoint = new Vector2(spawnXPos, spawnYPos);
        Vector2 endPoint = new Vector2(endXPos, destYPos);
        Vector2 controlPoint = new Vector2(
            (spawnPoint.x + endPoint.x) / 2f,
            4f
        );

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.Euler(0f, 0f, 180f));
        enemy.transform.parent = enemiesContainer.transform; //make enemy child of 'EnemiesContainer'
        // server spawn
        Mirror.NetworkServer.Spawn(enemy);

        if (waveManager.gameObject.TryGetComponent<NextWaveTrigger>(out var newWaveTrigger))
        {
            newWaveTrigger.AddToEnemyCounter(1);
        }

        Vector3[] path = new Vector3[] { spawnPoint, controlPoint, endPoint };

        enemy.transform
            .DOPath(path, animationDuration, PathType.CatmullRom)
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