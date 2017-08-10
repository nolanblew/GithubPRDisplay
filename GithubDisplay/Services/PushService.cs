using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.UI.Notifications;

namespace GithubDisplay.Services
{
    public static class PushService
    {
        static bool? _appHasBadge;

        public static void SendPush(string title, string message, string URL = null, bool showBadge = true)
        {
            if (App.RunningOnXbox) { return; }

            ToastContent content = new ToastContent()
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                                HintMaxLines = 1
                            },

                            new AdaptiveText()
                            {
                                Text = message
                            }
                        }
                    }
                }
            };
            if (!string.IsNullOrWhiteSpace(URL)) { content.Launch = $"pr:{URL}"; }

            var toast = new ToastNotification(content.GetXml());
            toast.ExpirationTime = DateTime.Now.AddDays(1);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            if (showBadge)
                SetBadgeNotification();
        }

        public static void SetBadgeNotification()
        {
            var glyphContent = new BadgeGlyphContent(BadgeGlyphValue.Alert);
            var badgeNotification = new BadgeNotification(glyphContent.GetXml());

            var badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            badgeUpdater.Update(badgeNotification);
            _appHasBadge = true;
        }

        public static void ClearBadgeNotification()
        {
            if (!_appHasBadge.HasValue)
            {
                _appHasBadge = true;
            }

            if (_appHasBadge.Value)
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
                _appHasBadge = false;
            }
        }
    }
}
