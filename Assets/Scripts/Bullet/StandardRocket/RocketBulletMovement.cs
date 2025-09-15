using Mirror;
using TMPro;
using UnityEngine;

public class RocketBulletMovement : Mirror.NetworkBehaviour
{
    [HideInInspector] [SyncVar] public Transform target;
    public float speed = 5f;
    public float rotateSpeed = 200f;

    protected Rigidbody2D rb;
    [SyncVar] protected Vector2 targetPosition;      //player position
    [SyncVar] protected bool reachedTarget = false;
    public ParticleSystem burstParticle; //burst particle system
    public float explodeRadius; //objects in that area get damage from explosion
    //public float playerDamageZone; //radius of circle that designate bullet explosion

    [SyncVar] protected Vector2 direction;
    [SyncVar] private bool bouncingUp = false;

    [SyncVar] float screenCenterYPos;
    protected virtual void Start()
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
    protected virtual void FixedUpdate()
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

    [Server]
    protected virtual void Explode()
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
        //Destroy(gameObject); //destroy rocket
        RpcBulletDestroyed();
    }
    [ClientRpc]
    void RpcBulletDestroyed()
    {
        ExplodeParticles();//particle explosion
        Destroy(gameObject); //destroy rocket
    }
    public void ExplodeParticles()
    {
        ParticleSystem ep = Instantiate(burstParticle, transform.position, Quaternion.identity);
        ep.Play();
        Destroy(ep.gameObject, ep.main.duration + ep.main.startLifetime.constantMax);
    }
    [Server]
    public void Bounce(Transform playerBulletTransform)
    {
        if (reachedTarget) return;
        if (target == null) return;
        if (rb == null) return;
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
        RpcSyncBounce(rb.position, rb.rotation, rb.linearVelocity);
    }

    [ClientRpc]
    void RpcSyncBounce(Vector2 position, float rotation, Vector2 velocity)
    {
        if (isServer) return;

        rb.position = position;
        rb.rotation = rotation;
        rb.linearVelocity = velocity;
    }

    //show radius of exposion when selected on edit mode
    protected void OnDrawGizmosSelected()
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
