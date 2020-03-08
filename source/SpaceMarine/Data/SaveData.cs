using System.IO;
using Newtonsoft.Json;

namespace DeenGames.SpaceMarine.Data
{
    public class SaveData
    {
        public static SaveData Instance;

        private int currency;

        static SaveData()
        {
            if (!File.Exists("data.dat"))
            {
                var empty = new SaveData();
                empty.Save();
            }

            SaveData.Instance = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText("data.dat"));
        }

        public int Currency
        {
            get {
                return this.currency;
            }
            set
            {
                this.currency = value;
                this.Save();
            }
        }

        private void Save()
        {
            var serialized = JsonConvert.SerializeObject(this);
            File.WriteAllText("data.dat", serialized);
        }
    }
}