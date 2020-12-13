using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

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

        public Color ScoreColor
        {
            get
            {
                if (Score >= 75)
                {
                    return Color.FromArgb(0xff, 0x74, 0xcb, 0x2c);
                }
                else if (Score >= 50)
                {
                    return Color.FromArgb(0xff, 0xFB, 0xCC, 0x21);
                }
                else
                {
                    return Color.FromArgb(0xff, 0xF5, 0x16, 0x00);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
