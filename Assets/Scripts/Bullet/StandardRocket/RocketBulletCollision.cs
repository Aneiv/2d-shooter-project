using Mirror;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RocketBulletCollision : MonoBehaviour
{
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private Vector2 pos;
    private float leftXClamp, rightXClamp, downYClamp, upYClamp;
    private float clampSize = 0.5f;
    public float invincibilityTime = 2f; // invincible time
    private bool isVulnerable = false;
    public SpriteRenderer mainSprite;
    [HideInInspector] public GameObject shooter;
    public int damage;

    // runs before Start()
    public void Init(GameObject shooter)
    {
        // get shooter (owner) damage
        this.shooter = shooter;
    }
    void Start()
    {
        isVulnerable = false;
        //border clamp
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        //clamp borders
        leftXClamp = bottomLeft.x - clampSize;
        rightXClamp = topRight.x + clampSize;
        downYClamp = bottomLeft.y - clampSize;
        upYClamp = topRight.y + clampSize;

        StartCoroutine(Invincibility());//rocket temporary initial invincibility
    }

    void Update()
    {
        pos = transform.position;
        if (pos.x < leftXClamp || pos.x > rightXClamp || pos.y < downYClamp || pos.y > upYClamp)
        {
            //Debug.Log("LOG Bullet hit screen bounds");
            Destroy(gameObject);
        }
    }
    IEnumerator Invincibility()
    {
        isVulnerable = false;
        yield return new WaitForSeconds(invincibilityTime);
        isVulnerable = true;
    }
    [Server]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var sturdyBullet = GetComponent<SturdyBullet>();
        NetworkIdentity attackerNetId = this.GetComponent<NetworkIdentity>();
        if (isVulnerable)
        {
            if (collision.CompareTag("Player"))
            {

                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    //Debug.Log("LOG Bullet hit player");
                    //Destroy(gameObject);
                    //var rocketBullet = GetComponent<SturdyBullet>();
                    sturdyBullet.Die();
                    player.TakeDamage(damage);

                }
            }
            else if (collision.CompareTag("Enemy"))
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, attackerNetId);
                    //Destroy(gameObject);
                    sturdyBullet.Die();
                }
            }
            else if (collision.CompareTag("PlayerBullet"))
            {
                GameObject playerBullet = collision.gameObject;
                BulletCollisionDetection playerBulletCollision = playerBullet.GetComponent<BulletCollisionDetection>();
                //Debug.Log($"LOG Player Bullet hit RocketBullet with damage: {shooterScript.BulletDamage}");
                Destroy(collision.gameObject);
                sturdyBullet.TakeDamage(playerBulletCollision.damage);
                var bulletBounce = GetComponent<RocketBulletMovement>();
                bulletBounce.Bounce(playerBullet.transform);

            }

        }
        else
        {
            if (collision.CompareTag("PlayerBullet"))
            {
                Destroy(collision.gameObject);
                sturdyBullet.InVulnerableHitAnim(mainSprite);
            }
        }
    }
}
