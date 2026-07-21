using Project.Core.Services;
using UnityEngine;

namespace Project.Infrastructure.Settings
{
    public sealed class PlayerPrefsSettingsStorage : ISettingsStorage
    {
        private const string SettingsKey = "DeadEcho.Settings.v1";

        public bool TryLoad(out GameSettingsData data)
        {
            string json = PlayerPrefs.GetString(SettingsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                data = null;
                return false;
            }

            data = JsonUtility.FromJson<GameSettingsData>(json);
            if (data == null || data.Version <= 0)
            {
                data = null;
                return false;
            }

            return true;
        }

        public void Save(GameSettingsData data)
        {
            PlayerPrefs.SetString(SettingsKey, JsonUtility.ToJson(data ?? new GameSettingsData()));
            PlayerPrefs.Save();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(SettingsKey);
            PlayerPrefs.Save();
        }
    }
}
