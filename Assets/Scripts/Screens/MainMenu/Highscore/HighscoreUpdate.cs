using TMPro;
using UnityEngine;

public class HighscoreUpdate : MonoBehaviour
{
    public TextMeshProUGUI highscoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var settings = GameSettings.Instance;
        highscoreText.text = settings.highscore.ToString();
    }
}
