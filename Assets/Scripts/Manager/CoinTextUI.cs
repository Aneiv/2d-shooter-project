using UnityEngine;

public class CoinTextUI : MonoBehaviour
{
    public float showDuration = 3.0f;
    public float fadeDuration = 1.0f;
    private bool isShown = false;
    private float timer = 0f;
    private CanvasGroup canvasGroup;
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
        if (isShown)
        {
            timer += Time.deltaTime;

            if (timer <= showDuration)
            {
                canvasGroup.alpha = 1f;
            }
            else if (timer <= showDuration + fadeDuration)
            {
                float fadeTimer = timer - showDuration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            }
            else
            {
                isShown = false;
                canvasGroup.alpha = 0f;
            }
        }
    }

    public void showCoins()
    {
        timer = 0f;
        isShown = true;
    }
}
