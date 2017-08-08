using GithubDisplay.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;

namespace GithubDisplay.ViewModels
{
    public class ReviewItem : INotifyPropertyChanged
    {
        string _title;

        string _secondary_text;

        string _status;

        Brush _statusBrush;

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string SecondaryText
        {
            get => _secondary_text;
            set
            {
                if (value == _secondary_text) return;
                _secondary_text = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public Brush StatusBrush
        {
            get => _statusBrush;
            set
            {
                if (Equals(value, _statusBrush)) return;
                _statusBrush = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
