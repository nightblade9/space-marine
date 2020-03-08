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
    class TitleScene : Puffin.Core.Scene
    {
        override public void Ready()
        {
            var title = new Entity().Label("Space Marine").Move(460, 300);
            title.Get<TextLabelComponent>().FontSize = 72;
            this.Add(title);

            var currency = new Entity().Label("Click to start\nCurrency: 0").Move(600, 400);
            this.Add(currency);

            this.OnMouseClick = () => {
                SpaceMarineGame.Instance.ShowScene(new MapScene());
            };
        }
    }
}