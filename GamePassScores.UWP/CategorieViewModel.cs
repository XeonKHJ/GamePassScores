using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.UWP
{
    public class CategorieViewModel: INotifyPropertyChanged
    {
        public string Categorie { set; get; } = string.Empty;
        public CategorieViewModel(string categorie)
        {
            Categorie = categorie;
        }

        private bool _isChecked = false;
        public bool IsChecked
        {
            set
            {
                _isChecked = value;
                NotifyPropertyChanged();
            }
            get
            {
                return _isChecked;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
