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
    public float explodeDistance = 0.2f; //how near to explode

    private Vector2 direction;
    private bool bouncingUp = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (target != null)
        {
            targetPosition = target.position;  //saving position
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (reachedTarget) return;
        if (target == null) return;

        //destination
        Vector2 toTarget = targetPosition - rb.position;
        float distance = toTarget.magnitude;

        if (bouncingUp)
        {
            bouncingUp = false;
            targetPosition = target.position;
            return;
        }
        //destroy rocket when near destination
        if (distance <= explodeDistance)
        {
            Explode();
            return;
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

        //position
        Gizmos.DrawWireSphere(transform.position, explodeDistance);
    }
}
