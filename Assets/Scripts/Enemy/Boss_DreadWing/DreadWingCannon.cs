using DG.Tweening;
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

    protected Vector2 targetPosition;
    protected Vector2 direction;

    public override void Start()
    {
        base.Start();
        reloadTimer = reloadDelay;
    }

    private void FixedUpdate()
    {
        /*        if (!waiting)
                {
                    reloadTimer -= Time.deltaTime;

                    // aim
                    targetPosition = targetPlayer.position;
                    //destination
                    Vector2 toTarget = targetPosition - rb.position;

                    direction = toTarget.normalized;
                    //rotation
                    float rotateAmount = Vector3.Cross(direction, transform.up).z;

                    rb.rotation -= rotateAmount * rotationSpeed * Time.deltaTime;

                    // shoot
                    if (reloadTimer <= 0f)
                    {
                        waiting = true;
                        reloadTimer = Random.Range(minReloadDelay, max1Delay);

                        StartCoroutine(SpawnBulletCoroutine());
                    }
                }*/
    }

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

    void SpawnBullet()
    {
        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        GameObject Bullet = Instantiate(enemyBullet, firePoint.position, firePoint.rotation);

        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rbBullet = Bullet.GetComponent<Rigidbody2D>();
        rbBullet.linearVelocity = direction.normalized * bulletSpeed;
    }
    private void DrawBulletTrajectory(Vector3 position, Vector2 direction)
    {
        if (mainEnemy == null) return;
        if (!mainEnemy.IsAlive()) return;

        GameObject lineObj = new GameObject("BulletTrajectoryLine");
        lineObj.transform.parent = bulletsContainer.transform;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;        
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.red;

        float lineLength = 10f;
        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, firePoint.position + (Vector3)(direction.normalized * lineLength));

        //dotween line fading animation
        Sequence seq = DOTween.Sequence();
        seq.Append(lr.material.DOFade(1f, 0.1f)); 
        seq.Append(lr.material.DOFade(0f, 0.1f));
        seq.SetLoops(4, LoopType.Yoyo);
        seq
            .SetTarget(mainEnemy)
            .OnComplete(() => Destroy(lineObj))
            .OnKill(() => Destroy(lineObj));
    }

    public void RotateCannonToDestination(Transform destination, float intialDelay, float bulletSpawnDelay, int numberOfBulletInBurst, bool addOffset)
    {
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
            DrawBulletTrajectory(firePoint.position, toTarget); //draw bullet trajectory
            StartCoroutine(SpawnBulletCoroutine(intialDelay, bulletSpawnDelay, numberOfBulletInBurst));

        });
    }


}
