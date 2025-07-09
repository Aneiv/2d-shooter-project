using UnityEngine;

public class EllipticalMotion : MonoBehaviour
{
    public float a = 25f;
    public float b = 15f;
    public float transformationSpeed = 1f;

    private RectTransform rectTransform;
    private Vector2 center;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        center = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float t = Time.time * transformationSpeed;
        float x = a * Mathf.Cos(t);
        float y = b * Mathf.Sin(t);

        rectTransform.anchoredPosition = center + new Vector2(x, y);
    }
}
