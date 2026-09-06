using System;
using UnityEngine;

namespace OrbitalDefense
{
    [CreateAssetMenu(menuName = "Orbital Defense/Localization/Table", fileName = "LocalizationTable")]
    public sealed class LocalizationTable : ScriptableObject
    {
        [SerializeField] private GameLanguage defaultLanguage = GameLanguage.Russian;
        [SerializeField] private LocalizationEntry[] entries;

        public GameLanguage DefaultLanguage => defaultLanguage;

        public string Get(string key, GameLanguage language)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (entries == null)
            {
                return key;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                LocalizationEntry entry = entries[i];
                if (entry != null && entry.Key == key)
                {
                    return entry.Get(language);
                }
            }

            return key;
        }
    }

    [Serializable]
    public sealed class LocalizationEntry
    {
        [SerializeField] private string key;
        [SerializeField] private string russian;
        [SerializeField] private string english;

        public string Key => key;

        public string Get(GameLanguage language)
        {
            string value = language == GameLanguage.English ? english : russian;
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            return !string.IsNullOrEmpty(russian) ? russian : key;
        }
    }
}
