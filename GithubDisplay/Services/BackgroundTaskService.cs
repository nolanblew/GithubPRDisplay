using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace GithubDisplay.Services
{
    public static class BackgroundTaskService
    {
        static string id;

        public static Task TaskToRun { get; private set; }

        static BackgroundTaskRegistration _backgroundTask;

        public static void StartTask(Task taskToRun)
        {
            TaskToRun = taskToRun;

            var builder = new BackgroundTaskBuilder();
            builder.Name = "Github PR Background Task";
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            _backgroundTask = builder.Register();
        }

        public static void EndTask()
        {
            _backgroundTask?.Unregister(true);
        }
    }
}
