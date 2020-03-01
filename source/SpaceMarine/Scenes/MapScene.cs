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
        private TileMap tileMap;
        private TileMap entitiesTileMap;
        private MapEntity player;
        private AreaMap areaMap= new AreaMap();

        override public void Ready()
        {
            this.BackgroundColour = 0x190D14;
            this.player = new MapEntity() { 
                TileX = Constants.MAP_TILES_WIDE / 2, TileY = Constants.MAP_TILES_HIGH / 2 };

            tileMap = new TileMap(
                Constants.MAP_TILES_WIDE, Constants.MAP_TILES_HIGH,
                Path.Combine("Content", "Images", "Tileset.png"),
                Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            
            tileMap.Define("Wall", 0, 0);
            
            for (var x = 0; x < Constants.MAP_TILES_WIDE; x++)
            {
                tileMap[x, 0] = "Wall";
                tileMap[x, Constants.MAP_TILES_HIGH - 1] = "Wall";
            }

            for (var y = 0; y < Constants.MAP_TILES_HIGH; y++)
            {
                tileMap[0, y] = "Wall";
                tileMap[Constants.MAP_TILES_WIDE - 1, y] = "Wall";
            }

            this.Add(tileMap);
            
            this.entitiesTileMap = new TileMap(
                Constants.MAP_TILES_WIDE, Constants.MAP_TILES_HIGH,
                Path.Join("Content", "Images", "Characters.png"),
                Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            
            this.entitiesTileMap.Define("Player", 0, 0);

            this.Add(this.entitiesTileMap);
            this.entitiesTileMap[this.player.TileX, this.player.TileY] = "Player";

            this.Add(new Entity().Camera(Constants.GAME_ZOOM));

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