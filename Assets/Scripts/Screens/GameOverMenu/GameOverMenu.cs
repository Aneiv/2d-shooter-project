using DG.Tweening;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GameOverMenu : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    public GameObject newText;
    public GameObject newText_2;
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
        //score numbers animations
        DisplayNumberAnimation(scoreText, 0, score, 0.6f);
        if (score > highscore)
        {
            //save new highscore
            PlayerPrefs.SetInt("highscore", score);
            highscore = score;
            highscoreText.text = score.ToString();
            newText.SetActive(true);
            DisplayNewTextAnimation(newText, newText_2);
        }
        DisplayNumberAnimation(highscoreText, 0, highscore, 0.6f);
    }

    private void DisplayNumberAnimation(TMP_Text numberText, int currentScore, int targetScore, float animationPace)
    {
        numberText.text = currentScore.ToString(); //make display old value on UI to simulate animation for value increase
        DOVirtual.Int(currentScore, targetScore, animationPace, (x) =>
        {
            currentScore = x;
            numberText.text = currentScore.ToString();
        })
        .SetEase(Ease.InOutSine)
        .SetUpdate(true);// for unscaledDeltaTime
    }
    private void DisplayNewTextAnimation(GameObject newText, GameObject newText2)
    {
        var newTextMesh = newText.GetComponent<TextMeshProUGUI>();
        var newText2Mesh = newText2.GetComponent<TextMeshProUGUI>();

        //start parameters
        newTextMesh.alpha = 0; //start alpha

        //Animation
        Sequence seq = DOTween.Sequence();

        seq.Join(newText.transform.DORotate(new Vector3(0, 0, 1119f), 0.7f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutSine));
        seq.Join(newTextMesh.DOFade(1f, 0.75f).SetEase(Ease.OutSine));
        seq.Join(newText.transform.DOScale(1f, 0.75f).SetEase(Ease.InOutSine));

        seq.SetUpdate(true);

        seq.OnComplete(() =>
        {
            newText2.SetActive(true);
            newTextMesh.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            newText2Mesh.DOFade(0f, 1f)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
            //infinite animation after first animation
            //scale
            newText.transform.DOScale(1.1f, 1.6f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);

            //left-right rotation
            newText.transform.DORotate(new Vector3(0, 0, 5f), 1.5f, RotateMode.LocalAxisAdd)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
        }
            );

    }
}
