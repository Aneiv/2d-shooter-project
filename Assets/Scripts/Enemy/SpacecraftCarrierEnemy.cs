using UnityEngine;
using DG.Tweening;

public class SpacecraftCarrierEnemy : Enemy
{
    public int cannonCounter = 6;
    public GameObject[] cannonsObjs;


    protected override void Start()
    {
        base.Start();
        foreach (GameObject obj in cannonsObjs) {
            if(obj.TryGetComponent<IShootReady>(out var cannon))
            {
                cannon.ReadyToShoot();
            }
        }
        isVulnerable = false;
    }
    public void DestroyCannon()
    {
        cannonCounter--;
        if (cannonCounter <= 0) {
            isVulnerable = true;
        }
    }

    override public void Die(GameObject attacker)
    {
        //Debug.Log("KILLED ENEMY");
        if (!enemyKilled)
        {
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

