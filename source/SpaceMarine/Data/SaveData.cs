using System.IO;
using Newtonsoft.Json;

namespace DeenGames.SpaceMarine.Data
{
    public class SaveData
    {
        public static SaveData Instance;

        public int Currency { get; set; }

        static SaveData()
        {
            if (!File.Exists("data.dat"))
            {
                var empty = new SaveData();
                var serialized = JsonConvert.SerializeObject(empty);
                File.WriteAllText("data.dat", serialized);
            }

            SaveData.Instance = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("data.dat"));
        }
    }
}