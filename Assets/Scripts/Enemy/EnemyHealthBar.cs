using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : HealthBar
{
    public CanvasGroup HealthBarUI;

    protected float showUITimer = 0f;
    public float showUIDuration = 2.0f;
    public float hideUISpeed = 5f;
    protected float lastHealthVal = 0f;

    public override void Hide()
    {
        showUITimer = 0f; // hide bar with fade out
    }

    public override void SetMaxHealth(int health)
    {
        base.SetMaxHealth(health);

        lastHealthVal = health;

        HealthBarUI.alpha = 0f; // hide bar
    }

    public override void SetHealth(int health)
    {
        base.SetHealth(health);

        comboBar.value = lastHealthVal;

        showUITimer = showUIDuration;

        HealthBarUI.alpha = 1f; // show bar

    }

    protected override void FixedUpdate()
    {
        // combo bar drop
        if (healthBar.value < comboBar.value && comboTimer <= 0)
        {
            comboBar.value -= comboDropSpeed * Time.deltaTime;
            lastHealthVal = healthBar.value;
        }

        // fade out
        if (showUITimer <= 0 && HealthBarUI.alpha > 0)
        {
            comboBar.value = 0f;
            HealthBarUI.alpha = Mathf.Max(0f, HealthBarUI.alpha - hideUISpeed * Time.deltaTime);
        }
        comboTimer -= Time.deltaTime;
        showUITimer -= Time.deltaTime;
    }
    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity; // health bar doesn't rotate
    }
}
