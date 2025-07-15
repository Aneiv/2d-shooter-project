using UnityEngine;

public class Player : MonoBehaviour, IHealth
{
    public int maxHp = 80;
    private int currentHp;
    private Animator playerAnimator;

    public GameObject GameOverUI;
    public GameObject gameUI;
    public PlayerHealthBar healthBar;
    void Start()
    {
        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);
        playerAnimator = GetComponent<Animator>();
    }
    public void TakeDamage(int damage)
    {
        //Debug.Log("Player took: " + damage.ToString() + " dmg");
        if (currentHp - damage > 0)
        {
            currentHp -= damage;
            healthBar.SetHealth(currentHp);
            //Damage received animation
            playerAnimator.SetTrigger("DamageReceived");
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        gameUI.SetActive(false);
        GameOverUI.SetActive(true);
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;

        Destroy(gameObject);
    }
}
