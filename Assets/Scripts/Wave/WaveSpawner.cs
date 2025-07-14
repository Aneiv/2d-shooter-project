using DG.Tweening;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    public GameObject[] enemiesPrefabs;     // Enemy Ship prefab
    private float moveBegingYPosition = 8f;   // first Y position
    public float nextWaveTimeDelay; //delay before creating next wave

    private SpriteRenderer shipSpriteRenderer; //to get sizes of ships (in use of creating rows)
    private int shipCount;         // ship amount
    private int shipRows;        //rows amount
    private float spacing;        // x-axis ship spacing
    private int enemiesAmount = 0;
    [Header("Wave spawn settings")]
    public float[] animationDurations; // animation time (lower - faster)
    public int shipRowsMin;
    public int shipRowsMax;
    public int shipPerRowMinAmount;
    public int shipPerRowMaxAmount;

    //Screen size
    private Vector3 bottomLeft;
    private Vector3 topRight;

    //spawn patterns
    private List<Action> spawnPatterns;
    void Start()
    {
        //Calculation of screen size
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        //Wave spawn
        //list of functions for spawn enemies
        spawnPatterns = new List<Action>
        {
            UpDownSpawn,        //animationDurations[0]
            SpiralMovement,     //animationDurations[1] ...
            //more to be made
        };

        //choose random wave style
        SpawnWave();
    }

    public void SpawnWave()
    {
        StartCoroutine(SpawnWaveWithDelay());
    }
    //random enemy wave appear style
    IEnumerator SpawnWaveWithDelay()
    {
        yield return new WaitForSeconds(nextWaveTimeDelay);//delay before new wave
        int index = UnityEngine.Random.Range(0, spawnPatterns.Count);
        spawnPatterns[index]?.Invoke();
    }
    //set enemies counter and reset local counter
    void WaveSpawned()
    {
        //NextWaveTrigger script reference
        var newWaveTrigger = GetComponent<NextWaveTrigger>();
        newWaveTrigger.SetRemainingEnemies(enemiesAmount);
        enemiesAmount = 0;//reset enemies amount
    }
    public void UpDownSpawn()
    {
        //Random wave rows amount
        shipRows = UnityEngine.Random.Range(shipRowsMin, shipRowsMax);

        for (int i = 0; i < shipRows; i++)
        {
            //random enemies count per row
            shipCount = UnityEngine.Random.Range(shipPerRowMinAmount, shipPerRowMaxAmount);
            //x-axis spacing calculation between ships in rows
            spacing = (topRight.x - bottomLeft.x) / shipCount;
            float startX = -(shipCount - 1) * spacing / 2f; // ships spacing and centering

            for (int j = 0; j < shipCount; j++)
            {
                //Debug.Log($"Row {i} :enemies {j} ");
                enemiesAmount++;//counting enemies
                //random enemy type
                int enemyIndex = UnityEngine.Random.Range(0, enemiesPrefabs.Length);
                //get SpriteRenderer of that ship
                shipSpriteRenderer = enemiesPrefabs[enemyIndex].transform.Find("EnemyVisual").GetComponent<SpriteRenderer>();

                // start position calculation
                Vector3 spawnPos = new Vector3(startX + j * spacing, moveBegingYPosition, 0f);

                //create ship instance and set position
                GameObject ship = Instantiate(enemiesPrefabs[enemyIndex], spawnPos, Quaternion.identity);
                //rotate ship to correct value
                ship.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                float rowHeight = 1.0f;
                //bounds - ingame units
                //DOMoveY(endY,duration)
                //ship.transform.DOMoveY(4f - i * shipSpriteRenderer.bounds.size.y, animationDuration)
                ship.transform.DOMoveY(4f - i * rowHeight, animationDurations[0])
                    .SetEase(Ease.OutQuad) //nice looking slowing down ships when near correct Y position
                    .SetDelay(i * 0.3f) //delay between spawning rows of ships
                    .OnComplete(() =>
                    {
                        //animation play at random delay for every ship
                        var shipAnim = ship.GetComponent<Animator>();
                        var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
                        float randomOffset = UnityEngine.Random.Range(0f, 1f);
                        shipAnimator.Play("Idle", -1, randomOffset);
                    });
            }
        }
        WaveSpawned();
    }

    public void SpiralMovement()
    {
        //random direction (left or right)
        bool fromLeft = UnityEngine.Random.value < 0.5f;
        //Random wave rows amount
        shipRows = UnityEngine.Random.Range(shipRowsMin, shipRowsMax);
        //rows spacing
        float rowHeight = 1.0f;

        for (int i = 0; i < shipRows; i++)
        {
            //random enemies count per row
            shipCount = UnityEngine.Random.Range(shipPerRowMinAmount, shipPerRowMaxAmount);

            //x-axis spacing calculation between ships in rows
            spacing = (topRight.x - bottomLeft.x) / shipCount;
            float startX = -(shipCount - 1) * spacing / 2f; // ships spacing and centering

            for (int j = 0; j < shipCount; j++)
            {
                enemiesAmount++; //enemies amount counting
                int enemyIndex = UnityEngine.Random.Range(0, enemiesPrefabs.Length);   //random enemy type index
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

                //ship instance creation
                GameObject ship = Instantiate(enemiesPrefabs[enemyIndex], start, Quaternion.identity);
                //rotate ship to correct value
                ship.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                //animation start
                DOVirtual.Float(0f, 1f, animationDurations[1], (t) =>
                {
                    //position on Bezier curve
                    Vector2 pos = QuadraticBezier(start, control, end, t);
                    ship.transform.position = pos; //ship position change

                    //future position calculation (for place and rotation prediction)
                    Vector2 futurePos = QuadraticBezier(start, control, end, t + 0.01f);
                    Vector2 dir = (futurePos - pos).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; //angle calculation

                    //rotation adjusted to curve with sprite offset
                    ship.transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90f offset
                })
                .SetEase(Ease.InOutSine) //make smooth begin and end of animation
                .OnComplete(() => //after animation end
                {
                    //correct ship rotation
                    ship.transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
                    //animation play at random delay for every ship
                    var shipAnim = ship.GetComponent<Animator>();
                    var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
                    float randomOffset = UnityEngine.Random.Range(0f, 1f);
                    shipAnimator.Play("Idle", -1, randomOffset);
                });
            }
        }
        //additional function for Bezier curve calculation
        Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            Vector2 ab = Vector2.Lerp(a, b, t);
            Vector2 bc = Vector2.Lerp(b, c, t);
            return Vector2.Lerp(ab, bc, t);
        }
        WaveSpawned();
    }
}