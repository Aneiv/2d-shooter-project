using UnityEngine;

public class Enemy : MonoBehaviour, IHealth
{
    public int maxHp = 50;
    private int currentHp;
    public GameObject waveManager;
    public EnemyHealthBar healthBar;
    public GameObject rootEnemy;
    bool enemyKilled = false;

    private Animator animator;
    public Animator[] aditionalAnimators;
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

    public void TakeDamage(int damage)
    {
        //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
        if (currentHp - damage > 0)
        {
            currentHp -= damage;
            healthBar.SetHealth(currentHp);
            animator.SetTrigger("DamageReceived");

            foreach (var anim in aditionalAnimators)
            {
                anim.SetTrigger("DamageReceived");
            }
        }
        else
        {
            Die();
        }
    }
    public void Die()
    {
        //Debug.Log("KILLED ENEMY");
        if (!enemyKilled)
        {
            var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
            destroyTrigger.EnemyKilled();
            enemyKilled = true;
            Destroy(rootEnemy);
        }
    }
}
