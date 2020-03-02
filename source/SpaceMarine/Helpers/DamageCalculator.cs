using DeenGames.SpaceMarine.Models;
using System;

namespace DeenGames.SpaceMarine.Helpers
{
    static class DamageCalculator
    {
        public static int CalculateDamage(MapEntity attacker, MapEntity defender)
        {
            var damage = attacker.Strength - defender.Defense;
            return Math.Max(damage, 0);
        }
    }
}