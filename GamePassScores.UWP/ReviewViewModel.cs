using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP
{
    public class ReviewViewModel : INotifyPropertyChanged
    {
        private string _mediaName = string.Empty;
        public string MediaName
        {
            set
            {
                _mediaName = value;
                NotifyPropertyChanged();
            }
            get
            {
                return _mediaName;
            }
        }

        public DateTime PublishDate { set; get; }
        public string Description { set; get; }
        public string Url { set; get; }
        public int Score { set; get; }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
