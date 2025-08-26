using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    public GameObject newText;
    public GameObject player;
    private int highscore;
    private int score;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        highscore = PlayerPrefs.GetInt("highscore", 0);
    }

    public void OnMenuShow()
    {
        score = player.GetComponent<Player>().currentScore;
        scoreText.text = score.ToString();
        highscoreText.text = highscore.ToString();
        if (score > highscore)
        {
            //save new highscore
            PlayerPrefs.SetInt("highscore", score);
            highscoreText.text = score.ToString();
            newText.SetActive(true);

        }
    }
}
