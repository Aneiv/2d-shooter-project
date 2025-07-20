using TMPro;
using UnityEngine;

public class LightRocketBulletMovement : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public float rotateSpeed = 200f;

    private Rigidbody2D rb;
    private Vector2 targetPosition;      //player position
    private bool reachedTarget = false;
    public ParticleSystem burstParticle; //burst particle system
    public float explodeRadius; //objects in that area get damage from explosion
    //public float playerDamageZone; //radius of circle that designate bullet explosion


    private Vector2 direction;
    private float targetAngle;

    private float timer;
    public float rotationTime = 3f;

    public float maxRandomNoisePlayerPosition = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (target != null)
        {
            Vector2 playerPos = target.position;

            float randomNoisePos = Random.Range(-maxRandomNoisePlayerPosition, maxRandomNoisePlayerPosition);
            Vector2 noisyTarget = new Vector2(playerPos.x + randomNoisePos, playerPos.y + randomNoisePos);

            targetPosition = noisyTarget;  //saving position
            timer = rotationTime;
        }
    }

    void FixedUpdate()
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

        if(timer >= 0f)
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

        timer -= Time.deltaTime;
    }

    void Explode()
    {
        reachedTarget = true;
        //check other players in radius of explosion
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {                
                hit.GetComponent<Player>().TakeDamage(GetComponent<RocketBulletCollision>().damage);
                //Debug.Log("LOG Rocket Bullet Exploded near player damaging them");
            }
        }

        //later sound, etc
        ExplodeParticles();//particle explosion
        Destroy(gameObject); //destroy rocket
    }
    public void ExplodeParticles()
    {
        Instantiate(burstParticle, transform.position, Quaternion.identity).Play();
    }

    //show radius of exposion when selected on edit mode
    void OnDrawGizmosSelected()
    {
        //color
        Gizmos.color = Color.red;

        //explosion circle of bullet (all objects in that radius get damage
        Gizmos.DrawWireSphere(transform.position, explodeRadius);


        //bullet target
        Gizmos.color = Color.darkOrange;
        Gizmos.DrawSphere(targetPosition, 0.1f);
    }
}
