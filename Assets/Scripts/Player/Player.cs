using UnityEngine;

public class Player : MonoBehaviour, IShooter, IHealth
{
    public int hp = 50;
    public int bulletDamage = 20;
    public int BulletDamage => bulletDamage;

    public GameObject GameOverUI;
    public GameObject gameUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        //Debug.Log("Player took: " + damage.ToString() + " dmg");
        if (hp - damage > 0)
        {
            hp -= damage;
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
