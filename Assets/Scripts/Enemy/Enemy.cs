using UnityEngine;

public class Enemy : MonoBehaviour, IShooter, IHealth
{
    public int hp = 50;
    public int bulletDamage = 10;
    public int BulletDamage => bulletDamage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Enemy took: " + damage.ToString() + " dmg");
        if (hp - damage > 0)
        {
            hp -= damage;
        }
        else
        {
            Die();
        }
    }
    public void Die()
    {
        Destroy(gameObject);
    }
}
