using UnityEngine;

public class SturdyBullet : MonoBehaviour, IHealth
{
    public int hp = 50;
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
        //Debug.Log("SturdyBullet took: " + damage.ToString() + " dmg");
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
        Destroy(this.gameObject);
    }
}
