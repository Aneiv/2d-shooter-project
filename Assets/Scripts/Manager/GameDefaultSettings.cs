using TMPro;
using UnityEngine;

public class GameDefaultSettings : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI highscoreText;

    void Start()
    {
        DontDestroyOnLoad(gameObject);//dont destroy after scene change
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;//set framerate to refresh rate
        //Highscore load from PlayerPrefs
        int highscore = PlayerPrefs.GetInt("highscore", 0);
        highscoreText.text = highscore.ToString();
    }
}
