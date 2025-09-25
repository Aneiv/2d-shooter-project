using DG.Tweening;
using Mirror;
using System.Collections;
using UnityEngine;

public class DreadWingCannon : EnemyCannonShoot
{
    [Header("Bullet")]
    public GameObject enemyBullet;
    public float bulletSpeed = 3f;

    [Header("Shooting")]
    //public int numberOfBulletInBurst = 3;
    //public float bulletSpawnDelay = 1f;
    public float rotationSpeed = 200f;

    [SyncVar] protected Vector2 targetPosition;
    [SyncVar] protected Vector2 direction;

    [Header("Laser")]
    public LineRenderer aimingRay;
    [SyncVar(hook = nameof(OnLaserStartChanged))]
    private Vector3 laserStart = Vector3.zero;

    [SyncVar(hook = nameof(OnLaserEndChanged))]
    private Vector3 laserEnd = Vector3.zero;

    [Server]
    public override void Start()
    {
        base.Start();
        reloadTimer = reloadDelay;
        RpcSetStateAimingLaser(false);
    }

    private void OnLaserStartChanged(Vector3 oldValue, Vector3 newValue)
    {
        aimingRay.SetPosition(0, newValue);
    }

    private void OnLaserEndChanged(Vector3 oldValue, Vector3 newValue)
    {
        aimingRay.SetPosition(1, newValue);
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

    [Server]
    IEnumerator SpawnBulletCoroutine(float intialDelay, float bulletSpawnDelay, int numberOfBulletInBurst)
    {        
        yield return new WaitForSeconds(intialDelay);

        mainEnemy.IsVulnerable = true;

        for (int i = 0; i < numberOfBulletInBurst; i++)
        {
            SpawnBullet();
            yield return new WaitForSeconds(bulletSpawnDelay);
        }
        waiting = false;
        mainEnemy.IsVulnerable = false;
    }

    [Server]
    void SpawnBullet()
    {
        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        GameObject bullet = Instantiate(enemyBullet, firePoint.position, firePoint.rotation);

        bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Mirror.NetworkServer.Spawn(bullet);
        RpcSetBulletVelocity(bullet, direction);
    }

    [ClientRpc]
    void RpcSetBulletVelocity(GameObject bullet, Vector2 direction)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }



    [ClientRpc]
    private void RpcDrawBulletTrajectory(Vector3 position, Vector2 direction)
    {
        if (mainEnemy == null) return;
        if (!mainEnemy.IsAlive()) return;

        //GameObject lineObj = new GameObject("BulletTrajectoryLine");
        //lineObj.transform.parent = this.transform;
        //LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        //lr.positionCount = 2;
        //lr.startWidth = 0.01f;
        //lr.endWidth = 0.01f;        
        //lr.material = new Material(Shader.Find("Sprites/Default"));
        //lr.startColor = Color.red;
        //lr.endColor = Color.red;
        //aimingRay.SetPosition(0, firePoint.position);
        //aimingRay.SetPosition(1, firePoint.position + (Vector3)(direction.normalized * lineLength));

        float lineLength = 10f;

        laserStart = firePoint.position;
        laserEnd = firePoint.position + (Vector3)(direction.normalized * lineLength);


        //dotween line fading animation
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => RpcSetStateAimingLaser(true));
        seq.AppendInterval(0.1f); // "on"
        seq.AppendCallback(() => RpcSetStateAimingLaser(false));
        seq.AppendInterval(0.1f); // "off"
        seq.SetLoops(4);
        seq.SetTarget(mainEnemy);
        seq.OnComplete(() => RpcSetStateAimingLaser(false));
        seq.OnKill(() => RpcSetStateAimingLaser(false));

    }

    [Server]
    public void RotateCannonToDestination(Transform destination, float intialDelay, float bulletSpawnDelay, int numberOfBulletInBurst, bool addOffset)
    {
        if (destination == null) return;
        if (mainEnemy == null) return;
        if (!mainEnemy.IsAlive()) return;

        float rotationDuration = 0.5f; 

        // aim
        targetPosition = destination.position;
        //destination
        Vector2 toTarget = targetPosition - rb.position;

        float randomOffset = Random.Range(5f, 15f);

        //rotation angle
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;

        if (addOffset)
        {
            toTarget = (Vector2)(Quaternion.Euler(0f, 0f, randomOffset) * toTarget);
            angle += randomOffset;
        }
        //float rotateAmount = Vector3.Cross(direction, transform.up).z;

        //rb.rotation -= rotateAmount * rotationSpeed * Time.deltaTime;

        rb.DORotate(angle, rotationDuration)
            .SetTarget(mainEnemy)
            .OnComplete(() =>
        {
            RpcDrawBulletTrajectory(firePoint.position, toTarget); //draw bullet trajectory
            StartCoroutine(SpawnBulletCoroutine(intialDelay, bulletSpawnDelay, numberOfBulletInBurst));

        });
    }


}
