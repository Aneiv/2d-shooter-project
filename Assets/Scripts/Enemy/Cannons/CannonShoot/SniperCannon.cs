using Mirror;
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
    [SyncVar] private float aimingTimer = 0f;
    [SyncVar] private Vector2 targetPosition;

    private Vector3 laserStart = Vector3.zero;
    private Vector3 laserEnd = Vector3.zero;

    [Server]
    public override void Start()
    {
        base.Start();
        //aimingRay.enabled = true;
        //hurtfulRay.enabled = false;
        RpcSetStateAimingLaser(false);
        RpcSetStateShootLaser(false);
    }

    [Server]
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!waiting && aimAndShootCoroutine == null)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                reloadTimer = reloadDelay;

                //aimingRay.enabled = true;
                //hurtfulRay.enabled = false;
                RpcSetStateAimingLaser(true);
                RpcSetStateShootLaser(false);

                GetRandomPlayerTarget();
                
                aimAndShootCoroutine = StartCoroutine(AimAndShootAtPlayerCoroutine());
            }
        }

        UpdateLaser(firePoint.position, firePoint.position + firePoint.up * 100f);
    }

    private float laserSyncCooldown = 0.025f;
    private float laserSyncTimer = 0f;

    [Server]
    private void UpdateLaser(Vector3 start, Vector3 end)
    {
        laserSyncTimer -= Time.deltaTime;
        if (laserSyncTimer <= 0f)
        {
            laserStart = start;
            laserEnd = end;
            RpcUpdateAimingLaser(start, end);
            laserSyncTimer = laserSyncCooldown;
        }
    }

    [ClientRpc]
    void RpcUpdateAimingLaser(Vector3 start, Vector3 end)
    {
        aimingRay.SetPosition(0, start);
        aimingRay.SetPosition(1, end);
    }

    [ClientRpc]
    void RpcSetStateAimingLaser(bool state)
    {
        if (aimingRay != null)
        {
            aimingRay.enabled = state;

            if (state)
            {
                aimingRay.positionCount = 2;
                aimingRay.SetPosition(0, laserStart);
                aimingRay.SetPosition(1, laserEnd);
            }
        }
    }

    [ClientRpc]
    void RpcSetStateShootLaser(bool state)
    {
        if(hurtfulRay != null)
        {
            hurtfulRay.enabled = state;
        }
    }

    [ClientRpc]
    void RpcShootLaser(Vector3 start, Vector3 end)
    {
        if (hurtfulRay != null)
        {
            hurtfulRay.enabled = true;
            hurtfulRay.SetPosition(0, start);
            hurtfulRay.SetPosition(1, end);
        }

        if (hurtfulRayParticle != null)
        {
            Quaternion particleDirection = Quaternion.LookRotation(firePoint.up);
            currentHurtfulRayParticle = Instantiate(hurtfulRayParticle, start, particleDirection);
            currentHurtfulRayParticle.Play();
        }
    }

    [ClientRpc]
    void RpcTurnOffShootLaser()
    {
        RpcSetStateShootLaser(false);
        if(currentHurtfulRayParticle != null)
        {
            Destroy(currentHurtfulRayParticle.gameObject);
            currentHurtfulRayParticle = null;
        }
    }


    [Server]
    private IEnumerator AimAndShootAtPlayerCoroutine()
    {
        aimingTimer = lockAimTime;
        if (targetPlayer == null) yield break;
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
            Vector2 hitPoint = firePoint.position + firePoint.up * 100;
            hitsInfoAim = Physics2D.RaycastAll(firePoint.position, firePoint.up, 100f);

            bool playerSeen = false;

            //aimingRay.SetPosition(0, firePoint.position);
            //aimingRay.SetPosition(1, hitPoint);
            laserStart = firePoint.position;
            laserEnd = hitPoint;

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

            laserEnd = hitPoint;

            yield return null;
        }

        // laser blink
        //aimingRay.SetPosition(1, firePoint.position + firePoint.up * 100);

        for (int i = 0; i < numberOfBlinks * 2; i++)
        {
            RpcSetStateAimingLaser(!aimingRay.enabled);
            yield return new WaitForSeconds(aimingLaserBlinkDuration);
        }
        RpcSetStateAimingLaser(false);

        // shoot at player
        hurtfulRay.enabled = true;

        // particles

        //Quaternion particleDirection = Quaternion.LookRotation(firePoint.up);
        //currentHurtfulRayParticle = Instantiate(hurtfulRayParticle, firePoint.position, particleDirection);
        //currentHurtfulRayParticle.Play();

        RaycastHit2D[] hitsInfoShoot = Physics2D.RaycastAll(firePoint.position, firePoint.up, 100f);
        Vector2 shootHitPoint = firePoint.position + firePoint.up * 100f;

        //hurtfulRay.SetPosition(0, firePoint.position);
        //hurtfulRay.SetPosition(1, shootHitPoint);

        foreach (RaycastHit2D hit in hitsInfoShoot)
        {
            if (hit.transform.CompareTag("Player"))
            {
                shootHitPoint = hit.point;
                //hurtfulRay.SetPosition(1, shootHitPoint);
                

                Player player = hit.transform.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
                break;
            }
        }
        RpcShootLaser(firePoint.position, shootHitPoint);

        yield return new WaitForSeconds(hurtfulLaserBeamDuration);


        //hurtfulRay.enabled = false;
        //Destroy(currentHurtfulRayParticle.gameObject);
        RpcTurnOffShootLaser();

        reloadTimer = reloadDelay;
        aimAndShootCoroutine = null;
    }
}

