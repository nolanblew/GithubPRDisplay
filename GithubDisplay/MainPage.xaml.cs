using GithubDisplay.Annotations;
using GithubDisplay.Comparers;
using GithubDisplay.Models;
using GithubDisplay.Services;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Security.Authentication.Web;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Application = Windows.UI.Xaml.Application;
using Page = Windows.UI.Xaml.Controls.Page;
using PullRequest = GithubDisplay.Models.PullRequest;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GithubDisplay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        GitHubClient _client;
        Repository _uwpRepository;

        Api _api = new Api();

        Stack<string> _backgroundImages = new Stack<string>();

        Timer _refreshTimer;

        bool _isImage1 = true;

        public MainPage()
        {
            this.InitializeComponent();
            Initialize();
            CoreApplication.MainView.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.S || args.VirtualKey == VirtualKey.GamepadMenu
                || args.VirtualKey == VirtualKey.GamepadA)
            {
                Frame.Navigate(typeof(SettingsPage));
            } else if (args.VirtualKey == VirtualKey.B || args.VirtualKey == VirtualKey.GamepadB)
            {
                if (Frame.CanGoBack) { Frame.GoBack(); }
            } else if (args.VirtualKey == VirtualKey.R)
            {
                _Refresh();
            }
        }

        Tracker<Models.PullRequest> _pullRequests;

        public Tracker<Models.PullRequest> PullRequests
        {
            get { return _pullRequests; }
            set
            {
                if (_pullRequests != value)
                {
                    _pullRequests = value;
                    OnPropertyChanged();
                    if (value != null)
                    {
                        value.CollectionChanged += (_, e) =>
                        {
                            OnPropertyChanged(nameof(PRsCodeReview));
                            OnPropertyChanged(nameof(PRsTesting));
                            OnPropertyChanged(nameof(PRsDone));
                            OnPropertyChanged(nameof(HasHitMaxPRs));
                        };
                    }

                    OnPropertyChanged(nameof(PRsCodeReview));
                    OnPropertyChanged(nameof(PRsTesting));
                    OnPropertyChanged(nameof(PRsDone));
                    OnPropertyChanged(nameof(HasHitMaxPRs));
                }
            }
        }

        public IList<Models.PullRequest> PRsCodeReview => PullRequests?.Entities.Where(
            pr => pr.Entity.State == PRState.CodeReview).Select(pr => pr.Entity).ToList();

        public IList<Models.PullRequest> PRsTesting => PullRequests?.Entities.Where(
            pr => pr.Entity.State == PRState.Testing).Select(pr => pr.Entity).ToList();

        public IList<Models.PullRequest> PRsDone => PullRequests?.Entities.Where(
            pr => pr.Entity.State == PRState.Done).Select(pr => pr.Entity).ToList();

        public int MaxOpenPRs => 7;

        public bool HasHitMaxPRs => (PRsCodeReview?.Count ?? 0) >= MaxOpenPRs;

        public bool IsOnXbox => App.RunningOnXbox;

        bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        Octokit.User _currentUser;

        public Octokit.User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

        async void Initialize()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsBusy = true);

            PullRequests = new Tracker<Models.PullRequest>(PullRequestComparer.Create());
            PullRequests.AllowedTrackedProperties = new[] { nameof(Models.PullRequest.CodeReviewStatus),
                                                            nameof(Models.PullRequest.ErrorStatus),
                                                            nameof(Models.PullRequest.State) };
            PullRequests.TrackedPropertyChanged += PullRequests_TrackedPropertyChanged;

            try
            {
                await Login();
                _refreshTimer = new Timer((_) => _Refresh(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            }
            finally
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsBusy = false);
            }
        }

        async Task _Refresh()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsBusy = true);
            Debug.WriteLine($"Refresh at {DateTime.Now.TimeOfDay.ToString()}");
            try
            {
                var prs = await _GetPullRequestsAsync();
                await _FilterListsAsync(prs, true);
            }
            finally
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                    var btmap = new BitmapImage(new Uri(await _GetNextBackgroundImageAsync()));

                    if (IsOnXbox)
                    {
                        if (_isImage1)
                            bckImage1.Source = btmap;
                        else
                            bckImage2.Source = btmap;

                        _isImage1 = !_isImage1;
                    }
                    else
                    {
                        bckImage1.Source = btmap;
                    }

                    IsBusy = false;
                });
            }
        }

        async Task Login()
        {
            try
            {
                _client = new GitHubClient(new ProductHeaderValue(GithubDisplay.Resources.ClientName));

                if (IsOnXbox)
                {
                    _client.Credentials = new Credentials(GithubDisplay.Resources.GithubBypassToken);
                }
                else
                {
                    var startUrl =
                        _client.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(GithubDisplay.Resources.ClientId));
                    var redirectUrl = new Uri(GithubDisplay.Resources.ClientRedirectUrl);

                    var accessToken = SettingsService.OauthToken;
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        var result = await WebAuthenticationBroker.AuthenticateAsync(
                            WebAuthenticationOptions.None,
                            startUrl,
                            redirectUrl);

                        if (result.ResponseStatus != WebAuthenticationStatus.Success)
                        {
                            await new MessageDialog(
                                    "Error! Username and password do not match. Please try again.",
                                    "Error")
                                .ShowAsync();
                            Login();
                            return;
                        }

                        // Ask for auth token
                        var token = await _client.Oauth.CreateAccessToken(
                            new OauthTokenRequest(
                                GithubDisplay.Resources.ClientId,
                                GithubDisplay.Resources.ClientSecret,
                                result.ResponseData.Substring(
                                    result.ResponseData.LastIndexOf("code=") + 5)));

                        accessToken = token.AccessToken;
                        SettingsService.OauthToken = accessToken;
                    }

                    _client.Credentials = new Credentials(accessToken);
                }

                CurrentUser = await _client.User.Current();

                _uwpRepository = await _client.Repository.Get("procore", "uwp");
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Error: {ex.Message}", "Error")
                    .ShowAsync();

                Application.Current.Exit();
            }
        }

        async Task<string> _GetNextBackgroundImageAsync()
        {
            if (IsOnXbox)
            {
                if (_backgroundImages.Any())
                {
                    return _backgroundImages.Pop();
                }
                try
                {
                    var response = await Api.GetResponseAsync<List<Models.RandomPhotoResponse>>(
                        $"photos/random?query={SettingsService.BackgroundQuery.ToLower()}&count=30");

                    var newList = response.Select(p => p.urls.full).ToList();

                    _backgroundImages = new Stack<string>(newList);
                    return _backgroundImages.Pop();
                }
                catch (Exception ex)
                {
                    if (Debugger.IsAttached)
                    {
                        throw ex;
                    }

                    return string.Empty;
                }
            }
            else
            {
                return "ms-appx:///Assets/c_background.jpg";
            }
        }

        async Task<IList<Issue>> _GetPullRequestsAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsBusy = true);
            try
            {
                var issues = (await _client.Issue.GetAllForRepository(
                        _uwpRepository.Id,
                        new RepositoryIssueRequest {State = ItemStateFilter.Open}))
                    .Where(i => i.PullRequest != null);

                return issues.ToList();
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) { throw ex; }
                return null;
            }
            finally
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsBusy = false);
            }
        }

        async Task _FilterListsAsync(IList<Issue> issues, bool forceRefresh = false)
        {
            if (PullRequests != null && !forceRefresh)
            {
                issues = issues.Where(i => PullRequests.Entities.Any(pr => pr.Entity.Number == i.PullRequest.Number)).ToList();
            }

            var prsTask = issues.Select(
                async i =>
                {
                    var number = int.Parse(i.PullRequest.Url.Substring(i.PullRequest.Url.LastIndexOf("/") + 1));
                    var pr = await _client.PullRequest.Get(_uwpRepository.Id, number);
                    var reviews = await _client.PullRequest.Reviews.GetAll("procore", "uwp", pr.Number);
                    return new Models.PullRequest(i, pr, reviews.ToList());
                }).ToList();

            var prs = await Task.WhenAll(prsTask);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    //PullRequests = new ObservableCollection<Models.PullRequest>(prs)
                    PullRequests.Update(prs);
                });
        }
        
        void PullRequests_TrackedPropertyChanged(object sender, TrackedPropertyChangedEvent<Models.PullRequest> e)
        {
            var isMe = e.Entity.AssigneeName != CurrentUser.Login;
            
            if (e.PropertyName == "State")
            {
                if (e.Entity.State == PRState.Done
                    && e.Entity.TestingState == Models.PullRequest.LabelState.None
                    && string.IsNullOrEmpty(e.Entity.ErrorStatus)
                    && SettingsService.NotifyOnDone
                    && isMe)
                {
                    PushService.SendPush("Ready to Merge", $"Your PR {e.Entity.Name} is clear and ready to merge", e.Entity.PrUrl);
                }
                else if
                (e.Entity.State == PRState.Testing
                    && e.Entity.TestingState == PullRequest.LabelState.Needed
                    && SettingsService.NotifyNewTesting)
                {
                    PushService.SendPush("New PR Needs Testing", $"PR # {e.Entity.Number} ({e.Entity.Name}) is ready for testing.", e.Entity.PrUrl);
                }
            }

            if (!isMe) { return; }

            if (e.PropertyName == "TestingState" && SettingsService.NotifyOnDone) {
                switch (e.Entity.TestingState)
                {
                    case Models.PullRequest.LabelState.Failed:
                        PushService.SendPush("Testing Failed", $"Testing failed on {e.Entity.Name}", e.Entity.PrUrl);
                        break;
                    case Models.PullRequest.LabelState.Passed:
                        string followup = "and is ready to be merged!";
                        if (e.Entity.IsBlocked) followup = "but is blocked.";
                        if (e.Entity.Mergable) followup = "but has merge conflicts.";
                        if (e.Entity.HasChangeRequests) followup = "but has changes requested.";
                        if (e.Entity.UXReviewState == Models.PullRequest.LabelState.Failed ||
                            e.Entity.UXReviewState == Models.PullRequest.LabelState.Needed)
                            followup = "but still needs to pass UX review.";

                        PushService.SendPush("Testing Passed", $"Testing passed on {e.Entity.Name} {followup}", e.Entity.PrUrl);
                        break;
                }
            }

            if (e.PropertyName == "ErrorStatus" && SettingsService.NotifyOnProblem)
            {
                if (!string.IsNullOrWhiteSpace(e.Entity.ErrorStatus))
                {
                    PushService.SendPush($"Problem with PR", $"Your PR {e.Entity.Name} {e.Entity.ErrorStatus}", e.Entity.PrUrl);
                } else if (e.Entity.State == PRState.Done)
                {
                    PushService.SendPush("Ready to Merge", $"Your PR {e.Entity.Name} is clear and ready to merge", e.Entity.PrUrl);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var requests = await _GetPullRequestsAsync();
            await _FilterListsAsync(requests, true);
        }

        void BckImage1_OnImageOpened(object sender, RoutedEventArgs e)
        {
            ShowImage1.Begin();
        }

        void BckImage2_OnImageOpened(object sender, RoutedEventArgs e)
        {
            ShowImage2.Begin();
        }

        void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        async void PullRequest_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Models.PullRequest pr)
            {
                await LauncherService.OpenWebsite(pr.PrUrl);
            }
        }
    }
}
