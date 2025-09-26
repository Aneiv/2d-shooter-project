
using DG.Tweening;
using Mirror;
using System.Collections;
using UnityEngine;

// PARTIAL
// --- DEATH AND TAKING DAMAGE ---
// OF DreadWingEnemy

// Defines:
// - Take damage animations
// - Death actions and animations
public partial class DreadWingEnemy
{
    [Server]
    public override void TakeDamage(int damage, GameObject attacker)
    {
        if (!isVulnerable) return;

        RpcGetMaterials();
        //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
        currentHp = Mathf.Max(currentHp - damage, 0);
        if (currentHp <= 0)
        {
            Die(attacker);
        }
    }

    [ClientRpc]
    private void RpcGetMaterials()
    {
        // sprite bug fix
        if (mainSprite == null)
        {
            mainSprite = GetComponentInChildren<SpriteRenderer>();
        }
        if (mainMaterial == null)
        {
            mainMaterial = Resources.Load<Material>("Materials/Sprite-Lit-Default");
        }
        if (flashMaterial == null)
        {
            flashMaterial = Resources.Load<Material>("Materials/Flash_White");
        }
    }

    [Server]
    IEnumerator DieWithDelayCoroutine()
    {
        yield return new WaitForSeconds(4f);
        Mirror.NetworkServer.Destroy(rootEnemy);
    }

    [Server]
    public override void Die(GameObject attacker)
    {
        //reset screen clamp
        inputSystem.ResetScreenClamp();

        if (enemyKilled) return;
        enemyKilled = true;

        Player player = attacker.GetComponent<Player>();

        // death animation
        RpcSetDeathAnimation();

        // mini explosions
        ExplosionsAndScore(player);

        DOTween.Kill(mainSprite);
        foreach (var sprite in addSprites)
        {
            DOTween.Kill(sprite);
        }
        DOTween.Kill(gameObject);
        StartCoroutine(DieWithDelayCoroutine());

    }

    [ClientRpc]
    void RpcSetDeathAnimation()
    {
        // sprite
        if (mainSprite != null)
        {
            Color color = mainSprite.color;
            color.a = 1f;
            mainSprite.color = color;
            mainSprite.material = mainMaterial;
        }

        // animator
        if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (animator != null)
        {
            animator.SetTrigger("OnDeath");
        }
    }

    [ClientRpc]
    void RpcExplosions()
    {
        StartCoroutine(RpcExplosionsCoroutine());
    }
    private IEnumerator RpcExplosionsCoroutine()
    {
        // quick explosions
        foreach (GameObject obj in deathExplosionsObj)
        {
            Vector2 pos = obj.transform.position;
            ExplosionParticles(pos, null, null, 1f);
            // fire
            var fireInstance = Instantiate(firePart, pos, Quaternion.identity);
            fireInstance.transform.SetParent(transform, true);
            var mainFire = fireInstance.main;
            mainFire.startSizeMultiplier = fireSmokePartScale;
            fireInstance.Play();

            yield return new WaitForSeconds(miniExplosionDelay / 2f);
        }

        // explosions when boss is falling
        float scale = 1f;
        foreach (GameObject obj in deathExplosionsObj)
        {
            Vector2 pos = obj.transform.position;
            ExplosionParticles(pos, null, null, scale);
            yield return new WaitForSeconds(miniExplosionDelay);
            scale -= 0.1f;
        }
        yield return new WaitForSeconds(0.5f);
        ExplosionParticles(null, hugeExplosionTextPart, hugeFragPart, 0.8f, 75f, 90f);
    }

    [TargetRpc]
    void TargetScoreAnim(NetworkConnection target, float delay)
    {
        StartCoroutine(DelayedScoreAnim(delay));
    }
    IEnumerator DelayedScoreAnim(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject srObj = Instantiate(scoreRewardPrefab, transform.position, Quaternion.identity);
        ScoreRewardAnim srAnim = srObj.GetComponent<ScoreRewardAnim>();
        if (srAnim != null)
        {
            srAnim.SetScore(scoreReward);
        }
    }

    [Server]
    void ExplosionsAndScore(Player player)
    {
        RpcExplosions();

        // score reward with delay
        if (player != null)
        {
            float totalDelay =
                (deathExplosionsObj.Length * (miniExplosionDelay / 2f)) +
                (deathExplosionsObj.Length * miniExplosionDelay) +
                0.5f +
                0.1f;
            StartCoroutine(DelayedAddScoreAndNextWave(player, totalDelay));
            TargetScoreAnim(player.connectionToClient, totalDelay);
        }
    }
}

