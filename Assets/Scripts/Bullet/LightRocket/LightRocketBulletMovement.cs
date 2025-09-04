using TMPro;
using UnityEngine;

public class LightRocketBulletMovement : RocketBulletMovement
{
    protected float targetAngle;

    protected float rotationTimer;
    public float rotationTime = 3f;

    public float maxRandomNoisePlayerPosition = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        if (target != null)
        {
            Vector2 playerPos = target.position;

            float randomNoisePos = Random.Range(-maxRandomNoisePlayerPosition, maxRandomNoisePlayerPosition);
            Vector2 noisyTarget = new Vector2(playerPos.x + randomNoisePos, playerPos.y + randomNoisePos);

            targetPosition = noisyTarget;  //saving position
            rotationTimer = rotationTime;
        }
    }

    protected override void FixedUpdate()
    {
        if (reachedTarget) return;
        if (target == null) return;

        

        //check if player in radius of explosion
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Explode();
                break;
            }
        }

        if(rotationTimer >= 0f)
        {
            //destination
            Vector2 toTarget = targetPosition - rb.position;
            direction = toTarget.normalized;

            targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        }

        //rotation
        float rotation = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotateSpeed * Time.deltaTime);
        rb.MoveRotation(rotation);


        //movement
        rb.linearVelocity = transform.up * speed;

        rotationTimer -= Time.deltaTime;
    }
}
