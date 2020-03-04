using DeenGames.SpaceMarine.Helpers;
using Puffin.Core.Events;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeenGames.SpaceMarine.Models
{
    // Map of a planetoid
    class PlanetoidMap
    {
        internal List<MapEntity> Aliens = new List<MapEntity>();
        internal readonly MapEntity Player;
        internal int CurrentWaveNumber = 0; // floor number

        private const float ALIEN_TILE_SPAWN_PROBABILITY = 0.5f;
        private const int ALIEN_TILE_SPAWN_RADIUS = 1;
        private const int PLAYER_STARTING_HEALTH = 250;
        private const int PLAYER_STRENGTH = 20;
        private const int PLAYER_DEFENSE = 10;
        private const int NUM_CLUSTERS = 6;
        private const int MIN_CLUSTER_SIZE = 5;
        private const int MAX_CLUSTER_SIZE = 8;

        private readonly ArrayMap<bool> isWalkable;
        private readonly Random random = new Random();
        // In TILES
        private readonly int width;
        private readonly int height;
        private const int CountDownMoves = 10;
        private int countDownLeft = 0;
        private EventBus eventBus;
        private bool gameOver = false;
        
        public PlanetoidMap(EventBus eventBus)
        {
            this.eventBus = eventBus;
            this.width = Constants.MAP_TILES_WIDE;
            this.height = Constants.MAP_TILES_HIGH;
            this.isWalkable = new ArrayMap<bool>(Constants.MAP_TILES_WIDE, Constants.MAP_TILES_HIGH);
            this.Player = new MapEntity(PLAYER_STARTING_HEALTH, PLAYER_STRENGTH, PLAYER_DEFENSE, this.width / 2, this.height / 2);

            this.GenerateMap();
            this.GenerateAliens();
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
            if (gameOver)
            {
                return;
            }

            this.TryToMove(this.Player, deltaX, deltaY);
            this.UpdateCountDown();
        }

        public void TryToMove(MapEntity entity, int deltaX, int deltaY)
        {
            if (gameOver)
            {
                return;
            }

            var destinationX = entity.TileX + deltaX;
            var destinationY = entity.TileY + deltaY;

            if (destinationX < 0 || destinationX >= this.width || destinationY < 0 || destinationY >= this.height ||
                !this.isWalkable[destinationX, destinationY])
            {
                return;
            }
            else if (this.Aliens.Any(m => m.TileX == destinationX && m.TileY == destinationY))
            {
                var target = this.Aliens.Single(m => m.TileX == destinationX && m.TileY == destinationY);
                var damage = DamageCalculator.CalculateDamage(entity, target);
                target.CurrentHealth -= damage;
                this.eventBus.Broadcast(SpaceMarineEvent.ShowMessage, $"Alien hits alien for {damage} damage! {(target.CurrentHealth <= 0 ? "It dies!" : "")}");

                if (target.CurrentHealth <= 0)
                {
                    this.Aliens.Remove(target);
                }
            }
            else if (this.Player.TileX == destinationX && this.Player.TileY == destinationY)
            {
                var damage = DamageCalculator.CalculateDamage(entity, this.Player);
                this.Player.CurrentHealth -= damage;
                this.eventBus.Broadcast(SpaceMarineEvent.ShowMessage, $"Alien hits you for {damage} damage! {(this.Player.CurrentHealth <= 0 ? "YOU DIE!" : "")}");

                if (this.Player.CurrentHealth <= 0)
                {
                    this.gameOver = true;
                }
            }
            else
            {
                // It's clear, so move.
                entity.TileX = destinationX;
                entity.TileY = destinationY;
            }
        }
        
        public void GenerateAliens()
        {
            // TODO: more sophisticated.
            var numAliens = random.Next(6, 10);
        }

        private void IncrementWave()
        {
            this.countDownLeft = CountDownMoves;
            this.CurrentWaveNumber++;
        }

        private void UpdateCountDown()
        {
            if (this.countDownLeft > 0)
            {
                this.countDownLeft--;
                if (this.countDownLeft > 0)
                {
                    this.eventBus.Broadcast(SpaceMarineEvent.ShowMessage, $"{this.countDownLeft} seconds until impact.");
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
            var cornerOffset = ALIEN_TILE_SPAWN_RADIUS + 2; // spacing from walls
            var corners = new List<Tuple<int, int>> {
                new Tuple<int, int>(cornerOffset, cornerOffset),
                new Tuple<int, int>(Constants.MAP_TILES_WIDE - cornerOffset, cornerOffset),
                new Tuple<int, int>(Constants.MAP_TILES_WIDE - cornerOffset, Constants.MAP_TILES_HIGH - cornerOffset),
                new Tuple<int, int>(cornerOffset, Constants.MAP_TILES_HIGH - cornerOffset),
            };

            var spawnPoints = corners.OrderBy(r => random.Next()).Take(2);
            foreach (var spawnPoint in spawnPoints)
            {
                // Spawn stuff in a random 3x3 radius. Each point has an independent chance of spawning an alien.
                for (var y = spawnPoint.Item2 - ALIEN_TILE_SPAWN_RADIUS; y <= spawnPoint.Item2 + ALIEN_TILE_SPAWN_RADIUS; y++)
                {
                    for (var x = spawnPoint.Item1 - ALIEN_TILE_SPAWN_RADIUS; x <= spawnPoint.Item1 + ALIEN_TILE_SPAWN_RADIUS; x++)
                    {
                        if (random.NextDouble() <= ALIEN_TILE_SPAWN_PROBABILITY)
                        {
                            this.Aliens.Add(AlienSpawner.Spawn("Xarling", x, y));
                        }
                    }
                }
            }
        }

        private void GenerateMap()
        {
            QuickGenerators.GenerateRectangleMap(isWalkable);
            var clustersLeft = NUM_CLUSTERS;

            while (clustersLeft-- > 0)
            {
                var x = random.Next(this.width);
                var y = random.Next(this.height);
                var numTiles = random.Next(MIN_CLUSTER_SIZE, MAX_CLUSTER_SIZE + 1);
                this.RandomWalk(x, y, numTiles, false);
            }
        }

        private void RandomWalk(int startX, int startY, int numTiles, bool makeWalkable)
        {
            var tries = 0;
            var numLeft = numTiles;
            var x = startX;
            var y = startY;

            while (numLeft > 0 && tries++ < 1000)
            {
                if (x < 0) {
                    x = 0;
                } else if (x >= width)
                {
                    x = width - 1;
                }
                if (y < 0) {
                    y = 0;
                } else if (y >= height)
                {
                    y = height - 1;
                }

                if (this.isWalkable[x, y] != makeWalkable)
                {
                    this.isWalkable[x, y] = makeWalkable;
                    numLeft--;
                }

                var isX = random.NextDouble() < 0.5;
                var delta = random.Next(-1, 2);
                if (isX) {
                    x += delta;
                } else {
                    y += delta;
                }
            }
        }
    }
}