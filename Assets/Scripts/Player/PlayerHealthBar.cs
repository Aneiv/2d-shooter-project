using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Slider healthBar;
    public Slider comboBar;
    public Gradient Gradient;
    public Image fillHealth;

    public float comboDropSpeed = 0.1f;

    private float comboTimer = 0f;
    public float comboDuration = 1.0f;


    public void SetMaxHealth(int health)
    {
        healthBar.maxValue = health;
        healthBar.value = health;

        comboBar.maxValue = health;
        comboBar.value = health;

        fillHealth.color = Gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        healthBar.value = health;

        fillHealth.color = Gradient.Evaluate(healthBar.normalizedValue);

        comboTimer = comboDuration;
    }

    private void FixedUpdate()
    {
        // combo bar drop
        if (healthBar.value < comboBar.value && comboTimer <= 0) {
            comboBar.value -= comboDropSpeed * Time.deltaTime;
        }
        
        comboTimer -= Time.deltaTime;
    }
}
