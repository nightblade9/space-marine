using DeenGames.SpaceMarine.Models;
using Puffin.Core;
using Puffin.Core.Ecs;
using Puffin.Core.Ecs.Components;
using Puffin.Core.IO;
using Puffin.Core.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace DeenGames.SpaceMarine.Scenes
{
    class MapScene : Puffin.Core.Scene
    {
        // View
        private TileMap tileMap;
        private TileMap entitiesTileMap;
        private TileMap effectsTileMap;
        private Entity statusLabel;

        // Model
        private PlanetoidMap areaMap;

        // If aiming, becomes the path from us to the alien; if not, empty list.
        private List<GoRogue.Coord> rangeAttackTiles = new List<GoRogue.Coord>();

        override public void Ready()
        {
            // Models
            this.areaMap = new PlanetoidMap(this.EventBus);

            // Views
            this.BackgroundColour = 0x190D14;

            tileMap = new TileMap(
                Constants.MAP_TILES_WIDE, Constants.MAP_TILES_HIGH,
                Path.Combine("Content", "Images", "Tileset.png"),
                Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            
            tileMap.Define("Wall", 0, 0);
            
            for (var y = 0; y < Constants.MAP_TILES_HIGH; y++)
            {
                for (var x = 0; x < Constants.MAP_TILES_WIDE; x++)
                {
                    if (!areaMap[x, y])
                    {
                        tileMap[x, y] = "Wall";
                    }
                }
            }

            this.Add(tileMap);
            
            this.entitiesTileMap = new TileMap(
                Constants.MAP_TILES_WIDE, Constants.MAP_TILES_HIGH,
                Path.Join("Content", "Images", "Characters.png"),
                Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            
            this.entitiesTileMap.Define("Player", 0, 0);
            this.entitiesTileMap.Define("Xarling", 0, 1);
            this.entitiesTileMap.Define("Rayon", 1, 1);

            this.Add(this.entitiesTileMap);
            this.entitiesTileMap[this.areaMap.Player.TileX, this.areaMap.Player.TileY] = "Player";

            this.effectsTileMap = new TileMap(
                Constants.MAP_TILES_WIDE, Constants.MAP_TILES_HIGH,
                Path.Join("Content", "Images", "Effects.png"),
                Constants.TILE_WIDTH, Constants.TILE_HEIGHT);

            this.effectsTileMap.Define("LineOfSight", 0, 0);
            this.effectsTileMap.Define("Plasma", 1, 0);
            
            this.Add(this.effectsTileMap);

            this.statusLabel = new Entity(true);
            this.Add(this.statusLabel);
                this.statusLabel.Move(0, Constants.MAP_TILES_HIGH * Constants.TILE_HEIGHT * Constants.GAME_ZOOM)
                .Colour(0x393238,
                    (int)(Constants.MAP_TILES_WIDE * Constants.TILE_WIDTH * Constants.GAME_ZOOM),
                    Constants.STATUS_BAR_HEIGHT)
                // TODO: Puffin doesn't support label colour :/
                .Label("", 8, 8);

            this.Add(new Entity().Camera(Constants.GAME_ZOOM));

            // Event handlers
            this.EventBus.Subscribe(SpaceMarineEvent.ShowMessage, (message) => this.ShowMessage((string)message));
            this.OnActionPressed = this.ProcessPlayerInput;

            // Trigger initial message. TODO: tutorial here.
            this.ShowMessage("Alien meteors inbound.");
        }

        private void ProcessPlayerInput(object data)
        {
            if (data is PuffinAction)
            {
                var action = (PuffinAction)data;

                if (action == PuffinAction.Up)
                {
                    areaMap.OnPlayerIntendToMove(0, -1);
                    this.rangeAttackTiles.Clear();
                }
                else if (action == PuffinAction.Down)
                {
                    areaMap.OnPlayerIntendToMove(0, 1);
                    this.rangeAttackTiles.Clear();
                }
                else if (action == PuffinAction.Left)
                {
                    areaMap.OnPlayerIntendToMove(-1, 0);
                    this.rangeAttackTiles.Clear();
                }
                else if (action == PuffinAction.Right)
                {
                    areaMap.OnPlayerIntendToMove(1, 0);
                    this.rangeAttackTiles.Clear();
                }

                this.RedrawEverything();
            }
            else if (data is SpaceMarineEvent)
            {
                var marineEvent = (SpaceMarineEvent)data;
                if (marineEvent == SpaceMarineEvent.AimOrFire)
                {
                    if (!rangeAttackTiles.Any())
                    {
                        this.effectsTileMap.Clear();
                        var target = this.areaMap.Aliens.OrderBy(
                            a => GoRogue.Distance.EUCLIDEAN.Calculate(a.TileX, a.TileY, this.areaMap.Player.TileX, this.areaMap.Player.TileY))
                            .FirstOrDefault();
                        
                        if (target != null)
                        {
                            var stop = new GoRogue.Coord(target.TileX, target.TileY);
                            var start = new GoRogue.Coord(this.areaMap.Player.TileX, this.areaMap.Player.TileY);
                            // Calculate line, stop at the first solid tile; order from player => target
                            var line = GoRogue.Lines.Get(start, stop)
                                .Where(s => s != start)
                                .OrderBy(s => GoRogue.Distance.EUCLIDEAN.Calculate(s.X, s.Y, this.areaMap.Player.TileX, this.areaMap.Player.TileY))
                                .ToList();

                            var stopAt = line.FirstOrDefault(s => this.areaMap[s.X, s.Y] == false);
                            if (stopAt.X != 0 && stopAt.Y != 0) // not nullable, we get zero if not found
                            {
                                var stopIndex = line.IndexOf(stopAt);
                                line = line.GetRange(0, stopIndex);
                            }

                            this.rangeAttackTiles = line;
                            
                            // Draw
                            this.RedrawEverything();
                        }
                    }
                    else
                    {
                        var lastTile = this.rangeAttackTiles.Last();
                        var target = this.areaMap.Aliens.SingleOrDefault(a => a.TileX == lastTile.X && a.TileY == lastTile.Y);
                        if (target != null)
                        {
                            // pew pew
                            this.areaMap.PlayerShoots(target);
                        }
                        this.rangeAttackTiles.Clear();
                        this.RedrawEverything();
                    }
                }
            }
        }

        private void RedrawEverything()
        {
            this.entitiesTileMap.Clear();
            this.effectsTileMap.Clear();

            foreach (var plasma in this.areaMap.Plasma)
            {
                this.effectsTileMap[plasma.Item1, plasma.Item2] = "Plasma";
            }

            this.entitiesTileMap[this.areaMap.Player.TileX, this.areaMap.Player.TileY] = "Player";
            
            foreach (var alien in this.areaMap.Aliens.ToArray())
            {
                this.entitiesTileMap[alien.TileX, alien.TileY] = alien.Name;
            }

            foreach (var spot in this.rangeAttackTiles)
            {
                this.effectsTileMap[spot.X, spot.Y] = "LineOfSight";
            }
        }

        private void ShowMessage(string message)
        {
            var player = this.areaMap.Player;
            this.statusLabel.Get<TextLabelComponent>().Text = 
                $"Health: {player.CurrentHealth}/{player.TotalHealth}\tWave {this.areaMap.CurrentWaveNumber}\n{message}";
            Console.WriteLine(message);
        }
    }
}