using DG.Tweening;
using UnityEngine;

public class DreadWingEnemyCannon : Enemy
{
    private DreadWingEnemy dreadWingEnemy;

    //public EnemyHealthBar enemyHealthBar;
    //private DreadWingEnemyHealthBar enemyHealthBarScript;

    public ParticleSystem firePart;
    public ParticleSystem smokePart;
    public float fireSmokePartScale;
    protected override void Start()
    {
        base.Start();

        isVulnerable = false;
        dreadWingEnemy = FindFirstObjectByType<DreadWingEnemy>();
        //enemyHealthBarScript = enemyHealthBar.GetComponent<DreadWingEnemyHealthBar>();
    }

    public override void Die(GameObject attacker)
    {
        //Debug.Log("KILLED ENEMY");
        if (!enemyKilled)
        {
            if (dreadWingEnemy != null)
            {
                dreadWingEnemy.DestroyCannon(maxHp);
            }

            // fire and smoke particles
            var fireInstance = Instantiate(firePart, transform.position, Quaternion.identity);
            var smokeInstance = Instantiate(smokePart, transform.position, Quaternion.Euler(-90f, 0f, 0f));

            fireInstance.transform.SetParent(dreadWingEnemy.transform, true);
            smokeInstance.transform.SetParent(dreadWingEnemy.transform, true);

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

            //enemyHealthBarScript.SingleCannonDestroyed(dreadWingEnemy.singleCannonScoreValue);
            enemyKilled = true;

            DOTween.Kill(mainSprite);
            DOTween.Kill(gameObject);
            Destroy(rootEnemy);
        }
    }
    protected override void OnCollisionWithPlayer(GameObject playerObj) { }
}
