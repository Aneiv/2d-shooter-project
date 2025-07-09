using TMPro;
using UnityEngine;

public class RocketBulletMovement : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;
    public float rotateSpeed = 200f;

    private Rigidbody2D rb;
    private Vector2 targetPosition;      //player position
    private bool reachedTarget = false;
    public float explodeRadius; //objects in that area get damage from explosion
    //public float playerDamageZone; //radius of circle that designate bullet explosion

    private Vector2 direction;
    private bool bouncingUp = false;

    float screenCenterYPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (target != null)
        {
            targetPosition = target.position;  //saving position
        }
        //center of screen y ingame position
        //Calculation of screen size
        Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        screenCenterYPos = screenCenter.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (reachedTarget) return;
        if (target == null) return;

        //if bullet position is greater than half of screen then bullet is tracking player
        if (rb.position.y >= screenCenterYPos)
        {
            targetPosition = target.position;
        }

        //destination
        Vector2 toTarget = targetPosition - rb.position;
        //float distance = toTarget.magnitude;

        //bounce and set new target
        if (bouncingUp)
        {
            bouncingUp = false;
            targetPosition = target.position;
            return;
        }
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
        direction = toTarget.normalized;

        //rotation
        float rotateAmount = Vector3.Cross(direction, transform.up).z;

        rb.rotation -= rotateAmount * rotateSpeed * Time.deltaTime;

        //movement
        rb.linearVelocity = transform.up * speed;
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
                Debug.Log("LOG Rocket Bullet Exploded near player damaging them");
            }
        }

        //later particle, sound, etc

        Destroy(gameObject); //destroy rocket
    }
    public void Bounce(Transform playerBulletTransform)
    {
        //flag to check if bullet is in bounce state
        bouncingUp = true;

        //bullet position and playerBullet position
        Vector2 rocketPos = rb.position;
        Vector2 bulletPos = playerBulletTransform.position;

        //direction from playerBullet to rocket (bounce direction)
        Vector2 incomingDir = (rocketPos - bulletPos).normalized;

        //making bounce more upward
        Vector2 bounceDir = (incomingDir + Vector2.up * 1.5f).normalized;

        //bullet angle change
        float angle = Mathf.Atan2(bounceDir.y, bounceDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;

        //bullet velocity change
        rb.linearVelocity = bounceDir * speed;

        reachedTarget = false;
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
