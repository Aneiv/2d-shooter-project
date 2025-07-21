using System.Collections;
using UnityEngine;

public class SniperCannon : MonoBehaviour
{
    public Transform firePoint;
    private Transform targetPlayer;
    private Rigidbody2D rb;

    public LineRenderer aimingRay;

    private Vector2 targetPosition;

    public float reloadDelay = 6f;
    public float rotationSpeed = 200f;

    public float onPlayerTrackingTime = 1f; // time of aiming the cannon at the player
    public float lockAimTime = 2f;
    public float hurtfulLaserBeamDuration = 0.8f;

    public float aimingLaserBlinkDuration = 0.2f;
    public int numberOfBlinks = 2;

    private bool isRealoading = false;
    private bool isRotating = true;
    private float reloadTimer = 0f;
    private float aimingTimer = 0f;

    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }
    }

    public void ReadyToShoot()
    {
        isRealoading = false;
        isRotating = true;
    }

    private void FixedUpdate()
    {
        if (!isRealoading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                reloadTimer = reloadDelay;

                aimingRay.enabled = true;
                StartCoroutine(AimAtPlayer());
                
            }
        }
    }

    private IEnumerator AimAtPlayer()
    {
        isRealoading = true;

        aimingTimer = lockAimTime;

        while(aimingTimer > 0f)
        {
            targetPosition = targetPlayer.position;
            //destination
            Vector2 toTarget = targetPosition - rb.position;

            Vector2 direction = toTarget.normalized;
            //rotation
            float rotateAmount = Vector3.Cross(direction, transform.up).z;

            rb.rotation -= rotateAmount * rotationSpeed * Time.deltaTime;

            //aiming Raycast
            RaycastHit2D hitInfo = Physics2D.Raycast(firePoint.position, firePoint.up);
            aimingRay.SetPosition(0, firePoint.position);
            aimingRay.SetPosition(1, firePoint.position + firePoint.up * 100);

            if (!hitInfo)
            {
                aimingTimer = lockAimTime;
            }
            else
            {
                if (hitInfo.transform.CompareTag("Player"))
                {
                    aimingTimer -= Time.deltaTime;
                    aimingRay.SetPosition(1, hitInfo.point);
                }
            }

            yield return null;
        }

        aimingRay.enabled = false;
        isRealoading = false;
    }
}
