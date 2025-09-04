using System;
using UnityEngine;

[Obsolete]
public class DreadWingEnemyHealthBar : EnemyHealthBar
{
    public GameObject dreadWingEnemy;
    private DreadWingEnemy dreadWingEnemyScript;
    private void Start()
    {
        dreadWingEnemyScript = dreadWingEnemy.GetComponent<DreadWingEnemy>();
    }
    [Obsolete]
    public void SingleCannonDestroyed(float damage)
    {
        float currentHp = lastHealthVal - damage;
        healthBar.value = currentHp;
        comboBar.value = lastHealthVal;


        fillHealth.color = Gradient.Evaluate(healthBar.normalizedValue);

        comboTimer = comboDuration;
        showUITimer = showUIDuration;

        HealthBarUI.alpha = 1f; // show bar
    }

}
