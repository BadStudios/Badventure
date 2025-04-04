using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badventure.Scripts.Csharp.Solution.Models
{
    public class PlayerModel : EntityModel
    {
        public AbilityModel Ability1 { get; set; }
        public AbilityModel Ability2 { get; set; }
        public AbilityModel Ultimate { get; set; }

        public PlayerModel(string name,
            int maxHealthPoints,
            int healthRegenerationIndex,
            int baseDamage,
            int baseArmor,
            int attackRange,
            int attackSpeed,
            int movementSpeed,
            int sprintMultiplier,
            int jumpHeight,
            int criticalChance,
            AbilityModel ability1, 
            AbilityModel ability2, 
            AbilityModel ultimate,
            bool isVisible = true,
            bool isAlive = true
        ) 
        {
            this.Name = name;
            this.MaxHealth = maxHealthPoints;
            this.HealthRegenerationFactor = healthRegenerationIndex;
            this.BaseDamage = baseDamage;
            this.BaseArmor = baseArmor;
            this.AttackRange = attackRange;
            this.AttackSpeed = attackSpeed;
            this.MovementSpeed = movementSpeed;
            this.SprintMultiplier = sprintMultiplier;
            this.JumpPower = jumpHeight;
            this.CriticalChance = criticalChance;
            this.Ability1 = ability1;
            this.Ability2 = ability2;
            this.Ultimate = ultimate;
            this.IsVisible = isVisible;
            this.IsAlive = isAlive;
            this.Level = 0;
            this.Experience = 0;
            this.Health = this.MaxHealth;
        }
    }
}
