using DeenGames.SpaceMarine.Models;
using System;

namespace DeenGames.SpaceMarine.Helpers
{
    static class AlienSpawner
    {
        public static MapEntity Spawn(string name, int x, int y)
        {
            switch (name)
            {
                case "Xarling": return new MapEntity(50, 25, 5, x, y);
                default:
                throw new ArgumentException($"Not sure how to spawn a(n) {name}!");
            }
        }
    }
}