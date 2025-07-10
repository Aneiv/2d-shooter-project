using UnityEngine;

public class BulletCollisionDetection : MonoBehaviour
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
        if (collision.CompareTag("Player") && !shooter.CompareTag("Player")) // prevents self-shot
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                //Debug.Log("LOG Bullet hit player");
                Destroy(gameObject);
                player.TakeDamage(damage);
                
            }
        }
        else if (collision.CompareTag("Enemy") && !shooter.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                //Debug.Log("LOG Bullet hit Basic_Enemy");
                Destroy(gameObject);
                enemy.TakeDamage(damage);
            }
        }
    }

}
