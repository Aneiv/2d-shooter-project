using UnityEngine;

public class RocketBulletCollision : MonoBehaviour
{
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private Vector2 pos;
    private float leftXClamp, rightXClamp, downYClamp, upYClamp;
    private float clampSize = 0.5f;

    public GameObject shooter;
    private int damage;

    // runs before Start()
    public void Init(GameObject shooter)
    {
        // get shooter (owner) damage
        this.shooter = shooter;

        IShooter shooterScript = shooter.GetComponent<IShooter>();
        if (shooterScript != null)
        {
            damage = shooterScript.BulletDamage;
        }
    }
    void Start()
    {
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        //clamp borders
        leftXClamp = bottomLeft.x - clampSize;
        rightXClamp = topRight.x + clampSize;
        downYClamp = bottomLeft.y - clampSize;
        upYClamp = topRight.y + clampSize;
    }

    void Update()
    {
        pos = transform.position;
        if (pos.x < leftXClamp || pos.x > rightXClamp || pos.y < downYClamp || pos.y > upYClamp)
        {
            Debug.Log("LOG Bullet hit screen bounds");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log("LOG Bullet hit player");
                Destroy(gameObject);
                player.TakeDamage(damage);

            }
        }
        else if (collision.CompareTag("PlayerBullet"))
        {
            SturdyBullet bullet = GetComponent<SturdyBullet>();
            GameObject playerBullet = collision.gameObject;
            BulletCollisionDetection  playerBulletCollision = playerBullet.GetComponent<BulletCollisionDetection>();
            IShooter shooterScript = playerBulletCollision.shooter.GetComponent<IShooter>();
            if (bullet != null)
            {
                Debug.Log($"LOG Player Bullet hit RocketBullet with damage: {shooterScript.BulletDamage}");
                Destroy(collision.gameObject);
                bullet.TakeDamage(shooterScript.BulletDamage);
                var bulletBounce = bullet.GetComponent<RocketBulletMovement>();
                bulletBounce.Bounce(playerBullet.transform);
            }
        }
    }

}
