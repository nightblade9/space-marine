using DeenGames.SpaceMarine.Models;
using Puffin.Core;
using Puffin.Core.Ecs;
using Puffin.Core.Ecs.Components;
using Puffin.Core.IO;
using Puffin.Core.Tiles;
using System;
using System.IO;

namespace DeenGames.SpaceMarine.Scenes
{
    class MapScene : Puffin.Core.Scene
    {
        // View
        private TileMap tileMap;
        private TileMap entitiesTileMap;
        private Entity statusLabel;

        // Model
        private PlanetoidMap areaMap;

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

            this.Add(this.entitiesTileMap);
            this.entitiesTileMap[this.areaMap.Player.TileX, this.areaMap.Player.TileY] = "Player";

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
                this.entitiesTileMap.Clear();

                var action = (PuffinAction)data;

                if (action == PuffinAction.Up)
                {
                    areaMap.OnPlayerIntendToMove(0, -1);
                }
                else if (action == PuffinAction.Down)
                {
                    areaMap.OnPlayerIntendToMove(0, 1);
                }
                else if (action == PuffinAction.Left)
                {
                    areaMap.OnPlayerIntendToMove(-1, 0);
                }
                else if (action == PuffinAction.Right)
                {
                    areaMap.OnPlayerIntendToMove(1, 0);
                }

                this.entitiesTileMap[this.areaMap.Player.TileX, this.areaMap.Player.TileY] = "Player";
                foreach (var alien in this.areaMap.Aliens.ToArray())
                {
                    (int dx, int dy) = alien.Stalk(this.areaMap.Player);
                    areaMap.TryToMove(alien, dx, dy);
                    this.entitiesTileMap[alien.TileX, alien.TileY] = "Xarling";
                }
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