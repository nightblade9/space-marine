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
        private Player player;

        override public void Ready()
        {
            this.BackgroundColour = 0x190D14;
            this.player = new Player() { 
                X = Constants.MAP_TILES_WIDE / 2, Y = Constants.MAP_TILES_HIGH / 2 };

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
            this.entitiesTileMap[this.player.X, this.player.Y] = "Player";

            this.Add(new Entity().Camera(Constants.GAME_ZOOM));

            this.OnActionPressed = this.ProcessPlayerInput;
        }

        private void ProcessPlayerInput(object data)
        {
            if (data is PuffinAction)
            {
                this.entitiesTileMap[this.player.X, this.player.Y] = null;

                var action = (PuffinAction)data;
                var moved = false;

                // TODO: map mode
                if (action == PuffinAction.Up && IsClear(this.player.X, this.player.Y - 1))
                {
                    this.player.Y -= 1;
                    moved = true;
                }
                else if (action == PuffinAction.Down && IsClear(this.player.X, this.player.Y + 1))
                {
                    this.player.Y += 1;
                    moved = true;
                }
                else if (action == PuffinAction.Left && IsClear(this.player.X - 1, this.player.Y))
                {
                    this.player.X -= 1;
                    moved = true;
                }
                else if (action == PuffinAction.Right && IsClear(this.player.X + 1, this.player.Y))
                {
                    this.player.X += 1;
                    moved = true;
                }

                this.entitiesTileMap[this.player.X, this.player.Y] = "Player";
            }
        }

        private bool IsClear(int x, int y)
        {
            return this.tileMap[x, y] == null && this.entitiesTileMap[x, y] == null;
        }
    }
}