using UnityEngine;

namespace KeySlaught.Progression
{
    public interface IProgressionStore
    {
        string Load();
        void Save(string json);
        void Delete();
    }

    public sealed class PlayerPrefsProgressionStore : IProgressionStore
    {
        private const string Key = "KeySlaught.Progression.v1";
        public string Load() => PlayerPrefs.GetString(Key, string.Empty);
        public void Save(string json) { PlayerPrefs.SetString(Key, json); PlayerPrefs.Save(); }
        public void Delete() { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
    }
}
