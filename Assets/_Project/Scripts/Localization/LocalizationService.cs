using System;
using UnityEngine;

namespace OrbitalDefense
{
    public sealed class LocalizationService : MonoBehaviour
    {
        [SerializeField] private LocalizationTable table;
        [SerializeField] private GameLanguage currentLanguage = GameLanguage.Russian;

        public event Action LanguageChanged;

        public GameLanguage CurrentLanguage => currentLanguage;

        private void Awake()
        {
            if (table != null)
            {
                currentLanguage = table.DefaultLanguage;
            }
        }

        public void SetLanguage(GameLanguage language)
        {
            if (currentLanguage == language)
            {
                return;
            }

            currentLanguage = language;
            LanguageChanged?.Invoke();
        }

        public string Text(string key, params object[] args)
        {
            string template = table != null ? table.Get(key, currentLanguage) : key;
            return Format(template, args);
        }

        public string TextOrFallback(string key, string fallback, params object[] args)
        {
            string template = table != null ? table.Get(key, currentLanguage) : fallback;
            if (template == key && !string.IsNullOrEmpty(fallback))
            {
                template = fallback;
            }

            return Format(template, args);
        }

        private static string Format(string template, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return template;
            }

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }
    }
}
