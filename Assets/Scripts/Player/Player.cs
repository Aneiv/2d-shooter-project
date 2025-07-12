using UnityEngine;

public class Player : MonoBehaviour, IShooter, IHealth
{
    public int maxHp = 50;
    private int currentHp;
    public int bulletDamage = 20;
    public int BulletDamage => bulletDamage;

    public GameObject GameOverUI;
    public GameObject gameUI;
    public PlayerHealthBar healthBar;

    void Start()
    {
        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);
    }
    public void TakeDamage(int damage)
    {
        //Debug.Log("Player took: " + damage.ToString() + " dmg");
        if (currentHp - damage > 0)
        {
            currentHp -= damage;
            healthBar.SetHealth(currentHp);
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
