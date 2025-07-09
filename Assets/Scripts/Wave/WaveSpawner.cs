using DG.Tweening;
using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public GameObject shipPrefab;     // Enemy Ship prefab
    private SpriteRenderer shipSpriteRenderer; //to get sizes of ships (in use of creating rows)
    public int shipCount = 5;         // ship amount
    public int shipRows = 2;        //rows amount
    private float spacing;        // x-axis ship spacing
    public float moveBegingYPosition = 1f;   // first Y position
    public float animationDuration = 1.5f; // animation time (lower - faster)

    //Screen size
    private Vector3 bottomLeft;
    private Vector3 topRight;

    void Start()
    {
        //Calculation of screen size
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        //x-axis spacing calculation between ships in rows
        spacing = (topRight.x - bottomLeft.x) / shipCount;
        //get SpriteRenderer of that ship
        shipSpriteRenderer = shipPrefab.transform.Find("EnemyVisual").GetComponent<SpriteRenderer>();

        //Wave spawn
        UpDownSpawn();
    }

    void UpDownSpawn()
    {
        float startX = -(shipCount - 1) * spacing / 2f; // ships spacing and centering
        for (int i = 0; i < shipRows; i++)
        {

            for (int j = 0; j < shipCount; j++)
            {
                // start position calculation
                Vector3 spawnPos = new Vector3(startX + j * spacing, moveBegingYPosition, 0f);

                //create ship instance and set position
                GameObject ship = Instantiate(shipPrefab, spawnPos, Quaternion.identity);
                //rotate ship to correct value
                ship.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                //bounds - ingame units
                //DOMoveY(endY,duration)
                ship.transform.DOMoveY(4f - i * 1.2f * shipSpriteRenderer.bounds.size.y, animationDuration)
                    .SetEase(Ease.OutQuad) //nice looking slowing down ships when near correct Y position
                    .SetDelay(i * 0.3f) //delay between spawning rows of ships
                    .OnComplete(() => {
                        //var shipAnim = ship.GetComponent<Animator>();
                        //var shipAnimator = ship.transform.Find("EnemyVisual").GetComponent<Animator>();
                        //float randomOffset = Random.Range(0f, 1f);
                        //shipAnimator.Play("Idle",-1,randomOffset);
                    });
            }
        }
    }
}
