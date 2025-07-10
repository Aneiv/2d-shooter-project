using DG.Tweening;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    public GameObject[] enemiesPrefabs;     // Enemy Ship prefab
    public float moveBegingYPosition = 1f;   // first Y position
    public float animationDuration = 1.5f; // animation time (lower - faster)
    public float nextWaveTimeDelay; //delay before creating next wave

    private SpriteRenderer shipSpriteRenderer; //to get sizes of ships (in use of creating rows)
    private int shipCount;         // ship amount
    private int shipRows;        //rows amount
    private float spacing;        // x-axis ship spacing
    private int enemiesAmount = 0;
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
            UpDownSpawn,
            //to be made
        };

        //choose random wave style
        SpawnWave();
        //UpDownSpawn();
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
        shipRows = UnityEngine.Random.Range(1, 4);

        for (int i = 0; i < shipRows; i++)
        {
        //random enemies count per row
        shipCount = UnityEngine.Random.Range(1, 5);
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
                ship.transform.DOMoveY(4f - i * rowHeight, animationDuration)
                    .SetEase(Ease.OutQuad) //nice looking slowing down ships when near correct Y position
                    .SetDelay(i * 0.3f) //delay between spawning rows of ships
                    .OnComplete(() => {
                        var shipAnim = ship.GetComponent<Animator>();
                        var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
                        float randomOffset = UnityEngine.Random.Range(0f, 1f);
                        shipAnimator.Play("Idle",-1,randomOffset);
                    });
            }
        }
        WaveSpawned();
    }
}
