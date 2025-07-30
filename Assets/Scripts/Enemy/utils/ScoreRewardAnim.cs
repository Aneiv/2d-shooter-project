using TMPro;
using UnityEngine;

public class ScoreRewardAnim : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float fadeDuration = 1.0f;
    private float timer = 0f;
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

    public void SetText(string text)
    {
        if(scoreText != null)
        {
            scoreText.text = text;
        }
    }
}