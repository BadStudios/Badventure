using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badventure.Scripts.Csharp.Solution.Models
{
    public class EntityModel
    {
        //Initialization of private variables
        private int _health;
        private int _maxHealth;
        private int _healthRegenerationFactor;
        private int _baseDamage;
        private int _baseArmor;
        private string _name;
        private bool _isVisible;
        private bool _isAlive;
        private int _level;
        private int _experience;
        private int _id;
        private static int _idCounter = 0;
        private int _attackSpeed;
        private int _attackRange;
        private int _movementSpeed;
        private int _sprintMultiplier;
        private int _jumpPower;
        private int _criticalChance;
        private List<EffectModel> _effects;

        public int Health
        {
            get => _health;
            set => _health = value >= 0 ? value : 0;
        }

        public string Name { get; set; }

        public int MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = value > 0 ? value : 1;
        }

        public int HealthRegenerationFactor
        {
            get => _healthRegenerationFactor;
            set => _healthRegenerationFactor = value;
        }

        public int BaseDamage
        {
            get => _baseDamage;
            set => _baseDamage = value > 0 ? value : 1;
        }

        public int BaseArmor
        {
            get => _baseArmor;
            set => _baseArmor = value > 0 ? value : 1;
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        public bool IsAlive
        {
            get => _isAlive;
            set => _isAlive = value;
        }

        public int Level
        {
            get => _level;
            set => _level = value > 0 ? value : 1;
        }

        public int Experience
        {
            get => _experience;
            set => _experience = value > 0 ? value : 1;
        }

        public int Id
        {
            get => _id;
            set
            {
                if (value > _idCounter)
                {
                    _id = value;
                }
                else
                {
                    _id = _idCounter + 1;
                }
                _idCounter = _id;
            }
        }

        public int AttackSpeed
        {
            get => _attackSpeed;
            set => _attackSpeed = value > 0 ? value : 1;
        }

        public int AttackRange
        {
            get => _attackRange;
            set => _attackRange = value > 0 ? value : 1;
        }

        public int MovementSpeed
        {
            get => _movementSpeed;
            set => _movementSpeed = value > 0 ? value : 1;
        }

        public int SprintMultiplier
        {
            get => _sprintMultiplier;
            set => _sprintMultiplier = value > 0 ? value : 1;
        }
        public int JumpPower
        {
            get => _jumpPower;
            set => _jumpPower = value > 0 ? value : 1;
        }

        public int CriticalChance
        {
            get => _criticalChance;
            set => _criticalChance = value > 0 ? value : 1;
        }

        public List<EffectModel> Effects
        {
            get => _effects;
            set => _effects = value ?? new List<EffectModel>();
        }

        public void Heal(int amount)
        {
            this.Health += amount;
        }

        public void TakeDamage(int amount)
        {
            this.Health -= amount - (int)Mathf.Floor((float)BaseArmor / 100F);
        }

        public void AddExperience(int amount)
        {
            this.Experience += amount;
            if (this.Experience >= this.Level * 100)
            {
                this.LevelUp();
            }
        }

        public void LevelUp()
        {
            this.Level++;
            this.MaxHealth += 10;
            this.BaseDamage += 1;
            this.BaseArmor += 1;
            this.Health = this.MaxHealth;
        }

        public void AddEffect(EffectModel effect)
        {
            this.Effects.Add(effect);
        }

        public void RemoveEffect(EffectModel effect)
        {
            this.Effects.Remove(effect);
        }

        public void Die()
        {
            this.IsAlive = false;
        }

        public void Respawn()
        {
            this.IsAlive = true;
            this.Health = this.MaxHealth;
        }
    }
}
