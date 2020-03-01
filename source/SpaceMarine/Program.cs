using System;

namespace DeenGames.SpaceMarine
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new SpaceMarineGame())
            {
                game.Run();
            }
        }
    }
}
