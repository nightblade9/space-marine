using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using Puffin.Core.Events;
using Troschuetz.Random;
using Troschuetz.Random.Generators;

namespace DeenGames.SpaceMarine.Models
{
    // Map of a planetoid
    class PlanetoidMap
    {
        internal List<MapEntity> Monsters = new List<MapEntity>();
        internal readonly MapEntity Player;

        private const float MONSTER_TILE_SPAWN_PROBABILITY = 0.5f;
        private const int MONSTER_TILE_SPAWN_RADIUS = 1;
        private readonly ArrayMap<bool> isWalkable;
        // In TILES
        private readonly int width;
        private readonly int height;
        private const int CountDownMoves = 10;
        private int currentWaveNumber = 0; // floor number
        private int countDownLeft = 0;
        private EventBus eventBus;
        
        public PlanetoidMap(EventBus eventBus)
        {
            this.eventBus = eventBus;
            this.width = Constants.MAP_TILES_WIDE;
            this.height = Constants.MAP_TILES_HIGH;
            this.isWalkable = new ArrayMap<bool>(Constants.MAP_TILES_WIDE, Constants.MAP_TILES_HIGH);
            this.Player = new MapEntity(this.width / 2, this.height / 2);

            this.GenerateMap();
            this.GenerateMonsters();
            this.IncrementWave();
        }

        public bool this[int x, int y]
        {
            get {
                return this.isWalkable[x, y];
            }
        }

        public void OnPlayerIntendToMove(int deltaX, int deltaY)
        {
            this.TryToMove(this.Player, deltaX, deltaY);
            this.UpdateCountDown();
        }

        public void TryToMove(MapEntity entity, int deltaX, int deltaY)
        {
            var destinationX = entity.TileX + deltaX;
            var destinationY = entity.TileY + deltaY;

            if (destinationX < 0 || destinationX >= this.width || destinationY < 0 || destinationY >= this.height ||
                !this.isWalkable[destinationX, destinationY])
            {
                return;
            }
            else if (this.Monsters.Any(m => m.TileX == destinationX && m.TileY == destinationY))
            {
                var target = this.Monsters.Single(m => m.TileX == destinationX && m.TileY == destinationY);
                // damage target
            }
            else if (this.Player.TileX == destinationX && this.Player.TileY == destinationY)
            {
                // damage player
            }
            else
            {
                // It's clear, so move.
                entity.TileX = destinationX;
                entity.TileY = destinationY;
            }
        }
        
        public void GenerateMonsters()
        {
            var random = new Random();
            // TODO: more sophisticated.
            var numMonsters = random.Next(6, 10);
        }

        private void IncrementWave()
        {
            this.countDownLeft = CountDownMoves;
            this.currentWaveNumber++;
        }

        private void UpdateCountDown()
        {
            if (this.countDownLeft > 0)
            {
                this.countDownLeft--;
                if (this.countDownLeft > 0)
                {
                    this.eventBus.Broadcast(SpaceMarineEvent.ShowMessage, $"{this.countDownLeft} seconds until the next wave ...");
                }
                else
                {
                    this.eventBus.Broadcast(SpaceMarineEvent.ShowMessage, "Alien life-forms detected.");
                    this.SpawnMoreOverlords();
                }
            }
        }

        private void SpawnMoreOverlords()
        {
            var random = new Random();
            var cornerOffset = MONSTER_TILE_SPAWN_RADIUS + 2; // spacing from walls
            var corners = new List<Tuple<int, int>> {
                new Tuple<int, int>(cornerOffset, cornerOffset),
                new Tuple<int, int>(Constants.MAP_TILES_WIDE - cornerOffset, cornerOffset),
                new Tuple<int, int>(Constants.MAP_TILES_WIDE - cornerOffset, Constants.MAP_TILES_HIGH - cornerOffset),
                new Tuple<int, int>(cornerOffset, Constants.MAP_TILES_HIGH - cornerOffset),
            };

            var spawnPoints = corners.OrderBy(r => random.Next()).Take(2);
            foreach (var spawnPoint in spawnPoints)
            {
                // Spawn stuff in a random 3x3 radius. Each point has an independent chance of spawning a monster.
                for (var y = spawnPoint.Item2 - MONSTER_TILE_SPAWN_RADIUS; y <= spawnPoint.Item2 + MONSTER_TILE_SPAWN_RADIUS; y++)
                {
                    for (var x = spawnPoint.Item1 - MONSTER_TILE_SPAWN_RADIUS; x <= spawnPoint.Item1 + MONSTER_TILE_SPAWN_RADIUS; x++)
                    {
                        if (random.NextDouble() <= MONSTER_TILE_SPAWN_PROBABILITY)
                        {
                            this.Monsters.Add(new MapEntity(x, y));
                        }
                    }
                }
            }
        }

        private void GenerateMap()
        {
            QuickGenerators.GenerateRectangleMap(isWalkable);
        }
    }
}