using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public interface IHealthEnemy
{
    void TakeDamage(int damage, GameObject attacker);
    void Die(GameObject attacker);
    bool IsVulnerable { get; set; }

    bool IsAlive();
}

