using UnityEngine;
using DG.Tweening;
public class EnemyCannon : Enemy
{
    private SpacecraftCarrierEnemy spacecraftCarrierEnemy;
    public ParticleSystem firePart;
    public ParticleSystem smokePart;
    public float fireSmokePartScale;

    protected override void Start()
    {
        base.Start();
        isVulnerable = true;
        spacecraftCarrierEnemy = FindFirstObjectByType<SpacecraftCarrierEnemy>();
    }

    public override void Die(GameObject attacker)
    {
        //Debug.Log("KILLED ENEMY");
        if (!enemyKilled)
        {
            if (spacecraftCarrierEnemy != null) {
                spacecraftCarrierEnemy.DestroyCannon();
            }

            // fire and smoke particles
            var fireInstance = Instantiate(firePart, transform.position, Quaternion.identity);
            var smokeInstance = Instantiate(smokePart, transform.position, Quaternion.Euler(-90f, 0f, 0f));

            fireInstance.transform.SetParent(spacecraftCarrierEnemy.transform, true);
            smokeInstance.transform.SetParent(spacecraftCarrierEnemy.transform, true);

            var mainFire = fireInstance.main;
            mainFire.startSizeMultiplier = fireSmokePartScale;
            var mainSmoke = smokeInstance.main;
            mainSmoke.startSizeMultiplier = fireSmokePartScale * 2;

            fireInstance.Play();
            smokeInstance.Play();

            // score reward
            Player player = attacker.GetComponent<Player>();
            if (player != null)
            {
                player.AddToScore(scoreReward);
            }
            GameObject srObj = Instantiate(scoreRewardPrefab, transform.position, Quaternion.identity);
            ScoreRewardAnim srAnim = srObj.GetComponent<ScoreRewardAnim>();
            if (srAnim != null)
            {
                srAnim.SetScore(scoreReward);
            }

            ExplosionParticles();

            enemyKilled = true;

            DOTween.Kill(mainSprite);
            DOTween.Kill(gameObject);
            Destroy(rootEnemy);
        }
    }
    protected override void OnCollisionWithPlayer(GameObject playerObj) {}
}
