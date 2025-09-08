using DG.Tweening;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour, IHealth
{
    public int maxHp = 80;
    public int currentScore = 0;
    private int currentHp;
    private int currentNumberOfCoins = 0;
    private Animator playerAnimator;
    private GameOverMenu gameOverMenu;

    public GameObject GameOverUI;
    public GameObject gameUI;
    public HealthBar healthBar;
    public TMP_Text totalScoreText;
    public TMP_Text totalCoinsTextUI;
    public TMP_Text totalCoinsTextPause;
    public GameObject coinUI;
    public GameObject canvas;
    
    void Start()
    {
        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);
        playerAnimator = GetComponent<Animator>();
        totalScoreText.text = currentScore.ToString();
        gameOverMenu = canvas.GetComponent<GameOverMenu>();
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
        OnGameOver();

        Destroy(gameObject);
    }

    public void OnGameOver()
    {
        gameUI.SetActive(false);
        GameOverUI.SetActive(true);
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;
        gameOverMenu.OnMenuShow();
    }

    public void AddToScore(int score)
    {
        DisplayNumberAnimation(totalScoreText, currentScore, currentScore + score, 0.6f);
        currentScore += score;
        totalScoreText.text = currentScore.ToString(); //update score value
    }

    public void AddToCoins(int coinsNumber)
    {
        CoinTextUI coinTextUI = coinUI.GetComponent<CoinTextUI>();
        if (coinTextUI != null)
        {
            coinTextUI.showCoins();
            DisplayNumberAnimation(totalCoinsTextUI, currentNumberOfCoins, currentNumberOfCoins + coinsNumber, 0.2f);
        }
        currentNumberOfCoins += coinsNumber;
        totalCoinsTextUI.text = currentNumberOfCoins.ToString();//update score value
        totalCoinsTextPause.text = currentNumberOfCoins.ToString();//update score value
    }

    private void DisplayNumberAnimation(TMP_Text numberText, int currentScore, int targetScore, float animationPace)
    {
        numberText.text = currentScore.ToString(); //make display old value on UI to simulate animation for value increase
        DOVirtual.Int(currentScore, targetScore, animationPace, (x) =>
        {
            currentScore = x;
            numberText.text = currentScore.ToString();
        })
        .SetEase(Ease.Linear);
    }
}
