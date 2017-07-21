using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.UI.Notifications;

namespace GithubDisplay.Services
{
    public static class PushService
    {
        public static void SendPush(string title, string message, string URL = null)
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
        }
    }
}
