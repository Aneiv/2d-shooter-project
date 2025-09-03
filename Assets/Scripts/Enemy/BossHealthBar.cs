using TMPro;
using UnityEngine;

public class BossHealthBar : HealthBar
{
    public TMP_Text bossNameText;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public override void Show()
    {
        canvasGroup.alpha = 1f;
    }

    public override void Hide()
    {
        canvasGroup.alpha = 0f;
    }
    public void SetBossName(string name)
    {
        if(bossNameText != null)
        {
            bossNameText.text = name;
        }
    }
}

