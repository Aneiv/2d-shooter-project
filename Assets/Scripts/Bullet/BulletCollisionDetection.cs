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

    // runs before Start()
    public void Init(GameObject shooter)
    {
        // get shooter (owner) damage
        this.shooter = shooter;
        shooterTag = shooter.tag;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && shooterTag != "Player") // prevents self-shot
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                //Debug.Log("LOG Bullet hit player");
                Destroy(gameObject);
                player.TakeDamage(damage);
                
            }
        }
        else if (collision.CompareTag("Enemy") && shooterTag != "Enemy")
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                Quaternion particleDirection = Quaternion.LookRotation(-transform.right);
                Vector3 offset = transform.right * 0.2f;
                currentSparksParticles = Instantiate(SparksParticles, transform.position + offset, particleDirection);
                currentSparksParticles.Play();

                Destroy(currentSparksParticles.gameObject, 0.5f);
                //Debug.Log("LOG Bullet hit Basic_Enemy");
                Destroy(gameObject);
                enemy.TakeDamage(damage, shooter);
            }
        }
    }

}
