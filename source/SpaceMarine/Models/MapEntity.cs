
namespace DeenGames.SpaceMarine.Models
{
    class MapEntity
    {
        public int TileX { get; set; }
        public int TileY { get; set; }

        public MapEntity(int x, int y)
        {
            this.TileX = x;
            this.TileY = y;
        }
    }
}