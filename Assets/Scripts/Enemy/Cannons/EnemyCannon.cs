using UnityEngine;
using DG.Tweening;
using Mirror;
using System.Collections;
public class EnemyCannon : Enemy
{
    private SpacecraftCarrierEnemy spacecraftCarrierEnemy;
    public ParticleSystem firePart;
    public ParticleSystem smokePart;
    public GameObject fireParticleContainer;
    public float fireSmokePartScale;

    protected override void Start()
    {
        base.Start();
        spacecraftCarrierEnemy = FindFirstObjectByType<SpacecraftCarrierEnemy>();
        fireParticleContainer = GameObject.Find("FireParticles");
    }

    [ClientRpc]
    void RpcDie()
    {
        if (fireParticleContainer == null) return;

        // fire and smoke particles
        var fireInstance = Instantiate(firePart, transform.position, Quaternion.identity);
        var smokeInstance = Instantiate(smokePart, transform.position, Quaternion.Euler(-90f, 0f, 0f));


        fireInstance.transform.SetParent(fireParticleContainer.transform, true);
        smokeInstance.transform.SetParent(fireParticleContainer.transform, true);

        var mainFire = fireInstance.main;
        mainFire.startSizeMultiplier = fireSmokePartScale;
        var mainSmoke = smokeInstance.main;
        mainSmoke.startSizeMultiplier = fireSmokePartScale * 2;

        fireInstance.Play();
        smokeInstance.Play();

        // explosion
        ExplosionParticles();
    }

    [TargetRpc]
    void TargetScoreAnim(NetworkConnection target)
    {
        GameObject srObj = Instantiate(scoreRewardPrefab, transform.position, Quaternion.identity);
        ScoreRewardAnim srAnim = srObj.GetComponent<ScoreRewardAnim>();
        if (srAnim != null)
        {
            srAnim.SetScore(scoreReward);
        }
    }

    [Server]
    IEnumerator DieWithDelayCoroutine()
    {
        yield return null;
        DOTween.Kill(mainSprite);
        DOTween.Kill(gameObject);
        Mirror.NetworkServer.Destroy(rootEnemy);
    }

    [Server]
    public override void Die(GameObject attacker)
    {
        //Debug.Log("KILLED ENEMY");
        if (!enemyKilled)
        {
            enemyKilled = true;

            if (spacecraftCarrierEnemy != null) {
                spacecraftCarrierEnemy.DestroyCannon();
            }
            // score reward
            if(attacker != null)
            {
                if (attacker.TryGetComponent<Player>(out var player))
                {
                    player.AddToScore(scoreReward);
                    TargetScoreAnim(player.connectionToClient);
                }
            }


            RpcDie();

            StartCoroutine(DieWithDelayCoroutine());
        }
    }
    protected override void OnCollisionWithPlayer(GameObject playerObj) {}
}
