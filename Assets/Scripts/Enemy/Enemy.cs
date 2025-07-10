using UnityEngine;

public class Enemy : MonoBehaviour, IShooter, IHealth
{
    public int maxHp = 50;
    private int currentHp;
    public int bulletDamage = 10;
    public int BulletDamage => bulletDamage;
    public GameObject waveManager;
    public EnemyHealthBar healthBar;
    public GameObject rootEnemy;
    
    void Start()
    {
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int damage)
    {
        //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
        if (currentHp - damage > 0)
        {
            currentHp -= damage;
            healthBar.SetHealth(currentHp);
        }
        else
        {
            Die();
        }
    }
    public void Die()
    {
        //Debug.Log("KILLED ENEMY");
        var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
        destroyTrigger.EnemyKilled();
        Destroy(rootEnemy);
    }
}
