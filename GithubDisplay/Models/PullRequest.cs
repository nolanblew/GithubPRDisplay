using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using GithubDisplay.Annotations;
using GithubDisplay.Services;
using GithubDisplay.ViewModels;
using Octokit;

namespace GithubDisplay.Models
{
    public class PullRequest : INotifyPropertyChanged
    {
        public PullRequest() { }

        public PullRequest(Octokit.PullRequest pr)
        {
            Name = pr.Title;
            Number = pr.Number;
            Mergable = pr.Mergeable ?? true;
            AssigneeName = pr.Assignee?.Login ?? "[no assignee]";
            PrUrl = pr.HtmlUrl;
        }

        public PullRequest(Octokit.Issue issue, Octokit.PullRequest pullRequest)
            : this(pullRequest)
        {

            var labels = issue.Labels.Select(l => l.Name).ToList();
            IsReviewed = labels.Contains("Passed Review");
            IsBlocked = labels.Contains("Blocked");
            IsReadyForReview = labels.Contains("Ready for review");
            IsDoNotMerge = labels.Contains("Do Not Merge");
            if (labels.Contains("Passed Testing"))
            {
                TestingState = LabelState.Passed;
            }
            else if (labels.Contains("Didn't Pass Testing"))
            {
                TestingState = LabelState.Failed;
            }
            else if(labels.Contains("Needs testing"))
            {
                TestingState = LabelState.Needed;
            }
            else
            {
                TestingState = LabelState.None;
            }

            if (labels.Contains("Needs UX Review"))
            {
                UXReviewState = LabelState.Needed;
            }
            else if (labels.Contains("Passed UX Review"))
            {
                UXReviewState = LabelState.Passed;
            }
            else
            {
                UXReviewState = LabelState.None;
            }
        }

        public PullRequest(
            Octokit.Issue issue,
            Octokit.PullRequest pullRequest,
            IList<PullRequestReview> reviews)
            : this(issue, pullRequest)
        {
            Reviews = new ObservableCollection<PullRequestReview>(reviews);

            // TODO: Add logic to get PR state when api becomes available
            if (reviews.Any(
                r => r.State == PullRequestReview.PullRequestReviewStates.ChangesRequested
                    && reviews.Last(or => or.User.Login == r.User.Login).State
                    == PullRequestReview.PullRequestReviewStates.ChangesRequested))
            {
                HasChangeRequests = true;
            }
            else
            {
                var approved = reviews.Where(r => r.State == PullRequestReview.PullRequestReviewStates.Approved)
                                      .Select(r => r.User.Login);
                NumberOfApproved = approved.Distinct().Count();
            }

            DesiredNumberOfApproved = _GetNumberOfDesiredApprovers(pullRequest.Body);
        }

        string _name;

        int _number;

        string _assigneeName;

        string _prUrl;

        bool _isReadyForReview;

        bool _isReviewed;

        bool _isBlocked;

        bool _isDoNotMerge;

        LabelState _testingState;

        LabelState _uxReviewState;

        ObservableCollection<PullRequestReview> _reviews = new ObservableCollection<PullRequestReview>();

        bool _mergable;

        bool _hasChangeRequests;

        int _numberOfApproved;

        int _desiredNumberOfApproved = 3;

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public int Number
        {
            get => _number;
            set
            {
                if (value == _number) return;
                _number = value;
                OnPropertyChanged();
            }
        }

        public string AssigneeName
        {
            get => _assigneeName;
            set
            {
                if (value == _assigneeName) return;
                _assigneeName = value;
                OnPropertyChanged();
            }
        }

        public bool IsDoNotMerge
        {
            get => _isDoNotMerge;
            set
            {
                if (value == _isDoNotMerge) return;
                _isDoNotMerge = value;
                OnPropertyChanged();
            }
        }

        public string PrUrl
        {
            get => _prUrl;
            set
            {
                if (value == _prUrl) return;
                _prUrl = value;
                OnPropertyChanged();
            }
        }

        public bool IsReadyForReview
        {
            get => _isReadyForReview;
            set
            {
                if (value == _isReadyForReview) return;
                _isReadyForReview = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public bool IsReviewed
        {
            get => _isReviewed;
            set
            {
                if (value == _isReviewed) return;
                _isReviewed = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public bool IsBlocked
        {
            get => _isBlocked;
            set
            {
                if (value == _isBlocked) return;
                _isBlocked = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public LabelState TestingState
        {
            get => _testingState;
            set
            {
                if (value == _testingState) return;
                _testingState = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public LabelState UXReviewState
        {
            get => _uxReviewState;
            set
            {
                if (value == _uxReviewState) return;
                _uxReviewState = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public ObservableCollection<PullRequestReview> Reviews
        {
            get => _reviews;
            set
            {
                if (Equals(value, _reviews)) return;
                if (_reviews != null) _reviews.CollectionChanged -= ReviewsOnCollectionChanged;
                _reviews = value;
                _reviews.CollectionChanged += ReviewsOnCollectionChanged;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        void ReviewsOnCollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(nameof(PersonalStatus));
        }

        public bool Mergable
        {
            get => _mergable;
            set
            {
                if (value == _mergable) return;
                _mergable = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public bool HasChangeRequests
        {
            get => _hasChangeRequests;
            set
            {
                if (value == _hasChangeRequests) return;
                _hasChangeRequests = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public int NumberOfApproved
        {
            get => _numberOfApproved;
            set
            {
                if (value == _numberOfApproved) return;
                _numberOfApproved = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public int DesiredNumberOfApproved
        {
            get => _desiredNumberOfApproved;
            set
            {
                if (value == _desiredNumberOfApproved) return;
                _desiredNumberOfApproved = value;
                OnPropertyChanged();
                _PropertyChanges();
            }
        }

        public bool IsFailedTesting => TestingState == LabelState.Failed;

        public SolidColorBrush StatusBrush
        {
            get
            {
                if (SettingsService.IsPersonalStatus && AssigneeName != GithubDisplay.Resources.User.Login)
                {
                    return ReviewItem.StatusBrush as SolidColorBrush;
                }

                if (!string.IsNullOrEmpty(ErrorStatus))
                {
                    return new SolidColorBrush(Colors.DarkRed);
                }
                else if ((IsBlocked
                    || DesiredNumberOfApproved > NumberOfApproved
                    || UXReviewState == LabelState.Needed) && State == PRState.CodeReview)
                {
                    return new SolidColorBrush(Colors.DarkGoldenrod);
                }
                else
                {
                    return new SolidColorBrush(Colors.Green);
                }
            }
        }

        public string ErrorStatus
        {
            get
            {
                if (TestingState == LabelState.Failed)
                {
                    return "Failed Testing";
                }

                if (UXReviewState == LabelState.Failed)
                {
                    return "Failed UX Review";
                }

                if (HasChangeRequests)
                {
                    return "Changes Requested";
                }

                if (!Mergable)
                {
                    return "Merge Conflicts";
                }

                if (IsBlocked && State == PRState.Done)
                {
                    return "Blocked";
                }

                if (IsDoNotMerge && State == PRState.Done)
                {
                    return "Do Not Merge";
                }

                return string.Empty;
            }
        }

        public string PersonalStatus
        {
            get
            {
                var lastReview = Reviews.LastOrDefault(r => r.User.Login == Resources.User.Login && r.State != PullRequestReview.PullRequestReviewStates.Commented);

                if (lastReview == null) return "Needs Review";
                if (lastReview.State == PullRequestReview.PullRequestReviewStates.Approved) return "Approved";
                if (lastReview.State == PullRequestReview.PullRequestReviewStates.ChangesRequested)
                {
                    // TODO: Find out if changes are in need of review again
                    return "You Requested Changes";
                }

                return "Needs Review";
            }
        }

        public string CodeReviewStatus
        {
            get
            {
                if (State == PRState.CodeReview && string.IsNullOrEmpty(ErrorStatus))
                {
                    if (NumberOfApproved >= DesiredNumberOfApproved && UXReviewState == LabelState.Needed)
                    {
                        return "UX Review Needed";
                    }

                    return $"{NumberOfApproved}/{DesiredNumberOfApproved} approved";
                }

                return string.Empty;
            }
        }

        public PRState State
        {
            get
            {
                if (!HasChangeRequests
                    && (TestingState == LabelState.None || TestingState == LabelState.Passed)
                    && (UXReviewState == LabelState.None || UXReviewState == LabelState.Passed)
                    && IsReviewed)
                {
                    return PRState.Done;
                }
                else if (!HasChangeRequests && Mergable
                    && (TestingState == LabelState.Needed)
                    && (UXReviewState == LabelState.None || UXReviewState == LabelState.Passed)
                    && IsReviewed)
                {
                    return PRState.Testing;
                }
                else if (IsReadyForReview)
                {
                    return PRState.CodeReview;
                }
                else
                {
                    return PRState.None;
                }
            }
        }

        public ReviewItem ReviewItem
        {
            get
            {
                var reviewItem = new ReviewItem();
                reviewItem.Title = Name;
                reviewItem.SecondaryText = $"#{Number} by {AssigneeName}";

                if (SettingsService.IsPersonalStatus && AssigneeName != GithubDisplay.Resources.User.Login)
                {
                    reviewItem.Status = PersonalStatus;
                    reviewItem.StatusBrush =
                        new SolidColorBrush(PersonalStatus == "Approved" ? Colors.Green : Colors.DarkRed);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(ErrorStatus))
                    {
                        reviewItem.Status = CodeReviewStatus;
                        reviewItem.StatusBrush = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        reviewItem.Status = ErrorStatus;
                        reviewItem.StatusBrush = new SolidColorBrush(Colors.DarkRed);
                    }
                }

                return reviewItem;
            }
        }

        int _GetNumberOfDesiredApprovers(string body)
        {
            if (body.ToLower().Contains("approvers:"))
            {
                var appr = body.Substring(body.ToLower().IndexOf("approvers:") + "approvers:".Length, 3).Trim(' ', '\n', '\t', ';');
                
                if (int.TryParse(appr, out var result))
                {
                    return result;
                }
            }

            return 3;
        }

        void _PropertyChanges()
        {
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(StatusBrush));
            OnPropertyChanged(nameof(ErrorStatus));
            OnPropertyChanged(nameof(CodeReviewStatus));
            OnPropertyChanged(nameof(PersonalStatus));
            OnPropertyChanged(nameof(ReviewItem));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public enum LabelState
        {
            None,
            Needed,
            Passed,
            Failed,
        }
    }

    public enum PRState
    {
        None,
        CodeReview,
        Testing,
        Done,
    }

    public class PullRequestReviewComparer : IComparer<PullRequest>
    {
        public int Compare(PullRequest x, PullRequest y)
        {
            if (x.AssigneeName == GithubDisplay.Resources.User.Login
                && y.AssigneeName != x.AssigneeName)
                return 1;

            if (y.AssigneeName == GithubDisplay.Resources.User.Login
                && y.AssigneeName != x.AssigneeName)
                return -1;

            if (SettingsService.IsPersonalStatus)
            {
                if (x.PersonalStatus == "Approved"
                    && y.PersonalStatus != "Approved")
                    return 1;

                if (y.PersonalStatus == "Approved"
                    && x.PersonalStatus != "Approved")
                    return -1;
            }

            return x.Number - y.Number;
        }
    }
}
