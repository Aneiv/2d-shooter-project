using UnityEngine;

public class Enemy : MonoBehaviour, IHealthEnemy
{
    public int maxHp = 50;
    public int scoreReward = 10;
    private int currentHp;
    public GameObject waveManager;
    public EnemyHealthBar healthBar;
    public GameObject rootEnemy;
    bool enemyKilled = false;

    private Animator animator;
    public Animator[] aditionalAnimators;

    public ParticleSystem explosionParticles;
    public ParticleSystem fragParticles;
    public float particleScale = 1.0f;
    private Vector3 vectorParticleStartScale=new Vector3(0.4f, 0.4f, 0.4f);
    void Start()
    {
        waveManager = GameObject.FindGameObjectWithTag("GameController");
        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
        if (currentHp - damage > 0)
        {
            currentHp -= damage;
            healthBar.SetHealth(currentHp);

            HitFlashAnim();
        }
        else
        {
            Die(attacker);
        }
    }
    public void Die(GameObject attacker)
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

            ExplosionParticles();

            var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
            destroyTrigger.EnemyKilled();
            enemyKilled = true;
            Destroy(rootEnemy);
        }
    }

    private void HitFlashAnim()
    {
        animator.SetTrigger("DamageReceived");

        foreach (var anim in aditionalAnimators)
        {
            anim.SetTrigger("DamageReceived");
        }
    }

    private void ExplosionParticles()
    {
        ParticleSystem explosion = Instantiate(explosionParticles, transform.position, Quaternion.identity);
        explosion.transform.localScale = vectorParticleStartScale * particleScale;
        explosion.Play();
        Destroy(explosion.gameObject, 0.5f);

        ParticleSystem frag = Instantiate(fragParticles, transform.position, Quaternion.identity);
        frag.transform.localScale = vectorParticleStartScale * particleScale;
        frag.Play();
        Destroy(frag.gameObject, 1f);
    }
}
