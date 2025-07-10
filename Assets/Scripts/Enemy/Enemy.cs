using UnityEngine;

public class Enemy : MonoBehaviour, IShooter, IHealth
{
    public int maxHp = 50;
    private int currentHp;
    public int bulletDamage = 10;
    public int BulletDamage => bulletDamage;

    public EnemyHealthBar healthBar;
    public GameObject rootEnemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        Destroy(rootEnemy);
    }
}
