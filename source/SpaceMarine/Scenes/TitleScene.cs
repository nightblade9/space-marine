using DeenGames.SpaceMarine.Data;
using DeenGames.SpaceMarine.Models;
using Newtonsoft.Json;
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
            if (!File.Exists("data.dat"))
            {
                var empty = new SaveData();
                var serialized = JsonConvert.SerializeObject(empty);
                File.WriteAllText("data.dat", serialized);
            }

            var data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("data.dat"));

            var title = new Entity().Label("Space Marine").Move(460, 300);
            title.Get<TextLabelComponent>().FontSize = 72;
            this.Add(title);

            var currency = new Entity().Label($"Click to start\nCurrency: {data.Currency}").Move(600, 400);
            this.Add(currency);

            this.OnMouseClick = () => {
                SpaceMarineGame.Instance.ShowScene(new MapScene());
            };
        }
    }
}