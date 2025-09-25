using DG.Tweening;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using Mirror;

public class WaveSpawner : Mirror.NetworkBehaviour
{
    public GameObject[] enemyTestPrefabs;     // Enemy Ship prefab
    public GameObject minibossPrefab; // Mini boss prefab
    public GameObject bossPrefab;
    private float moveBegingYPosition = 8f;   // first Y position
    public float nextWaveTimeDelay; //delay before creating next wave
    private SpriteRenderer shipSpriteRenderer; //to get sizes of ships (in use of creating rows)
    private int shipCount;         // ship amount
    private int shipRows;        //rows amount
    private float spacing;        // x-axis ship spacing
    private int enemiesAmount = 0;
    private float rowHeight = 1.0f;
    [Header("Wave spawn settings")]
    public float[] animationDurations; // animation time (lower - faster)
    public int shipRowsMin;
    public int shipRowsMax;
    public int shipPerRowMinAmount;
    public int shipPerRowMaxAmount;
    public GameObject enemiesContainer;
    //Screen size
    [SyncVar] private Vector3 bottomLeft;
    [SyncVar] private Vector3 topRight;

    //spawn patterns
    private List<Action> spawnPatterns;

    private BossHealthBar bossHealthBar; // UI hp bar

    [Header("Wave spawner with increased difficulty level")]
    private int limitOfEnemiesPerWave = 15;
    public bool testMode;
    public DifficultyClass[] enemyDifficultyClasses;

    public int minNumberOfWaves = 13;
    public int maxNumberOfWaves = 16;
    private int numberOfWaves;

    private int[] budgetCurve;
    private float[,] setOfProbDiffClassPerWave;
    private List<Action> waves;
    private GameObject[] currentChosenPrefabs;
    private List<Action> standardSpawnPatterns;
    private int waveCounter = 0;
    void Start()
    {
        //Calculation of screen size
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        //Wave spawn
        if (testMode)
        {
            //list of functions for spawn enemies
            spawnPatterns = new List<Action>
            {
                //UpDownSpawn,        //animationDurations[0]
                //SpiralMovement,     //animationDurations[1] ...
                //SpawnMiniBoss,
                SpawnBoss
                //more to be made
            };
        }
        else
        {
            standardSpawnPatterns = new List<Action>
            {
                UpDownSpawn,
                //SpiralMovement
            };
            PrepareWaves();
        }
            GameObject bossBarObj = GameObject.FindGameObjectWithTag("BossHealthBar");
        if (bossBarObj != null)
        {
            bossHealthBar = bossBarObj.GetComponent<BossHealthBar>();
        }

        //choose random wave style
        SpawnWave();
    }

    private void PrepareWaves()
    {
        numberOfWaves = UnityEngine.Random.Range(minNumberOfWaves, maxNumberOfWaves);

        Debug.Log($"Created {numberOfWaves} waves");

        // budget curve
        budgetCurve = new int[numberOfWaves];

        float baseNumber = 5;
        float factorNumber = 1.15f;
        int shiftNumber = maxNumberOfWaves - numberOfWaves;
        for (int wave = 0; wave < numberOfWaves; wave++) {
            budgetCurve[wave] = (int)(baseNumber * Math.Pow(factorNumber, wave + 1 + shiftNumber));
        }

        // linearly interpolates firstWaveProbability → lastWaveProbability
        int numberOfDiffLevels = enemyDifficultyClasses.Length;
        setOfProbDiffClassPerWave = new float[numberOfDiffLevels, numberOfWaves];
        for (int wave = 0; wave < numberOfWaves; wave++)
        {
            float t = (numberOfWaves <= 1) ? 0f : (float)wave / (float)(numberOfWaves - 1);

            for (int i = 0; i < numberOfDiffLevels; i++)
            {
                float start = enemyDifficultyClasses[i].firstWaveProbability;
                float end = enemyDifficultyClasses[i].lastWaveProbability;

                float prob = Mathf.Lerp(start, end, t);
                setOfProbDiffClassPerWave[i, wave] = prob;
            }
        }

        // set waves on their positions
        waves = new List<Action>(numberOfWaves);
        for (int i = 0; i < numberOfWaves; i++)
            waves.Add(null);

        // boss 
        waves[numberOfWaves - 1] = SpawnBoss;

        // miniboss
        int miniBossIndex1 = UnityEngine.Random.Range(numberOfWaves / 2 - 2, numberOfWaves / 2 + 2);
        int miniBossIndex2 = miniBossIndex1 + UnityEngine.Random.Range(2, 3);
        waves[miniBossIndex1] = SpawnMiniBoss;
        waves[miniBossIndex2] = SpawnMiniBoss;

    }

    public void SpawnWave()
    {
        if (testMode)
        {
            StartCoroutine(SpawnRandomWaveWithDelay());
        }
        else
        {
            StartCoroutine(SpawnWaveWithDelay());
        }
    }

    //random enemy wave appear style
    IEnumerator SpawnRandomWaveWithDelay()
    {
        yield return new WaitForSeconds(nextWaveTimeDelay);//delay before new wave
        int index = UnityEngine.Random.Range(0, spawnPatterns.Count);
        spawnPatterns[index]?.Invoke();
    }

    // spawn wave 
    IEnumerator SpawnWaveWithDelay()
    {
        yield return new WaitForSeconds(nextWaveTimeDelay);//delay before new wave
        // game over - no more waves
        if(waveCounter >= waves.Count)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Player player = playerObj.GetComponent<Player>();
                if (player != null)
                {
                    player.OnGameOver();
                }
            }
        }
        else // next wave
        {
            if (waves[waveCounter] == null)
            {
                currentChosenPrefabs = PickEnemyPrefabs(waveCounter);

                // chose standard wave patterns 
                int randomIndex = UnityEngine.Random.Range(0, standardSpawnPatterns.Count);
                Action chosenPattern = standardSpawnPatterns[randomIndex];

                waves[waveCounter] = chosenPattern;
            }
            waves[waveCounter]?.Invoke();
            waveCounter++;
        }
    }

    // pick enemy prefabs for wave
    GameObject[] PickEnemyPrefabs(int waveIndex)
    {
        int budget = budgetCurve[waveIndex];
        List<GameObject> chosenPrefabs = new List<GameObject>();
        int enemiesCount = 0;

        //Debug.Log($"Wave {waveIndex}, budget: {budget}");
        //Debug.Log($"Prob: easy: {setOfProbDiffClassPerWave[0, waveIndex]}, medium: {setOfProbDiffClassPerWave[1, waveIndex]}, hard: {setOfProbDiffClassPerWave[2, waveIndex]}");

        while (budget > 0 && enemiesCount < limitOfEnemiesPerWave)
        {
            // get difficulty level class Enemy
            int diffIndex = PickDifficultyClassIndex(waveIndex);
            DifficultyClass diffClass = enemyDifficultyClasses[diffIndex];

            // check for budget
            if (diffClass.cost <= budget)
            {
                // random prefab
                int prefabIndex = UnityEngine.Random.Range(0, diffClass.enemyPrefabs.Length);
                GameObject prefab = diffClass.enemyPrefabs[prefabIndex];

                chosenPrefabs.Add(prefab);
                budget -= diffClass.cost;
                enemiesCount++;
            }
            else
            {
                break;
            }
        }

        return chosenPrefabs.ToArray();
    }

    // get random difficulty class 
    private int PickDifficultyClassIndex(int waveIndex)
    {
        float totalProb = 0f;
        int numberOfDiffClasses = enemyDifficultyClasses.Length;

        for (int i = 0; i < numberOfDiffClasses; i++)
            totalProb += setOfProbDiffClassPerWave[i, waveIndex];

        float randomProb = UnityEngine.Random.Range(0f, totalProb);
        float cumulative = 0f;

        for (int i = 0; i < numberOfDiffClasses; i++)
        {
            cumulative += setOfProbDiffClassPerWave[i, waveIndex];
            if (randomProb <= cumulative)
                return i;
        }

        return numberOfDiffClasses - 1;
    }


    //set enemies counter and reset local counter
    void WaveSpawned()
    {
        //NextWaveTrigger script reference
        var newWaveTrigger = GetComponent<NextWaveTrigger>();
        newWaveTrigger.SetRemainingEnemies(enemiesAmount);
        enemiesAmount = 0;//reset enemies amount
    }

    private (GameObject[] prefabs, int count) GetEnemyPrefabs()
    {
        GameObject[] enemyPrefabs;
        int numberOfEnemies;

        if (testMode)
        {
            numberOfEnemies = UnityEngine.Random.Range(1, limitOfEnemiesPerWave);

            enemyPrefabs = new GameObject[numberOfEnemies];
            for (int i = 0; i < numberOfEnemies; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, enemyTestPrefabs.Length);
                enemyPrefabs[i] = enemyTestPrefabs[randomIndex];
            }

        }
        else
        {
            enemyPrefabs = currentChosenPrefabs;
            numberOfEnemies = enemyPrefabs.Length;
        }
        // wave rows amount
        shipRows = (int)Math.Ceiling((float)numberOfEnemies / shipPerRowMaxAmount);

        return (enemyPrefabs, numberOfEnemies);
    }

    private float SetPrefabSpacing(int numberOfEnemies, int enemyPrefabIndex, int row)
    {
        // row calculations
        int remainingEnemies = numberOfEnemies - enemyPrefabIndex;
        if (row == shipRows - 1)
        {
            shipCount = remainingEnemies;
        }
        else
        {
            shipCount = Math.Min(shipPerRowMaxAmount, remainingEnemies);
        }

        //x-axis spacing calculation between ships in rows
        spacing = (topRight.x - bottomLeft.x) / shipCount;
        float startX = -(shipCount - 1) * spacing / 2f; // ships spacing and centering

        return startX;
    }
    [Server]
    public void UpDownSpawn()
    {
        var (enemyPrefabs, numberOfEnemies) = GetEnemyPrefabs();

        int enemyPrefabIndex = 0;

        for (int i = 0; i < shipRows; i++)
        {
            float startX = SetPrefabSpacing(numberOfEnemies, enemyPrefabIndex, i);

            for (int j = 0; j < shipCount; j++)
            {
                enemiesAmount++;//counting enemies

                // start position calculation
                Vector3 spawnPos = new Vector3(startX + j * spacing, moveBegingYPosition, 0f);

                //create ship instance and set position
                GameObject enemyPrefab = enemyPrefabs[enemyPrefabIndex];
                enemyPrefabIndex++;
                GameObject ship = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                ship.transform.parent = enemiesContainer.transform; //make enemy child of 'EnemiesContainer'
                //rotate ship to correct value
                ship.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                Mirror.NetworkServer.Spawn(ship);

                //idle animation play at random delay for every ship
                var shipAnim = ship.GetComponent<Animator>();
                var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
                float randomOffset = UnityEngine.Random.Range(0f, 1f);//0 - animation start   1 - animation end
                shipAnimator.Play("Idle", 0, randomOffset);//layer 0

                int rowIndex = i;
                RpcUpDownSpawnAnimation(rowIndex, animationDurations[0], ship);
            }
        }
        WaveSpawned();
    }
    [ClientRpc]
    void RpcUpDownSpawnAnimation(int rowIndex, float animationDuration, GameObject ship)
    {
        float targetY = 4f - rowIndex * rowHeight;
        float delay = rowIndex * 0.3f;

        ship.transform.DOMoveY(targetY, animationDurations[0])
            .SetEase(Ease.OutQuad)
            .SetDelay(delay)
            .OnComplete(() =>
            {
                Enemy enemyInstance = ship.GetComponentInChildren<Enemy>();
                if (enemyInstance != null)
                {
                    enemyInstance.OnArrival();
                }
            });
    }

    [Server]
    public void SpawnMiniBoss()
    {
        shipCount = 1;
        float startX = 0f;

        enemiesAmount = shipCount;

        //get SpriteRenderer of that ship
        shipSpriteRenderer = minibossPrefab.transform.Find("EnemyVisual").GetComponent<SpriteRenderer>();

        // start position calculation
        Vector3 spawnPos = new Vector3(startX, moveBegingYPosition, 0f);

        //create ship instance and set position
        GameObject ship = Instantiate(minibossPrefab, spawnPos, Quaternion.Euler(0f, 0f, 180f));
        Mirror.NetworkServer.Spawn(ship);
        ship.transform.parent = enemiesContainer.transform; //make enemy child of 'EnemiesContainer'

        //idle animation play at random delay for every ship
        var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
        if(shipAnimator != null)
        {
            float randomOffset = UnityEngine.Random.Range(0f, 1f);//0 - animation start   1 - animation end
            shipAnimator.Play("Idle", 0, randomOffset);//layer 0
        }

        RpcAnimMiniBoss(ship);
        StartCoroutine(TriggerArrivalWithDelay(ship, animationDurations[0] + 0.3f));
        WaveSpawned();
    }

    [ClientRpc]
    void RpcAnimMiniBoss(GameObject ship)
    {
        float endYPosition = 3f;

        ship.transform.DOMoveY(endYPosition, animationDurations[0])
            .SetEase(Ease.OutQuad) //nice looking slowing down ships when near correct Y position
            .SetDelay(0.3f); //delay between spawning rows of ships
    }

    [Server]
    IEnumerator TriggerArrivalWithDelay(GameObject ship, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ship != null)
        {
            var enemy = ship.GetComponentInChildren<Enemy>();
            if (enemy != null)
            {
                enemy.OnArrival();
            }
        }
    }

    [Server]
    public void SpawnBoss()
    {
        shipCount = 1;
        float startX = 0f;
        float endYPosition = 3f;
        enemiesAmount = shipCount;

        //get SpriteRenderer of that ship
        shipSpriteRenderer = bossPrefab.transform.Find("EnemyVisual").GetComponent<SpriteRenderer>();

        // start position calculation
        Vector3 spawnPos = new Vector3(startX, moveBegingYPosition, 0f);

        //create ship instance and set position
        GameObject ship = Instantiate(bossPrefab, spawnPos, Quaternion.Euler(0f, 0f, 180f));
        Mirror.NetworkServer.Spawn(ship);
        ship.transform.parent = enemiesContainer.transform; //make enemy child of 'EnemiesContainer'

        //idle animation play at random delay for every ship
        
        var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
        if(shipAnimator != null)
        {
            float randomOffset = UnityEngine.Random.Range(0f, 1f);//0 - animation start   1 - animation end
            shipAnimator.Play("Idle", 0, randomOffset);//layer 0
        }
        //Appear Animation
        ship.transform.DOMoveY(endYPosition, animationDurations[3])
            .SetEase(Ease.OutQuad) //nice looking slowing down ships when near correct Y position
            .SetDelay(0.3f) //delay between spawning rows of ships
            .OnComplete(() =>
            {
                Enemy enemyInstance = ship.GetComponentInChildren<Enemy>();
                if (enemyInstance != null)
                {
                    enemyInstance.OnArrival();
                }

                RpcShowBossHealthBar();
            });
        WaveSpawned();
    }

    [ClientRpc]
    private void RpcShowBossHealthBar()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.Show();
        }
    }

    public void SpiralMovement()
    {
        //random direction (left or right)
        bool fromLeft = UnityEngine.Random.value < 0.5f;

        var (enemyPrefabs, numberOfEnemies) = GetEnemyPrefabs();
        int enemyPrefabIndex = 0;

        for (int i = 0; i < shipRows; i++)
        {
            float startX = SetPrefabSpacing(numberOfEnemies, enemyPrefabIndex, i);

            for (int j = 0; j < shipCount; j++)
            {
                enemiesAmount++; //enemies amount counting
                //start position of movement
                Vector2 start = new Vector2(-3.5f, 1f);
                //end position of movement
                Vector2 end = new Vector2(startX + j * spacing, 4f - i * rowHeight);

                float targetAngleDeg = 180f; //final destined rotation angle
                if (fromLeft)
                {
                    start = new Vector2(-3.5f, 1f); //enter from left
                }
                else
                {
                    start = new Vector2(3.5f, 1f);  //enter from right
                    end.x = -end.x; //mirror position for symmetry
                }

                float controlHeight = 4f;  //how far above destination point place control point (changes trajectory)
                Vector2 control = end + Vector2.up * controlHeight; //control point position calculation                

                //create ship instance and set position
                GameObject enemyPrefab = enemyPrefabs[enemyPrefabIndex];
                enemyPrefabIndex++;
                GameObject ship = Instantiate(enemyPrefab, start, Quaternion.identity);
                Mirror.NetworkServer.Spawn(ship);
                ship.transform.parent = enemiesContainer.transform; //make enemy child of 'EnemiesContainer'
                //rotate ship to correct value
                ship.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

                //idle animation play at random delay for every ship
                var shipAnim = ship.GetComponent<Animator>();
                var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
                float randomOffset = UnityEngine.Random.Range(0f, 1f);//0 - animation start   1 - animation end
                shipAnimator.Play("Idle", 0, randomOffset);//layer 0

                //movement animation start
                DOVirtual.Float(0f, 1f, animationDurations[1], (t) =>
                {
                    //position on Bezier curve
                    Vector2 pos = BezierCurve.Quadratic(start, control, end, t);
                    ship.transform.position = pos; //ship position change

                    if (t >= 0.998f)
                    {
                        ship.transform.position = end;
                        ship.transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
                        return;
                    }
                    //future position calculation (for place and rotation prediction)
                    //Vector2 futurePos = QuadraticBezier(start, control, end, t + 0.01f);
                    Vector2 futurePos = BezierCurve.Quadratic(start, control, end, Mathf.Min(t + 0.01f, 1f));

                    Vector2 dir = (futurePos - pos).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; //angle calculation

                    //rotation adjusted to curve with sprite offset
                    ship.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90f offset
                })
                .SetEase(Ease.InOutSine) //make smooth begin and end of animation
                .OnComplete(() => //after animation end
                {
                    Enemy enemyInstance = ship.GetComponentInChildren<Enemy>();
                    if (enemyInstance != null)
                    {
                        enemyInstance.OnArrival();
                    }

                    //correct ship rotation
                    //ship.transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
                });
            }
        }
        WaveSpawned();
    }

    [System.Serializable]
    public struct DifficultyClass
    {
        public string name;
        public int cost;
        public GameObject[] enemyPrefabs;
        [Range(0f, 1f)] public float firstWaveProbability;
        [Range(0f, 1f)] public float lastWaveProbability;
    }
}