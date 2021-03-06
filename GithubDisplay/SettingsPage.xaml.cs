﻿using GithubDisplay.Annotations;
using GithubDisplay.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
            _LoadLabels();
        }

        private GithubService _githubService = new GithubService();

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

        Dictionary<string, bool> _filterByList = new Dictionary<string, bool>();

        public Dictionary<string, bool> FilterByList
        {
            get { return _filterByList; }
            set
            {
                if (_filterByList != value)
                {
                    _filterByList = value;
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

        public bool IsPersonalStatus
        {
            get => SettingsService.IsPersonalStatus;
            set => SettingsService.IsPersonalStatus = value;
        }

        async Task _LoadLabels()
        {
            var currentFiltered = SettingsService.FilteredLabels;
            var repoLabels = await _githubService.Client.Issue.Labels.GetAllForRepository(_githubService.Repo.Id);

            FilterByList = repoLabels.ToDictionary(label => label.Name, label => currentFiltered.Contains(label.Name));
        }

        void Logout_Click(object sender, RoutedEventArgs e)
        {
            SettingsService.OauthToken = null;
            Frame.Navigate(typeof(MainPage));
        }

        void Back_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Save filtered checkbox state
            var filteredChecks = FilterByList.Where(c => c.Value).Select(c => c.Key).ToList();
            SettingsService.FilteredLabels = filteredChecks;

            Frame.Navigate(typeof(MainPage));
        }

        private void FilterLabel_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)e.OriginalSource;
            var source = (KeyValuePair<string, bool>)checkbox.DataContext;
            FilterByList[source.Key] = checkbox.IsChecked ?? false;
            Debug.WriteLine($"Set value of {source.Key} to {FilterByList[source.Key]}");
        }

        private void DeselectAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilterByList = FilterByList.Select(c => c.Key).ToDictionary(c => c, c => false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
