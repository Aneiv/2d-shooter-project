using System.Collections;
using UnityEngine;

public class SniperCannon : EnemyCannonShoot
{
    [Header("Laser")]
    public LineRenderer aimingRay;
    public LineRenderer hurtfulRay;
    public ParticleSystem hurtfulRayParticle;
    private ParticleSystem currentHurtfulRayParticle;
    public int damage = 15;

    [Header("Shooting")]
    private RaycastHit2D[] hitsInfoAim;
    private Coroutine aimAndShootCoroutine;
    public float rotationSpeed = 200f;
    public float hurtfulLaserBeamDuration = 0.8f;

    [Header("Aiming")]
    public float onPlayerTrackingTime = 1f; // time of aiming the cannon at the player
    public float lockAimTime = 2f;
    public float aimingLaserBlinkDuration = 0.2f;
    public int numberOfBlinks = 3;
    private float aimingTimer = 0f;
    private Vector2 targetPosition;

    public override void Start()
    {
        base.Start();
        aimingRay.enabled = true;
        hurtfulRay.enabled = false;
    }

    private void FixedUpdate()
    {
        if (!waiting && aimAndShootCoroutine == null)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                reloadTimer = reloadDelay;

                aimingRay.enabled = true;
                hurtfulRay.enabled = false;

                aimAndShootCoroutine = StartCoroutine(AimAndShootAtPlayerCoroutine());
            }
        }
    }

    private IEnumerator AimAndShootAtPlayerCoroutine()
    {
        aimingTimer = lockAimTime;

        while (aimingTimer > 0f)
        {
            targetPosition = targetPlayer.position;
            //destination
            Vector2 toTarget = targetPosition - rb.position;

            Vector2 direction = toTarget.normalized;
            //rotation
            float rotateAmount = Vector3.Cross(direction, transform.up).z;

            rb.rotation -= rotateAmount * rotationSpeed * Time.deltaTime;

            //aiming Raycast
            hitsInfoAim = Physics2D.RaycastAll(firePoint.position, firePoint.up, 100f);
            aimingRay.SetPosition(0, firePoint.position);
            Vector2 hitPoint = firePoint.position + firePoint.up * 100;

            bool playerSeen = false;

            aimingRay.SetPosition(1, hitPoint);

            foreach (RaycastHit2D hit in hitsInfoAim)
            {
                if (hit.transform.CompareTag("Player"))
                {
                    playerSeen = true;
                    hitPoint = hit.point;
                    aimingTimer -= Time.deltaTime;
                    break;
                }

            }

            if (!playerSeen)
            {
                aimingTimer = lockAimTime;
            }

            aimingRay.SetPosition(1, hitPoint);


            yield return null;
        }

        // laser blink
        aimingRay.SetPosition(1, firePoint.position + firePoint.up * 100);

        for (int i = 0; i < numberOfBlinks * 2; i++)
        {
            aimingRay.enabled = !aimingRay.enabled;
            yield return new WaitForSeconds(aimingLaserBlinkDuration);
        }
        aimingRay.enabled = false;

        // shoot at player
        hurtfulRay.enabled = true;

        // particles
        Quaternion particleDirection = Quaternion.LookRotation(firePoint.up);
        currentHurtfulRayParticle = Instantiate(hurtfulRayParticle, firePoint.position, particleDirection);
        currentHurtfulRayParticle.Play();

        RaycastHit2D[] hitsInfoShoot = Physics2D.RaycastAll(firePoint.position, firePoint.up, 100f);
        hurtfulRay.SetPosition(0, firePoint.position);
        Vector2 shootHitPoint = firePoint.position + firePoint.up * 100f;

        hurtfulRay.SetPosition(1, shootHitPoint);



        foreach (RaycastHit2D hit in hitsInfoShoot)
        {
            if (hit.transform.CompareTag("Player"))
            {
                shootHitPoint = hit.point;
                hurtfulRay.SetPosition(1, shootHitPoint);

                Player player = hit.transform.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                break;
            }
        }

        yield return new WaitForSeconds(hurtfulLaserBeamDuration);


        hurtfulRay.enabled = false;
        reloadTimer = reloadDelay;

        Destroy(currentHurtfulRayParticle.gameObject);
        aimAndShootCoroutine = null;
    }
}

