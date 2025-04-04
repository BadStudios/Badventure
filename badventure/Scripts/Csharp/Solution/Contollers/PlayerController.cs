using Badventure.Scripts.Csharp.Solution.Models;
using Godot;

namespace Badventure.Scripts.Csharp.Solution.Controllers
{
    public partial class PlayerController : Node
    {
        private PlayerModel _playerModel;

        public PlayerController(PlayerModel playerModel)
        {
            _playerModel = playerModel;
        }

        public void Heal(int amount)
        {
            _playerModel.Heal(amount);
        }

        public void TakeDamage(int amount)
        {
            _playerModel.TakeDamage(amount);
        }

        public void AddExperience(int amount)
        {
            _playerModel.AddExperience(amount);
        }

        public void LevelUp()
        {
            _playerModel.LevelUp();
        }

        public void SetAbility1(AbilityModel ability)
        {
            _playerModel.Ability1 = ability;
        }

        public void SetAbility2(AbilityModel ability)
        {
            _playerModel.Ability2 = ability;
        }

        public void SetUltimate(AbilityModel ultimate)
        {
            _playerModel.Ultimate = ultimate;
        }
    }
}