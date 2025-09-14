using Mirror;
using UnityEngine;

public class BulletCollisionDetection : MonoBehaviour
{
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private Vector2 pos;
    private float leftXClamp, rightXClamp, downYClamp, upYClamp;
    private float clampSize = 0.5f;

    public GameObject shooter;
    private string shooterTag;
    public int damage;

    public ParticleSystem SparksParticles;
    private ParticleSystem currentSparksParticles;

    private NetworkIdentity attackerNetId;
    private GameObject attackerObj;
    // runs before Start()
    public void Init(GameObject shooter)
    {
        // get shooter (owner) damage
        this.shooter = shooter;
        shooterTag = shooter.tag;

        attackerNetId = this.GetComponent<NetworkIdentity>();
        attackerObj = attackerNetId != null ? attackerNetId.gameObject : null;
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
    void FixedUpdate()
    {
        pos = transform.position;
        if (pos.x < leftXClamp || pos.x > rightXClamp || pos.y < downYClamp || pos.y > upYClamp)
        {
            //Debug.Log("LOG Bullet hit screen bounds");
            Destroy(gameObject);
        }
    }
    [Server]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //enemy bullet collided with player
        if (collision.CompareTag("Player") && shooterTag != "Player") // prevents self-shot
        {
            Player player;
            if (collision.gameObject.TryGetComponent<Player>(out player))
            {
                //Debug.Log("LOG Bullet hit player");
                Destroy(gameObject);
                player.TakeDamage(damage);

            }
        }
        //prevent self-shoot by both players
        else if (collision.CompareTag("Player") && shooterTag == "PlayerBullet")
        {
            return;
        }
        //prevent self-shoot by enemies
        else if ((collision.CompareTag("Enemy") || (collision.CompareTag("EnemyCannon"))) &&
            (shooterTag == "Enemy" || shooterTag == "EnemyCannon"))
        {
            return;
        }
        //Player bullet hit enemy or enemyCannon
        else if ((collision.CompareTag("Enemy") || (collision.CompareTag("EnemyCannon"))) &&
            (shooterTag != "Enemy" && shooterTag != "EnemyCannon"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (enemy.IsVulnerable)
                {
                    Quaternion particleDirection = Quaternion.LookRotation(-transform.right);
                    Vector3 offset = transform.right * 0.2f;
                    currentSparksParticles = Instantiate(SparksParticles, transform.position + offset, particleDirection);
                    currentSparksParticles.Play();

                    Destroy(currentSparksParticles.gameObject, 0.5f);
                    //Debug.Log("LOG Bullet hit Basic_Enemy");
                    Destroy(gameObject);
                    enemy.TakeDamage(damage, attackerNetId);
                }
                else
                {
                    // triger InVulnerableHitAnim (blue flash)
                    enemy.TakeDamage(0, attackerNetId);
                }
            }
        }
    }

}
