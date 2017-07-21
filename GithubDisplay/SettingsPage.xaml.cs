using GithubDisplay.Annotations;
using GithubDisplay.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GithubDisplay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            SelectedValue = SettingsService.BackgroundQuery;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        public bool IsOnXbox => App.RunningOnXbox;

        List<string> _backgroundItems = new List<string>
        {
            "Nature",
            "Iceland",
            "Construction",
            "Space",
            "Sky",
            "Ocean",
            "Beach",
            "Cat"
        };

        public List<string> BackgroundItems
        {
            get { return _backgroundItems; }
            set
            {
                if (_backgroundItems != value)
                {
                    _backgroundItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedValue
        {
            get { return SettingsService.BackgroundQuery; }
            set { SettingsService.BackgroundQuery = value; }
        }

        public bool NotifyOnProblem
        {
            get => SettingsService.NotifyOnProblem;
            set => SettingsService.NotifyOnProblem = value;
        }

        public bool NotifyOnDone
        {
            get => SettingsService.NotifyOnDone;
            set => SettingsService.NotifyOnDone = value;
        }

        public bool NotifyNewTesting
        {
            get => SettingsService.NotifyNewTesting;
            set => SettingsService.NotifyNewTesting = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void Logout_Click(object sender, RoutedEventArgs e)
        {
            SettingsService.OauthToken = null;
            Frame.Navigate(typeof(MainPage));
        }

        void Back_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
