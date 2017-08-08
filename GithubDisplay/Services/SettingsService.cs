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
        const string _userToken = "USER_TOKEN";
        const string _notificationOnProblem = "NOTIFICATION_ON_PROBLEM";
        const string _notificationOnDone = "NOTIFICATION_ON_DONE";
        const string _notificationNewTesting = "NOTIFICATION_NEW_TESTING";
        const string _isPersonalStatus = "IS_PERSONAL_STATUS";

        public static string BackgroundQuery
        {
            get { return GetSetting(_backgroundKey, "Nature"); }
            set { SaveSetting(_backgroundKey, value); }
        }

        public static string OauthToken
        {
            get { return GetSetting<string>(_userToken); }
            set { SaveSetting(_userToken, value); }
        }

        public static bool NotifyOnProblem
        {
            get { return (GetSetting(_notificationOnProblem, true)); }
            set { SaveSetting(_notificationOnProblem, value); }
        }

        public static bool NotifyOnDone
        {
            get { return (GetSetting(_notificationOnDone, true)); }
            set { SaveSetting(_notificationOnDone, value); }
        }

        public static bool NotifyNewTesting
        {
            get { return (GetSetting(_notificationNewTesting, false)); }
            set { SaveSetting(_notificationNewTesting, value); }
        }

        static bool? _isPersonalStatusValue;

        public static bool IsPersonalStatus
        {
            get
            {
                if (!_isPersonalStatusValue.HasValue)
                {
                    _isPersonalStatusValue = GetSetting(_isPersonalStatus, false);
                }
                return _isPersonalStatusValue.Value;
            }
            set
            {
                if (_isPersonalStatusValue != value)
                {
                    _isPersonalStatusValue = value;
                    SaveSetting(_isPersonalStatus, value);
                }
            }
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
                }
                catch { }
            }

            return default(T);
        }

        static T GetSetting<T>(string key, T fallback)
        {

            if (!_localSettings.Values.ContainsKey(key))
            {
                _localSettings.Values.Add(key, fallback);
                return fallback;
            }

            try
            {
                return (T)_localSettings.Values[key];
            }
            catch
            {
                return fallback;
            }
        }

    }
}
