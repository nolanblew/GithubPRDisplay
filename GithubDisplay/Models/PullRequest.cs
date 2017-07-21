using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using GithubDisplay.Annotations;
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
            AssigneeName = pr.Assignee.Login;
            PrUrl = pr.HtmlUrl;
        }

        public PullRequest(Octokit.Issue issue) : this(issue, issue.PullRequest)  { }

        public PullRequest(Octokit.Issue issue, Octokit.PullRequest pullRequest)
            : this(pullRequest)
        {

            var labels = issue.Labels.Select(l => l.Name).ToList();
            IsReviewed = labels.Contains("Passed Review");
            IsBlocked = labels.Contains("Blocked");
            IsReadyForReview = labels.Contains("Ready for review");
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

        LabelState _testingState;

        LabelState _uxReviewState;

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

                return string.Empty;
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

        //public bool Merge(PullRequest newItem)
        //{
        //    this.AssigneeName = newItem.AssigneeName;
        //    this.DesiredNumberOfApproved = newItem.DesiredNumberOfApproved;
        //    this.HasChangeRequests = newItem.HasChangeRequests;
        //    this.IsBlocked = newItem.IsBlocked;
        //    this.IsReadyForReview = newItem.IsReadyForReview;
        //    this.IsReviewed = newItem.IsReviewed;
        //    this.Mergable = newItem.Mergable;
        //    this.Name = newItem.Name;
        //    this.TestingState = newItem.TestingState;
        //    this.NumberOfApproved = newItem.NumberOfApproved;
        //    this.UXReviewState = newItem.UXReviewState;
        //}
    }

    public enum PRState
    {
        None,
        CodeReview,
        Testing,
        Done,
    }
}
