using Unity.VisualScripting;
using System.Collections;
using UnityEngine;
using Mirror;

public class PlayerShoot : Mirror.NetworkBehaviour
{
    public GameObject PlayerBulletBig;
    public GameObject PlayerBulletSmall;

    public float bulletSpawnDelay = 0.5f;//delay in seconds

    public float bulletSpeed = 1f;

    private bool isWaitingForShot = false;
    private float timer = 0f;
    public float shootingCooldown = 2f;

    private Transform thisPlayerTransform;
    public Transform firePoint_LBig;
    public Transform firePoint_RBig;
    private GameObject bulletsContainer;
    public Transform firePoint_LSmall;
    public Transform firePoint_RSmall;

    private bool shootLeft = true;

    public int smallBulletCooldown = 1;
    private int bulletCounter = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bulletsContainer = GameObject.Find("BulletsContainer");
        thisPlayerTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isWaitingForShot)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartCoroutine(SpawnBulletCoroutine());
                timer = shootingCooldown; //timer reset
            }
        }
    }


    IEnumerator SpawnBulletCoroutine()
    {
        isWaitingForShot = true;

        yield return new WaitForSeconds(bulletSpawnDelay); //delay
        SpawnBullet();

        isWaitingForShot = false;
    }

    void SpawnBullet()
    {
        float angleInDegrees = thisPlayerTransform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Transform firePointBig = shootLeft ? firePoint_LBig: firePoint_RBig;
        shootLeft = !shootLeft;

        GameObject bulletBig = Instantiate(PlayerBulletBig, firePointBig.position, firePointBig.rotation);
        bulletBig.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        Mirror.NetworkServer.Spawn(bulletBig);
        RpcSetBulletVelocity(bulletBig, direction);
        bulletCounter++;
        
        // set owner of bullet
        bulletBig.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rb_Big = bulletBig.GetComponent<Rigidbody2D>();
        rb_Big.linearVelocity = direction.normalized * bulletSpeed;

        if(bulletCounter== smallBulletCooldown)
        {
            bulletCounter = 0;
            GameObject BulletSmall_Left = Instantiate(PlayerBulletSmall, firePoint_LSmall.position, firePoint_LSmall.rotation);
            BulletSmall_Left.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
            BulletSmall_Left.GetComponent<BulletCollisionDetection>().Init(this.gameObject);
            Rigidbody2D rb_Small_Left = BulletSmall_Left.GetComponent<Rigidbody2D>();
            rb_Small_Left.linearVelocity = direction.normalized * bulletSpeed;
            Mirror.NetworkServer.Spawn(BulletSmall_Left);            
            RpcSetBulletVelocity(BulletSmall_Left, direction);

            GameObject BulletSmall_Right = Instantiate(PlayerBulletSmall, firePoint_RSmall.position, firePoint_RSmall.rotation);
            BulletSmall_Right.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
            BulletSmall_Right.GetComponent<BulletCollisionDetection>().Init(this.gameObject);
            Rigidbody2D rb_Small_Right = BulletSmall_Right.GetComponent<Rigidbody2D>();
            rb_Small_Right.linearVelocity = direction.normalized * bulletSpeed;
            Mirror.NetworkServer.Spawn(BulletSmall_Right);
            RpcSetBulletVelocity(BulletSmall_Right, direction);
        }
    }
    [ClientRpc]
    void RpcSetBulletVelocity(GameObject bullet, Vector2 direction)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}
