using TMPro;
using UnityEngine;

public class ScoreRewardAnim : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float fadeDuration = 1.0f;
    private float timer = 0f;
    private float initScale = 0.006f;
    private float size = 1f;
    private CanvasGroup canvasGroup;
    public TMP_Text scoreText;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        timer += Time.deltaTime;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
        }

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }

    public void SetScore(int score)
    {
        if(scoreText != null)
        {
            scoreText.text = "+" + score.ToString();

            size = Mathf.Min(Mathf.Log10(score), 2f);
            transform.localScale = new Vector3(initScale, initScale, initScale) * size;
        }
    }
}