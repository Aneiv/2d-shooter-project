using UnityEngine;
using DG.Tweening;
public class EnemyCannon : Enemy
{
    private SpacecraftCarrierEnemy spacecraftCarrierEnemy;

    protected override void Start()
    {
        base.Start();
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
                srAnim.SetText("+" + scoreReward.ToString());
            }

            ExplosionParticles();

            enemyKilled = true;

            DOTween.Kill(mainSprite);
            DOTween.Kill(gameObject);
            Destroy(rootEnemy);
        }
    }
}
