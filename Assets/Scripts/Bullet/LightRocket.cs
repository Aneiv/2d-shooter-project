using UnityEngine;

public class LightRocket : MonoBehaviour, IHealth
{
    public int hp = 1;
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
        var RocketExplosionParticles = GetComponent<LightRocketBulletMovement>();
        RocketExplosionParticles.ExplodeParticles();
        Destroy(this.gameObject);
    }
}
