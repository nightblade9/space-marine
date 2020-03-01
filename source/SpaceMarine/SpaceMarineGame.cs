using Puffin.Infrastructure.MonoGame;
using DeenGames.SpaceMarine.Scenes;

namespace DeenGames.SpaceMarine
{
    public class SpaceMarineGame : PuffinGame
    {
        public SpaceMarineGame() :
            base(Constants.DISPLAY_TILES_WIDE * Constants.TILE_WIDTH * Constants.GAME_ZOOM,
                (Constants.DISPLAY_TILES_HIGH * Constants.TILE_HEIGHT * Constants.GAME_ZOOM) + Constants.STATUS_BAR_HEIGHT)
        {
        }

        override protected void Ready()
        {
            this.ShowScene(new MapScene());
        }
    }
}