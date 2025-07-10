using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthBar;
    public Slider comboBar;
    public Gradient Gradient;
    public Image fillHealth;
    public CanvasGroup HealthBarUI;

    public float comboDropSpeed = 0.1f;

    private float comboTimer = 0f;
    public float comboDuration = 1.0f;

    private float showUITimer = 0f;
    public float showUIDuration = 2.0f;
    public float hideUISpeed = 5f;
    private float lastHealthVal = 0f;


    public void SetMaxHealth(int health)
    {
        healthBar.maxValue = health;
        healthBar.value = health;

        comboBar.maxValue = health;
        comboBar.value = health;

        fillHealth.color = Gradient.Evaluate(1f);

        lastHealthVal = health;

        HealthBarUI.alpha = 0f; // hide bar
    }

    public void SetHealth(int health)
    {
        healthBar.value = health;
        comboBar.value = lastHealthVal;
      

        fillHealth.color = Gradient.Evaluate(healthBar.normalizedValue);

        comboTimer = comboDuration;
        showUITimer = showUIDuration;

        HealthBarUI.alpha = 1f; // show bar

    }

    private void FixedUpdate()
    {
        // combo bar drop
        if (healthBar.value < comboBar.value && comboTimer <= 0) {
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
}
