
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class KamikazeEnemy : EnemyShoot
{
    private Transform targetPlayer;
    private Enemy enemyScript;
    private Vector2 targetPosition;      //player position
    private Rigidbody2D rb;

    private Vector2 direction;
    private float targetAngle;

    public float speed = 5f;
    public float rotateSpeed = 200f;

    //borders
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private float leftXClamp, rightXClamp, downYClamp;
    private float clampSize = 0.5f;

    public override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyScript = GetComponent<Enemy>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }

        // borders
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        //clamp borders
        leftXClamp = bottomLeft.x - clampSize;
        rightXClamp = topRight.x + clampSize;
        downYClamp = bottomLeft.y - clampSize;
    }

    private void FixedUpdate()
    {
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
