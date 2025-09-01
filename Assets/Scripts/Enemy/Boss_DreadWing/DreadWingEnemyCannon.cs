using DG.Tweening;
using UnityEngine;

public class DreadWingEnemyCannon : Enemy
{
    private DreadWingEnemy dreadWingEnemy;

    public EnemyHealthBar enemyHealthBar;
    private DreadWingEnemyHealthBar enemyHealthBarScript;
    protected override void Start()
    {
        base.Start();

        isVulnerable = false;
        dreadWingEnemy = FindFirstObjectByType<DreadWingEnemy>();
        enemyHealthBarScript = enemyHealthBar.GetComponent<DreadWingEnemyHealthBar>();
    }

    public override void Die(GameObject attacker)
    {
        //Debug.Log("KILLED ENEMY");
        if (!enemyKilled)
        {
            if (dreadWingEnemy != null)
            {
                dreadWingEnemy.DestroyCannon();
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
                srAnim.SetScore(scoreReward);
            }

            ExplosionParticles();

            enemyHealthBarScript.SingleCannonDestroyed(dreadWingEnemy.singleCannonScoreValue);
            enemyKilled = true;

            DOTween.Kill(mainSprite);
            DOTween.Kill(gameObject);
            Destroy(rootEnemy);
        }
    }
    protected override void OnCollisionWithPlayer(GameObject playerObj) { }
}
