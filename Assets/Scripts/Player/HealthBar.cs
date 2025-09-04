using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    public Slider comboBar;
    public Gradient Gradient;
    public Image fillHealth;

    public float comboDropSpeed = 0.1f;

    protected float comboTimer = 0f;
    public float comboDuration = 1.0f;


    public virtual void SetMaxHealth(int health)
    {
        healthBar.maxValue = health;
        healthBar.value = health;

        comboBar.maxValue = health;
        comboBar.value = health;

        fillHealth.color = Gradient.Evaluate(1f);
    }

    public virtual void SetHealth(int health)
    {
        healthBar.value = health;

        fillHealth.color = Gradient.Evaluate(healthBar.normalizedValue);

        comboTimer = comboDuration;
    }

    protected virtual void FixedUpdate()
    {
        // combo bar drop
        if (healthBar.value < comboBar.value && comboTimer <= 0) {
            comboBar.value -= comboDropSpeed * Time.deltaTime;
        }
        
        comboTimer -= Time.deltaTime;
    }
    public virtual void Show() { throw new NotImplementedException(); }

    public virtual void Hide() { throw new NotImplementedException(); }
}
