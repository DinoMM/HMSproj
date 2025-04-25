using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents.Services
{
    public interface ICascadingService : INotifyPropertyChanged
    {
        // Pre 'CascDisabled'
        public bool Disabled { get; set; }

        // Pre 'CascLoading'
        public bool Nacitavanie { get; set; }
    }

    public class CascadingService : ICascadingService
    {
        private bool _disabled;
        private bool _nacitavanie;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (_disabled != value)
                {
                    _disabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Disabled)));
                }
            }
        }

        public bool Nacitavanie
        {
            get => _nacitavanie;
            set
            {
                if (_nacitavanie != value)
                {
                    _nacitavanie = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nacitavanie)));
                }
            }
        }
    }

}
