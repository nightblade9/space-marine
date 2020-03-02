using System;
using System.Collections.Generic;
using System.Linq;

namespace DeenGames.SpaceMarine.Models
{
    class MapEntity
    {
        private static Random random = new Random();

        public int TileX { get; set; }
        public int TileY { get; set; }

        public MapEntity(int x, int y)
        {
            this.TileX = x;
            this.TileY = y;
        }

        // For monsters: figure out how to move toward target
        public Tuple<int, int> Stalk(MapEntity target)
        {
            var dx = target.TileX - this.TileX;
            var dy = target.TileY - this.TileY;
            var potentialMoves = new List<Tuple<int, int>>();

            if (dx != 0)
            {
                potentialMoves.Add(new Tuple<int, int>(Math.Sign(dx), 0));
            }
            if (dy != 0)
            {
                potentialMoves.Add(new Tuple<int, int>(0, Math.Sign(dy)));
            }

            if (potentialMoves.Any())
            {
                return potentialMoves[random.Next(potentialMoves.Count)];
            }
            else
            {
                return new Tuple<int, int>(0, 0);
            }
        }
    }
}