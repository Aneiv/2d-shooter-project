using DG.Tweening;
using UnityEngine;

public class SturdyBullet : MonoBehaviour, IHealth
{
    public int hp = 50;    
    public float flashAnimationDuration = 0.1f;

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
    public void InVulnerableHitAnim(SpriteRenderer sprite)
    {
        Material mat = sprite.material;
        mat.DOColor(new Color(0f, 1.5f, 3f, 0.3f), flashAnimationDuration) // go to blue color
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject)
                    .OnComplete(() =>
                     {
                         mat.DOColor(Color.white, flashAnimationDuration) // go to default color
                             .SetEase(Ease.InOutSine)
                             .SetLink(gameObject);
                     });
    }
}
