using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class Enemy : MonoBehaviour, IHealthEnemy
{
    public int maxHp = 50;
    public int scoreReward = 10;
    private int currentHp;
    [HideInInspector]
    public GameObject waveManager;
    public EnemyHealthBar healthBar;
    public GameObject rootEnemy;
    protected bool enemyKilled = false;

    public Material flashMaterial;
    protected Material mainMaterial;
    protected SpriteRenderer mainSprite;
    public SpriteRenderer[] addSprites;
    public ParticleSystem[] engineParticles;
    public float flashDuration = 0.1f;

    public ParticleSystem explosionParticles;
    public ParticleSystem fragParticles;
    public float particleScale = 1.0f;
    private Vector3 vectorParticleStartScale=new Vector3(0.4f, 0.4f, 0.4f);

    public GameObject scoreRewardPrefab;

    protected bool isVulnerable = true;
    public bool IsVulnerable
    {
        get { return isVulnerable; } set { isVulnerable = value; }
    }

    protected virtual void Start()
    {
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);

        mainSprite = GetComponent<SpriteRenderer>();
        mainMaterial = mainSprite.material;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (isVulnerable)
        {
            //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
            if (currentHp - damage > 0)
            {
                currentHp -= damage;
                healthBar.SetHealth(currentHp);

                HitFlashAnim(mainSprite);
                foreach (var sprite in addSprites)
                {
                    HitFlashAnim(sprite);
                }
            }
            else
            {
                Die(attacker);
            }
        }
    }
    virtual public void Die(GameObject attacker)
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
                srAnim.SetText("+"+scoreReward.ToString());
            }

            ExplosionParticles();

            var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
            destroyTrigger.EnemyKilled();
            enemyKilled = true;

            DOTween.Kill(mainSprite);
            foreach (var sprite in addSprites)
            {
                DOTween.Kill(sprite);
            }
            DOTween.Kill(gameObject);
            Destroy(rootEnemy);
        }
    }

    protected void HitFlashAnim(SpriteRenderer sprite)
    {
        sprite.DOFade(0.1f, flashDuration)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject)
            .OnStart(() =>
            {
                float halfTime = flashDuration / 2f;
                DOVirtual.DelayedCall(halfTime, () =>
                {
                if (flashMaterial != null && !enemyKilled)
                    {
                        sprite.material = flashMaterial;
                        foreach(var particle in engineParticles)
                        {
                            if(particle != null)
                                particle.gameObject.SetActive(false);
                        }
                    } 
                });
            })
            .OnComplete(() =>
            {
                sprite.DOFade(1f, flashDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        if (!enemyKilled) {
                            sprite.material = mainMaterial;
                            foreach (var particle in engineParticles)
                            {
                                if (particle != null)
                                    particle.gameObject.SetActive(true);
                            }
                        }
                    });
            });
    }

    protected void ExplosionParticles(Vector2? position = null,
        ParticleSystem explPart = null,
        ParticleSystem fragPart = null,
        float? newParticleScale = null,
        float? YRotation = null)
    {
        particleScale = newParticleScale ?? particleScale;

        Vector2 pos = position ?? (Vector2)transform.position;
        ParticleSystem usedExplosionParticles = explPart ?? explosionParticles;
        ParticleSystem usedFragParticles = fragPart ?? fragParticles;

        float xRot = YRotation != null ? -16f : 0f;
        Quaternion rotation = Quaternion.Euler(xRot, YRotation ?? 0f, 0f);

        ParticleSystem explosion = Instantiate(usedExplosionParticles, pos, rotation);
        explosion.transform.localScale = vectorParticleStartScale * particleScale;
        explosion.transform.parent = null;
        explosion.Play();
        Destroy(explosion.gameObject, 3f);

        ParticleSystem frag = Instantiate(usedFragParticles, pos, rotation);
        frag.transform.localScale = vectorParticleStartScale * particleScale;
        frag.transform.parent = null;
        frag.Play();
        Destroy(frag.gameObject, 5f);
    }

    public bool IsAlive()
    {
        return !enemyKilled;
    }
}
