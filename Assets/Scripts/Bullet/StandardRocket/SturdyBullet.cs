using UnityEngine;

public class SturdyBullet : MonoBehaviour, IHealth
{
    public int hp = 50;

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
    public virtual void Die()
    {
        var RocketExplosionParticles = GetComponent<RocketBulletMovement>();
        RocketExplosionParticles.ExplodeParticles();
        Destroy(this.gameObject);
    }
}
