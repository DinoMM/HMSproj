using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniComponents.Services;

namespace UniComponents
{
    public enum CascadingState
    {
        Loading,
    }
}

namespace UniComponents.Services
{
    public interface ICascadingService : INotifyPropertyChanged
    {
        // Pre 'CascLoading'
        public bool Nacitavanie { get; set; }

        // Pre 'CascLoading'
        //public bool Nacitavanie { get; set; }

        public Func<bool, Task>? NacitavanieMethod { get; set; }

        //public Func<bool, Task>? DisabledMethod { get; set; }

        public void SetState(bool state, IEnumerable<CascadingState> states, bool exeptThose = false);
        public Task SetStateAsync(bool state, IEnumerable<CascadingState> states, bool exeptThose = false);

        public void Run(Action action, bool exeptThose = false, params CascadingState[] states);
        public Task RunAsync(Func<Task> action, bool exeptThose = false, params CascadingState[] states);


    }

    public class CascadingService : ICascadingService
    {
        //private bool _disabled;
        private bool _nacitavanie;

        public event PropertyChangedEventHandler PropertyChanged;

        //public bool Nacitavanie
        //{
        //    get => _disabled;
        //    set
        //    {
        //        if (_disabled != value)
        //        {
        //            _disabled = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Nacitavanie)));
        //        }
        //    }
        //}

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

        public Func<bool, Task>? NacitavanieMethod { get; set; }

        //public Func<bool, Task>? DisabledMethod { get; set; }

        public void SetState(bool state, IEnumerable<CascadingState> states, bool exeptThose = false)
        {
            List<CascadingState> listStates = new(states);
            if (exeptThose)
            {
                List<CascadingState> allStates = new(Enum.GetValues(typeof(CascadingState)).Cast<CascadingState>());
                foreach (var item in listStates)
                {
                    allStates.Remove(item);
                }
                listStates = allStates;
            }

            foreach (var s in listStates)
            {
                switch (s)
                {
                    //case CascadingState.Disabled:
                    //    Nacitavanie = state;
                    //    break;
                    case CascadingState.Loading:
                        Nacitavanie = state;
                        break;
                    default: throw new NotImplementedException("Neni vytvorené riešenie pre tento stav v ICascadingService" + s.ToString());
                }
            }
        }

        public async Task SetStateAsync(bool state, IEnumerable<CascadingState> states, bool exeptThose = false)
        {
            List<CascadingState> listStates = new(states);
            if (exeptThose)
            {
                List<CascadingState> allStates = new(Enum.GetValues(typeof(CascadingState)).Cast<CascadingState>());
                foreach (var item in listStates)
                {
                    allStates.Remove(item);
                }
                listStates = allStates;
            }

            foreach (var s in listStates)
            {
                switch (s)
                {
                    //case CascadingState.Disabled:
                    //    //if (DisabledMethod != null)
                    //    //{
                    //    //    await DisabledMethod(state);
                    //    //}
                    //    Nacitavanie = state;
                    //    break;
                    case CascadingState.Loading:
                        //if (NacitavanieMethod != null)
                        //{
                        //    await NacitavanieMethod(state);
                        //}
                        Nacitavanie = state;
                        break;
                    default: throw new NotImplementedException("Neni vytvorené riešenie pre tento stav v ICascadingService" + s.ToString());
                }
            }
        }

        public void Run(Action action, bool exeptThose = false, params CascadingState[] states)
        {
            if (states.Count() == 0)
            {
                return;
            }
            var statesDis = states.Distinct();
            SetState(true, statesDis, exeptThose: exeptThose);
            action();
            SetState(false, statesDis, exeptThose: exeptThose);
        }

        public async Task RunAsync(Func<Task> action, bool exeptThose = false, params CascadingState[] states)
        {
            if (states.Count() == 0)
            {
                return;
            }
            var statesDis = states.Distinct();
            //await SetStateAsync(true, statesDis, exeptThose: exeptThose);
            SetState(true, statesDis, exeptThose: exeptThose);
            await action();
            SetState(false, statesDis, exeptThose: exeptThose);
            //await SetStateAsync(false, statesDis, exeptThose: exeptThose);
        }

    }
}


