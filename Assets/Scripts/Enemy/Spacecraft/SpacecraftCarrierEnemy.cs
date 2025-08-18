using UnityEngine;
using DG.Tweening;
using System.Collections;

public class SpacecraftCarrierEnemy : Enemy
{
    public GameObject[] deathExplosionsObj;
    public float miniExplosionDelay = 0.2f;
    public ParticleSystem hugeExplosionPart;
    public ParticleSystem hugeFragPart;

    [Header("Cannons")]
    public int cannonCounter = 6;
    public GameObject[] cannonsObjs;
    public GameObject[] cannonContainers;

    private SpacecraftCarrierSpawner SpacecraftCarrierSpawner;
    private Animator animator;

    protected override void Start()
    {
        base.Start();        
        isVulnerable = false;
        SpacecraftCarrierSpawner = GetComponent<SpacecraftCarrierSpawner>();
        animator = GetComponent<Animator>();
    }
    public void DestroyCannon()
    {
        cannonCounter--;
        if (cannonCounter <= 0) {
            isVulnerable = true;
        }
    }

    public override void OnArrival()
    {
        isVulnerable = false;
        foreach (GameObject obj in cannonsObjs)
        {
            if (obj.TryGetComponent<IShootReady>(out var cannon))
            {
                //cannon.ReadyToShoot();
            }
        }
        foreach(GameObject container in cannonContainers)
        {
            if(container.TryGetComponent<FollowSprite>(out var followSprite))
            {
                //followSprite.StartFollow();
            }
        }

        if(SpacecraftCarrierSpawner != null)
        {
            SpacecraftCarrierSpawner.StartSpawningEnemies();
        }
    }

    public override void TakeDamage(int damage, GameObject attacker)
    {
        if (isVulnerable)
        {
            //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
            if (currentHp - damage > 0)
            {
                currentHp -= damage;
                healthBar.SetHealth(currentHp);

                HitFlashAnim(mainSprite);
            }
            else
            {
                Die(attacker);
            }
        }
    }

    public override void Die(GameObject attacker)
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
                srAnim.SetScore(scoreReward);
            }
            // death animation
            Color color = mainSprite.color;
            color.a = 1f;
            mainSprite.color = color;
            mainSprite.material = mainMaterial;

            animator.SetTrigger("OnDeath");

            // mini explosions
            StartCoroutine(ExplosionsCoroutine());
            

            var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
            destroyTrigger.EnemyKilled();
            enemyKilled = true;

            DOTween.Kill(mainSprite);
            foreach (var sprite in addSprites)
            {
                DOTween.Kill(sprite);
            }
            DOTween.Kill(gameObject);
            Destroy(rootEnemy, 2f);
        }
    }

    IEnumerator ExplosionsCoroutine()
    {
        float scale = 1f;
        foreach (GameObject obj in deathExplosionsObj)
        {
            Vector2 pos = obj.transform.position;
            ExplosionParticles(pos, null, null, scale);
            yield return new WaitForSeconds(miniExplosionDelay);
            scale -= 0.1f;
        }
        yield return new WaitForSeconds(0.1f);
        ExplosionParticles(null, hugeExplosionPart, hugeFragPart, 0.8f, 90f);
    }

    protected override void OnCollisionWithPlayer(GameObject playerObj) {}
}

