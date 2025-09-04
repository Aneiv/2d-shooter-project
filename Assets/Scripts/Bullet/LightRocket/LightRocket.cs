using UnityEngine;

public class LightRocket : SturdyBullet
{
    public override void Die()
    {
        var RocketExplosionParticles = GetComponent<LightRocketBulletMovement>();
        RocketExplosionParticles.ExplodeParticles();
        Destroy(this.gameObject);
    }
}
