using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GithubDisplay.Services
{
    public static class SettingsService
    {
        static SettingsService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        static ApplicationDataContainer _localSettings;

        const string _backgroundKey = "BACKGROUND_QUERY";

        public static string BackgroundQuery
        {
            get { return GetSetting<string>(_backgroundKey) ?? "Nature"; }
            set { SaveSetting(_backgroundKey, value); }
        }

        static void SaveSetting(string key, object setting)
        {
            _localSettings.Values[key] = setting;
        }

        static T GetSetting<T>(string key)
        {
            if (_localSettings.Values.ContainsKey(key))
            {
                try
                {
                    return (T)_localSettings.Values[key];
                } catch { }
            }

            return default(T);
        }

    }
}
