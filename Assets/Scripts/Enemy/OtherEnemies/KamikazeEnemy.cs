
using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KamikazeEnemy : EnemyShoot
{
    private Enemy enemyScript;
    private Rigidbody2D rb;

    protected List<Transform> targetPlayers = new List<Transform>();
    [SyncVar] private Transform targetPlayer;
    [SyncVar] private Vector2 targetPosition;      //player position

    [SyncVar] private Vector2 direction;
    private float targetAngle;

    public float speed = 5f;
    public float rotateSpeed = 200f;

    //borders
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private float leftXClamp, rightXClamp, downYClamp;
    private float clampSize = 0.5f;

    [Server]
    public override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyScript = GetComponent<Enemy>();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            foreach (GameObject p in players)
            {
                if (p != null)
                    targetPlayers.Add(p.transform);
            }
        }
        GetRandomPlayerTarget();

        // borders
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        //clamp borders
        leftXClamp = bottomLeft.x - clampSize;
        rightXClamp = topRight.x + clampSize;
        downYClamp = bottomLeft.y - clampSize;
    }

    [Server]
    public void GetRandomPlayerTarget()
    {
        // filters dead players
        targetPlayers.RemoveAll(p => p == null || !p.GetComponent<Player>().isAlive);

        if (targetPlayers.Count > 0)
        {
            int index = Random.Range(0, targetPlayers.Count);
            targetPlayer = targetPlayers[index];
        }
        else
        {
            targetPlayer = null;
        }
    }

    [Server]
    private void FixedUpdate()
    {
        if (targetPlayer != null)
        {
            if (!targetPlayer.GetComponent<Player>().isAlive)
            {
                GetRandomPlayerTarget();
            }
        }

        if (!waiting && targetPlayer != null)
        {
            Vector2 pos = rb.position;
            if (pos.x < leftXClamp || pos.x > rightXClamp || pos.y < downYClamp)
            {
                if (enemyScript != null)
                {
                    enemyScript.Die();
                }
            }

            // head to player
            targetPosition = targetPlayer.position;

            //destination
            Vector2 toTarget = targetPosition - rb.position;
            direction = toTarget.normalized;

            targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            //rotation
            float rotation = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotateSpeed * Time.deltaTime);
            rb.MoveRotation(rotation);

            //movement
            rb.linearVelocity = transform.up * speed;
        }
    }
}
