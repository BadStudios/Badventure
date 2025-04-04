using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badventure.Scripts.Csharp.Solution.Models
{
    public class AbilityModel
    {
        private int _id;
        private static int _idCounter = 0;
        private string _name;
        private string _description;
        private int _damage;
        private int _cooldown;
        private int _currentCooldown;
        private string _iconPath;
        private string _modelPath;
        private string _prefabPath;

        public AbilityModel(string name, string description, int damage, int cooldown, string iconPath, string modelPath, string prefabPath)
        {
            _idCounter++;
            this._id = _idCounter;
            this._name = name;
            this._description = description;
            this._damage = damage;
            this._cooldown = cooldown;
            this._iconPath = iconPath;
            this._modelPath = modelPath;
            this._prefabPath = prefabPath;
        }

        public int Id
        {
            get => _id;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public int Damage
        {
            get => _damage;
            set => _damage = value;
        }

        public int Cooldown
        {
            get => _cooldown;
            set => _cooldown = value;
        }

        public int CurrentCooldown
        {
            get => _currentCooldown;
            set => _currentCooldown = value;
        }

        public string IconPath
        {
            get => _iconPath;
            set => _iconPath = value;
        }

        public string ModelPath
        {
            get => _modelPath;
            set => _modelPath = value;
        }

        public string PrefabPath
        {
            get => _prefabPath;
            set => _prefabPath = value;
        }

        public void UseAbility()
        {
            this.CurrentCooldown = this.Cooldown;
        }
    }
}
