using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class SpacecraftCarrierSpawner : MonoBehaviour
{
    public GameObject[] enemiesPrefabs;

    public GameObject spawnPointObj;
    public GameObject midPointLeftObj;
    public GameObject midPointRightObj;
    public GameObject[] positionsObj;
    public GameObject[] defencePositionsObj;

    public float spawnDelay = 0.5f;
    public float checkForAliveEnemiesDelay = 5f;
    private int enemiesCount;
    private float targetAngleDeg = 180f; //final destined rotation angle
    private GameObject enemiesContainer;
    public float animationDuration;

    private GameObject waveManager; // to tell NextWaveTrigger that enemies were spawned
    private SpacecraftCarrierEnemy SpacecraftCarrierEnemy;
    private Coroutine SpawnMovement;

    private Coroutine EnemyChangePossitions;
    private Coroutine InRestMovement;
    private bool IsInCircleMovement = false;
    private bool IsInChangePositionMovement = false;

    private GameObject[] enemiesInstances;
    private int[] currentEnemyPossIndexes;
    private EnemyPositionStatus enemyPositionStatus;

    private float inPossitionDuration;
    public float minInPossDuration = 5f;
    public float maxInPossDuration = 10f;

    private int[] inCircleMovementStepsPerShip;
    public int minInCircleMovSteps = 3;
    public int maxInCircleMovSteps = 8;

    void Start()
    {
        enemiesContainer = GameObject.FindGameObjectWithTag("EnemiesContainer");
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        SpacecraftCarrierEnemy = GetComponent<SpacecraftCarrierEnemy>();
    }

    public void StartSpawningEnemies()
    {
        StartCoroutine(CheckForAliveEnemiesCoroutine());
    }

    IEnumerator CheckForAliveEnemiesCoroutine()
    {
        while (SpacecraftCarrierEnemy.IsAlive() && SpawnMovement == null)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            //Debug.Log($"Enemies globally: {enemies.Length}");
            if (enemies.Length <= 2) // 2 because enemies has 2 "Enemy" tag
            {
                EnemyChangePossitions = null;
                SpawnMovement = StartCoroutine(SpawnEnemiesCoroutine());
            }

            yield return new WaitForSeconds(checkForAliveEnemiesDelay);
        }
    }

    void EnemiesWereSpawned()
    {
        SpawnMovement = null;
        enemyPositionStatus = EnemyPositionStatus.IN_POSITION;
        IsInCircleMovement = false;
        IsInChangePositionMovement = false;

        if (EnemyChangePossitions == null)
        {
            EnemyChangePossitions = StartCoroutine(EnemyChangePossitionsCoroutine());
        }
        
    }

    IEnumerator CallEnemiesWereSpawnedWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        EnemiesWereSpawned();
    }

    IEnumerator SpawnEnemiesCoroutine()
    {
        Vector2 spawnPoint = spawnPointObj.transform.position;
        List<Vector2> randomPositions = GetRandomUniquePositions();
        enemiesCount = randomPositions.Count;
        enemiesInstances = new GameObject[enemiesCount];

        bool goLeft = false;
        enemyPositionStatus = EnemyPositionStatus.SPAWNING;

        int shipIndex = 0;
        foreach(Vector2 endPoint in randomPositions) {

            if (!SpacecraftCarrierEnemy.IsAlive())
            {
                CancelSpawnEnemies();
            }

            Vector2 midPoint = goLeft ? midPointLeftObj.transform.position : midPointRightObj.transform.position;
            goLeft = !goLeft;
            Vector2 midPoint2 = endPoint + new Vector2(0f, 0.2f);

            // spawn enemy
            int enemyIndex = UnityEngine.Random.Range(0, enemiesPrefabs.Length);
            GameObject ship = Instantiate(enemiesPrefabs[enemyIndex], spawnPoint, Quaternion.identity);
            enemiesInstances[shipIndex] = ship;

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
            shipAnimator.Update(0);

            //movement animation start
            DOVirtual.Float(0f, 1f, animationDuration, (t) =>
            {
                //position on Bezier curve
                Vector2 pos = CubicBezier(spawnPoint, midPoint,midPoint2, endPoint, t);
                ship.transform.position = pos;

                if (t >= 0.998f)
                {
                    ship.transform.position = endPoint;
                    ship.transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
                    return;
                }
                
                Vector2 futurePos = CubicBezier(spawnPoint, midPoint,midPoint2, endPoint, Mathf.Min(t + 0.01f, 1f));
                Vector2 dir = (futurePos - pos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                ship.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90f offset
            })
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                Enemy enemyInstance = ship.GetComponentInChildren<Enemy>();
                if (enemyInstance != null)
                {
                    enemyInstance.OnArrival();
                }
                if(shipIndex == enemiesCount) // last enemy on possition
                {
                    StartCoroutine(CallEnemiesWereSpawnedWithDelay());
                }
            });
            yield return new WaitForSeconds(spawnDelay);

            shipIndex++;
        }
    }
    //additional function for Bezier curve calculation
    Vector2 CubicBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        Vector2 cd = Vector2.Lerp(c, d, t);

        Vector2 abc = Vector2.Lerp(ab, bc, t);
        Vector2 bcd = Vector2.Lerp(bc, cd, t);

        return Vector2.Lerp(abc, bcd, t);
    }

    public IEnumerator EnemyChangePossitionsCoroutine()
    {
        while (SpacecraftCarrierEnemy.IsAlive())
        {
            switch (enemyPositionStatus)
            {
                case EnemyPositionStatus.SPAWNING:
                    //Debug.Log("Enemy is spawning...");
                    break;

                case EnemyPositionStatus.IN_POSITION:
                    if(InRestMovement == null)
                    {
                        //Debug.Log("Enemy is resting...");
                        InRestMovement = StartCoroutine(InRestMovementCoroutine());
                    }
                    break;

                case EnemyPositionStatus.IN_DEFENCE:
                    if (InRestMovement == null)
                    {
                        //Debug.Log("Enemy is in defence...");
                        InRestMovement = StartCoroutine(InRestMovementCoroutine());
                    }
                    break;

                case EnemyPositionStatus.IN_CIRCLE:
                    if (!IsInCircleMovement)
                    {
                        //Debug.Log("Enemy is moving in circle...");
                        IsInCircleMovement = true;
                        GetRandomNumberOfInCircleSteps();
                        ForEveryShipDOTween(InCircleMovementDOTween);
                    }
                    break;

                case EnemyPositionStatus.TO_DEFENCE:
                    if (!IsInChangePositionMovement)
                    {
                        //Debug.Log("Enemy is changing position...");
                        IsInChangePositionMovement = true;
                        ForEveryShipDOTween(GoToPositionDOTween);
                    }
                    
                    break;
                case EnemyPositionStatus.TO_POSITION:
                    if (!IsInChangePositionMovement)
                    {
                        //Debug.Log("Enemy is changing position...");
                        IsInChangePositionMovement = true;
                        ForEveryShipDOTween(GoToPositionDOTween);
                    }

                    break;

                default:
                    break;
            }
            
            // check delay
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    void ForEveryShipDOTween(Action<GameObject, int> DOTweenMethod)
    {
        for (int shipIndex = 0; shipIndex < enemiesInstances.Length; shipIndex++)
        {
            GameObject ship = enemiesInstances[shipIndex];
            if (ship != null)
            {
                DOTweenMethod(ship, shipIndex);
            }
        }
    }

    void GetRandomNumberOfInCircleSteps()
    {
        inCircleMovementStepsPerShip = new int[positionsObj.Length];
        int steps = UnityEngine.Random.Range(minInCircleMovSteps, maxInCircleMovSteps);
        for (int i = 0; i < inCircleMovementStepsPerShip.Length; i++)
        {
            inCircleMovementStepsPerShip[i] = steps;
        }
    }

    void InCircleMovementDOTween(GameObject ship, int shipIndex)
    {
        int numberOfPossitions = positionsObj.Length;
        int currentIndex = currentEnemyPossIndexes[shipIndex];

        // sinusoidal movement
        float waveAmplitude = 0.02f;
        float waveFrequency = 4f;


        //movement animation start
        DOVirtual.Float(0f, 1f, animationDuration, (t) =>
        {
            // in case ship was destroyed
            if (ship == null || !SpacecraftCarrierEnemy.IsAlive()) return;

            //position
            GameObject startPossObj = positionsObj[currentIndex];
            Vector2 startPoss = startPossObj.transform.position;

            GameObject endPossObj = positionsObj[(currentIndex + 1) % numberOfPossitions];
            Vector2 endPoss = endPossObj.transform.position;

            Vector2 pos = Vector2.Lerp(startPoss, endPoss, t);
            Vector2 dir = (endPoss - startPoss).normalized;
            Vector2 perpendicular = new Vector2(-dir.y, dir.x);

            float offset = Mathf.Sin(t * Mathf.PI * waveFrequency) * waveAmplitude;

            pos += perpendicular * offset;

            ship.transform.position = pos;


            if (t >= 0.998f)
            {
                ship.transform.position = endPoss;
                ship.transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
                return;
            }
        })
        .SetEase(Ease.Linear)
        .SetTarget(ship)
        .OnComplete(() =>
        {
            if (ship == null) return;

            currentEnemyPossIndexes[shipIndex] = (currentIndex + 1) % numberOfPossitions;

            if (inCircleMovementStepsPerShip[shipIndex] > 0)
            {
                inCircleMovementStepsPerShip[shipIndex]--;
                InCircleMovementDOTween(ship, shipIndex);
            }
            else
            {
                enemyPositionStatus = EnemyPositionStatus.IN_POSITION;
                IsInCircleMovement = false;
            }
        });
    }

    void GoToPositionDOTween(GameObject ship, int shipIndex)
    {
        int currentIndex = currentEnemyPossIndexes[shipIndex];
        GameObject startPosObj = new();
        GameObject endPosObj = new();
        if (enemyPositionStatus == EnemyPositionStatus.TO_DEFENCE)
        {
            startPosObj = positionsObj[currentIndex];
            endPosObj = defencePositionsObj[currentIndex];
        }else if (enemyPositionStatus == EnemyPositionStatus.TO_POSITION)
        {
            startPosObj = defencePositionsObj[currentIndex];
            endPosObj = positionsObj[currentIndex];
        }

        Vector2 startPos = startPosObj.transform.position;
        Vector2 endPos = endPosObj.transform.position;


        //movement animation start
        DOVirtual.Float(0f, 1f, animationDuration, (t) =>
        {
            // in case ship was destroyed
            if (ship == null || !SpacecraftCarrierEnemy.IsAlive()) return;

            Vector2 pos = Vector2.Lerp(startPos, endPos, t);
            ship.transform.position = pos;

            if (t >= 0.998f)
            {
                ship.transform.position = endPos;
                ship.transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
                return;
            }
        })
        .SetEase(Ease.InOutSine)
        .SetTarget(ship)
        .OnComplete(() =>
        {
            if (ship == null) return;
            IsInChangePositionMovement = false;

            if (enemyPositionStatus == EnemyPositionStatus.TO_DEFENCE)
            {
                enemyPositionStatus = EnemyPositionStatus.IN_DEFENCE;
            }
            else if (enemyPositionStatus == EnemyPositionStatus.TO_POSITION)
            {
                enemyPositionStatus = EnemyPositionStatus.IN_POSITION;
            }
        });
    }

    IEnumerator InRestMovementCoroutine()
    {

        inPossitionDuration = UnityEngine.Random.Range(minInPossDuration, maxInPossDuration);

        yield return new WaitForSeconds(inPossitionDuration);

        bool randomChoice = UnityEngine.Random.value < 0.5f;
        if (enemyPositionStatus == EnemyPositionStatus.IN_POSITION)
        {
            enemyPositionStatus = randomChoice ? EnemyPositionStatus.TO_DEFENCE : EnemyPositionStatus.IN_CIRCLE;
        }
        else if (enemyPositionStatus == EnemyPositionStatus.IN_DEFENCE) {
            enemyPositionStatus = EnemyPositionStatus.TO_POSITION;
        }

        InRestMovement = null;
    }

    List<Vector2> GetRandomUniquePositions()
    {
        int posLength = UnityEngine.Random.Range(positionsObj.Length / 2, positionsObj.Length);

        // list of all available possition indexes
        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < positionsObj.Length; i++)
        {
            availableIndexes.Add(i);
        }

        List<Vector2> result = new List<Vector2>();
        currentEnemyPossIndexes = new int[posLength];

        for (int i = 0; i < posLength; i++)
        {
            int randomPos = UnityEngine.Random.Range(0, availableIndexes.Count);
            int chosenIndex = availableIndexes[randomPos];

            Vector2 position = positionsObj[chosenIndex].transform.position;
            result.Add(position);

            currentEnemyPossIndexes[i] = chosenIndex;

            availableIndexes.RemoveAt(randomPos);
        }

        return result;
    }

    void CancelSpawnEnemies()
    {
        if (SpawnMovement != null)
        {
            StopCoroutine(SpawnMovement);
            SpawnMovement = null;
        }
    }

    private enum EnemyPositionStatus
    {
        SPAWNING,
        TO_POSITION,
        IN_POSITION,
        IN_CIRCLE,
        TO_DEFENCE,
        IN_DEFENCE
    }
}
