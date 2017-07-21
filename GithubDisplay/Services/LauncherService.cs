using System;
using System.Threading.Tasks;

namespace GithubDisplay.Services
{
    public static class LauncherService
    {
        public static async Task OpenWebsite(string url)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
