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
                case "Xarling":
                    return new MapEntity(name, 50, 25, 5, x, y);
                case "Rayon":
                    return new MapEntity(name, 25, 15, 10, x, y);
                default:
                    throw new ArgumentException($"Not sure how to spawn a(n) {name}!");
            }
        }
    }
}