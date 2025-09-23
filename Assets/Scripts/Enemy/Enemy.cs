using DG.Tweening;
using Mirror;
using UnityEngine;

public class Enemy : Mirror.NetworkBehaviour, IHealthEnemy, IEnemy
{
    [Header("Health stuff")]
    public int maxHp = 50;
    public int collisionDamage = 20;
    [SyncVar(hook = nameof(OnHealthChanged))] protected int currentHp;
    protected bool isVulnerable = false;
    public bool IsVulnerable
    {
        get { return isVulnerable; }
        set
        {
            isVulnerable = value;
            if (value == false && healthBar != null)
            {
                healthBar.Hide();
            }
        }
    }
    protected bool enemyKilled = false;

    [Header("UI stuff")]
    public int scoreReward = 10;
    public GameObject scoreRewardPrefab;
    public HealthBar healthBar;

    [Header("Flash Animation stuff")]
    public Material flashMaterial;
    protected Material mainMaterial;
    protected SpriteRenderer mainSprite;
    public SpriteRenderer[] addSprites;
    public GameObject fireAndSmokeParticles;
    public ParticleSystem[] engineParticles;
    public float flashDuration = 0.1f;

    [Header("Death stuff")]
    public GameObject rootEnemy;
    public ParticleSystem explosionParticles;
    public ParticleSystem fragParticles;
    public float particleScale = 1.0f;
    private Vector3 vectorParticleStartScale = new Vector3(0.4f, 0.4f, 0.4f);

    [HideInInspector]
    public GameObject waveManager;
    protected EnemyShoot enemyShoot;

    [SyncVar(hook = nameof(OnRotationChanged))] private Quaternion syncRotation;

    protected virtual void Start()
    {
        isVulnerable = false;
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        currentHp = maxHp;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHp);
        }
        mainSprite = GetComponent<SpriteRenderer>();
        enemyShoot = GetComponent<EnemyShoot>();
        mainMaterial = mainSprite.material;
    }

    protected void FixedUpdate()
    {
        if (isServer)
        {
            syncRotation = transform.rotation;
        }
    }

    void OnRotationChanged(Quaternion oldRotation, Quaternion newRotation)
    {
        transform.rotation = newRotation;
    }

    [Server]
    virtual public void TakeDamage(int damage, GameObject attacker)
    {
        if (!isVulnerable)
        {
            InvulnerableHitRpc();
            return;
        }
        currentHp = Mathf.Max(currentHp - damage, 0);
        if (currentHp <= 0)
        {
            //GameObject attacker = attackerNetId != null ? attackerNetId.gameObject : null;
            Die(attacker);
        }
    }
    void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (newHealth <= 0) return;
        else
        {
            healthBar.SetHealth(currentHp);
            HitFlashAnim(mainSprite);
            foreach (var sprite in addSprites)
            {
                HitFlashAnim(sprite);
            }
        }
    }
    [ClientRpc]
    protected void InvulnerableHitRpc()
    {
        InVulnerableHitAnim(mainSprite);
        foreach (var sprite in addSprites)
            InVulnerableHitAnim(sprite);
    }

    [ClientRpc]
    void RpcDie()
    {
        Mirror.NetworkServer.Destroy(rootEnemy);
        ExplosionParticles();
        if (isLocalPlayer)
        {
            GameObject srObj = Instantiate(scoreRewardPrefab, transform.position, Quaternion.identity);
            ScoreRewardAnim srAnim = srObj.GetComponent<ScoreRewardAnim>();
            if (srAnim != null)
            {
                srAnim.SetScore(scoreReward);
            }
        }
    }

    [Server]
    virtual public void Die(GameObject attacker = null)
    {
        if (enemyKilled) return;
        // score reward
        if (attacker != null)
        {
            Player player = attacker.GetComponent<Player>();
            if (player != null)
            {
                player.AddToScore(scoreReward);
            }
        }

        var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
        destroyTrigger.EnemyKilled();
        enemyKilled = true;

        DOTween.Kill(mainSprite);
        foreach (var sprite in addSprites) DOTween.Kill(sprite);
        DOTween.Kill(gameObject);
        RpcDie();
    }

    [Server]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isVulnerable)
        {
            GameObject playerObj = collision.gameObject;
            OnCollisionWithPlayer(playerObj);
        }
    }

    virtual protected void OnCollisionWithPlayer(GameObject playerObj)
    {
        Player player = playerObj.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(collisionDamage);
        }
        Die(playerObj);
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
                        foreach (var particle in engineParticles)
                        {
                            if (particle != null)
                                particle.gameObject.SetActive(false);
                        }
                        if (fireAndSmokeParticles != null) fireAndSmokeParticles.SetActive(false);
                    }
                });
            })
            .OnComplete(() =>
            {
                sprite.DOFade(1f, flashDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        if (!enemyKilled)
                        {
                            sprite.material = mainMaterial;
                            foreach (var particle in engineParticles)
                            {
                                if (particle != null)
                                    particle.gameObject.SetActive(true);
                            }
                            if (fireAndSmokeParticles != null) fireAndSmokeParticles.SetActive(true);
                        }
                    });
            });
    }

    protected void InVulnerableHitAnim(SpriteRenderer sprite)
    {
        Material mat = sprite.material;
        mat.DOColor(new Color(0f, 1.5f, 3f, 0.3f), flashDuration) // go to blue color
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject)
            .OnStart(() =>
            {
                if (!enemyKilled)
                {
                    foreach (var particle in engineParticles)
                    {
                        if (particle != null)
                            particle.gameObject.SetActive(false);
                    }
                    if (fireAndSmokeParticles != null) fireAndSmokeParticles.SetActive(false);
                }
            })
            .OnComplete(() =>
            {
                mat.DOColor(Color.white, flashDuration) // go to default color
                    .SetEase(Ease.InOutSine)
                    .SetLink(gameObject)
                    .OnComplete(() =>
                    {
                        if (!enemyKilled)
                        {
                            foreach (var particle in engineParticles)
                            {
                                if (particle != null)
                                    particle.gameObject.SetActive(true);
                            }
                            if (fireAndSmokeParticles != null) fireAndSmokeParticles.SetActive(true);
                        }
                    });
            });
    }

    protected void ExplosionParticles(Vector2? position = null,
        ParticleSystem explPart = null,
        ParticleSystem fragPart = null,
        float? newParticleScale = null,
        float? XRotation = null,
        float? YRotation = null)
    {
        particleScale = newParticleScale ?? particleScale;

        Vector2 pos = position ?? (Vector2)transform.position;
        ParticleSystem usedExplosionParticles = explPart ?? explosionParticles;
        ParticleSystem usedFragParticles = fragPart ?? fragParticles;

        Quaternion rotation = Quaternion.Euler(XRotation ?? 0f, YRotation ?? 0f, 0f);

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

    virtual public void OnArrival()
    {
        isVulnerable = true;
        if (enemyShoot != null)
        {
            enemyShoot.OnArrival();
        }
    }
}
