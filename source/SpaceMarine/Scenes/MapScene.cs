using DeenGames.SpaceMarine.Models;
using Puffin.Core;
using Puffin.Core.Ecs;
using Puffin.Core.Ecs.Components;
using Puffin.Core.IO;
using Puffin.Core.Tiles;
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
        private MapEntity player;
        private AreaMap areaMap;

        override public void Ready()
        {
            // Models
            this.areaMap = new AreaMap();

            this.player = new MapEntity() { 
                TileX = Constants.MAP_TILES_WIDE / 2,
                TileY = Constants.MAP_TILES_HIGH / 2
            };

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

            this.Add(this.entitiesTileMap);
            this.entitiesTileMap[this.player.TileX, this.player.TileY] = "Player";

            this.statusLabel = new Entity(true)
                .Move(0, Constants.MAP_TILES_HIGH * Constants.TILE_HEIGHT * Constants.GAME_ZOOM)
                .Colour(0x393238,
                    Constants.MAP_TILES_WIDE * Constants.TILE_WIDTH * Constants.GAME_ZOOM,
                    Constants.STATUS_BAR_HEIGHT)
                // TODO: Puffin doesn't support label colour :/
                .Label("Incoming aliens in: 5 ...\nhi mom", 4, 0);
            this.Add(this.statusLabel);

            this.Add(new Entity().Camera(Constants.GAME_ZOOM));

            // Event handlers

            this.OnActionPressed = this.ProcessPlayerInput;
        }

        private void ProcessPlayerInput(object data)
        {
            if (data is PuffinAction)
            {
                this.entitiesTileMap[this.player.TileX, this.player.TileY] = null;

                var action = (PuffinAction)data;

                // TODO: map mode
                if (action == PuffinAction.Up)
                {
                    areaMap.TryToMove(this.player, 0, -1);
                }
                else if (action == PuffinAction.Down)
                {
                    areaMap.TryToMove(this.player, 0, 1);
                }
                else if (action == PuffinAction.Left)
                {
                    areaMap.TryToMove(this.player, -1, 0);
                }
                else if (action == PuffinAction.Right)
                {
                    areaMap.TryToMove(this.player, 1, 0);
                }

                this.entitiesTileMap[this.player.TileX, this.player.TileY] = "Player";
            }
        }
    }
}