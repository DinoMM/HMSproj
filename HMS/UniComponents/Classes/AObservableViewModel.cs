using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public abstract class AObservableViewModel<T> : IObservableViewModel, IDisposable
    {
        /// <summary>
        /// Zoznam poloziek, s ktorými sa pracuje
        /// </summary>
        public ObservableCollection<T> ZoznamPoloziek { get; set; } = new();

        /// <summary>
        /// Indikátor, či sa práve načítavajú zoznamy
        /// </summary>
        public bool Nacitavanie { get; set; } = false;

        /// <summary>
        /// Tabuľka, ktorá sa udržuje
        /// </summary>
        [CopyProperties]
        public ComplexTable<T>? ComplexTable { get; set; }

        /// <summary>
        /// Sem treba dať načítanie zoznamov. Na volanie pre načítanie zoznamov sa použije metóda NacitajZoznamy
        /// </summary>
        /// <returns></returns>
        protected abstract Task NacitajZoznamyAsync();

        /// <summary>
        /// Spustí načítanie zoznamov.
        /// </summary>
        /// <returns></returns>
        public async Task NacitajZoznamy()
        {
            await Nacitaj(
                methodAsync: async () => await SilentCollection(
                    methodAsync: async () => await NacitajZoznamyAsync()
                    )
                );
            //Nacitavanie = true;
            //ZoznamPoloziek.CollectionChanged -= OnCollectionChanged;
            //await NacitajZoznamyAsync();
            //ZoznamPoloziek.CollectionChanged += OnCollectionChanged;
            //Nacitavanie = false;
        }

        /// <summary>
        /// Metóda, ktorá sa zavolá pri zmene ZoznamPoloziek cez event CollectionChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual async void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ComplexTable != null)
            {
                await ComplexTable.OnItemsChange(e);
            }
        }

        /// <summary>
        /// Notifikuje manualne po spustení metódy o zmene v kolekcii (funkcionalita istá ako OnCollectionChanged len bez boilerplatu). Default NotifyCollectionChangedAction.Reset
        /// </summary>
        /// <returns></returns>
        public virtual async Task ManualNotifyColection(NotifyCollectionChangedAction result = NotifyCollectionChangedAction.Reset)
        {
            if (ComplexTable != null)
            {
                await ComplexTable.OnItemsChange(new NotifyCollectionChangedEventArgs(result));
            }
        }

        /// <summary>
        /// Ak je možné vymazať položku, vráti true, inak false
        /// </summary>
        /// <param name="polozka"></param>
        /// <returns></returns>
        public virtual bool MoznoVymazat(T polozka)
        {
            return true;
        }

        /// <summary>
        /// Vymaže položku zo zoznamu, najlepšie volať vždy base.Vymazat(polozka); v prekrytých metódach
        /// </summary>
        /// <param name="polozka"></param>
        public virtual void Vymazat(T polozka)
        {
            ZoznamPoloziek.Remove(polozka);
        }

        /// <summary>
        /// Možná inicializácia, ktorá sa zavolá pri špecificky zdenených komponentoch (TransielHoldedComp)
        /// </summary>
        public virtual void Initialization()
        {
            ZoznamPoloziek.CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        /// Vykoná sa pri zničení objektu, ktorá sa zavolá pri špecificky zdenených komponentoch (TransielHoldedComp)
        /// </summary>
        public virtual void Dispose()
        {
            ZoznamPoloziek.CollectionChanged -= OnCollectionChanged;
        }

        /// <summary>
        /// Zaobalí metódu/y do načítavania. Default sa nastaví Nacitavanie na true a po skončení 'method' sa potom nastaví na false
        /// </summary>
        /// <param name="methodAsync">ak nie je null tak sa spustí</param>
        /// <param name="method">ak nie je null tak sa spustí</param>
        /// <returns></returns>
        public virtual async Task Nacitaj(Func<Task>? methodAsync = null, Action? method = null)
        {
            Nacitavanie = true;     //hlavna vec
            if (methodAsync != null)
            {
                await methodAsync();
            }
            if (method != null)
            {
                method?.Invoke();
            }
            Nacitavanie = false; //hlavna vec
        }

        /// <summary>
        /// Zaobalí metódu/y do načítavania. Default sa odstani event OnCollectionChanged z ObservableCollection a potom sa vráti späť
        /// </summary>
        /// <param name="methodAsync">ak nie je null tak sa spustí</param>
        /// <param name="method">ak nie je null tak sa spustí</param>
        /// <returns></returns>
        public virtual async Task SilentCollection(Func<Task>? methodAsync = null, Action? method = null)
        {
            ZoznamPoloziek.CollectionChanged -= OnCollectionChanged; //hlavna vec
            if (methodAsync != null)
            {
                await methodAsync();
            }
            if (method != null)
            {
                method?.Invoke();
            }
            ZoznamPoloziek.CollectionChanged += OnCollectionChanged; //hlavna vec
        }

    }

    public interface IObservableViewModel
    {
        //public ObservableCollection<T> ZoznamPoloziek { get; set; }
        public bool Nacitavanie { get; set; }
        //public ComplexTable<T>? ComplexTable { get; set; }

        public Task NacitajZoznamy();
        public void Dispose();
        public void Initialization();
        //public bool MoznoVymazat(T polozka);
        //public void Vymazat(T polozka);
    }

    /// <summary>
    /// Dedí z AObservableViewModel, podobna funkcionalita, cez konstruktor je moznost aj nastaviť metodu na nacitanie konkretneho zoznamu. Malo by to byt safe aj bez pouzitia ComplexTable<para/>
    /// Treba potom spustit NacitajZoznamy() na nacitanie zoznamov a najlepsie po skonceni spustit Dispose() pred odidenim z kontextu
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionOwn<T> : AObservableViewModel<T>
    {
        /// <summary>
        /// Ak existuje metodia v Nacitavani Zoznamov tak sa nastaví na true, inak false
        /// </summary>
        public bool CanBeLoaded { get; set; } = false;

        /// <summary>
        /// Pre potrebu zistenia, či je možné vymazať položku. Najlepšie nechať načítať v Nacitavani zoznamov
        /// </summary>
        public List<(T, bool)> MoznoVymazatList { get; set; } = new();

        private readonly Func<Task>? _nacitajZoznamyAsync;
        private readonly Func<T, bool>? _moznoVymazat;
        private readonly Action<T>? _Vymazat;

        /// <summary>
        /// Konštruktor, ktorý umožňuje nastaviť metódu na načítanie zoznamov a metódu na zistenie, či je možné vymazať položku + metodu na vymazanie
        /// </summary>
        /// <param name="nacitajZoznamyAsync">Default nic</param>
        /// <param name="moznoVymazat">Default true</param>
        /// <param name="vymazat">Vždy sa volá base.Vymazat(T)</param>
        public ObservableCollectionOwn(Func<Task>? nacitajZoznamyAsync = null, Func<T, bool>? moznoVymazat = null, Action<T>? vymazat = null)
        {
            _nacitajZoznamyAsync = nacitajZoznamyAsync;
            _moznoVymazat = moznoVymazat;
            _Vymazat = vymazat;

            CanBeLoaded = _nacitajZoznamyAsync != null;
        }

        protected override async Task NacitajZoznamyAsync()
        {
            if (_nacitajZoznamyAsync != null)
            {
                await _nacitajZoznamyAsync();
            }
        }

        public override bool MoznoVymazat(T polozka)
        {
            if (_moznoVymazat != null)
            {
                return _moznoVymazat(polozka);
            }
            return true;
        }

        public override void Vymazat(T polozka)
        {
            if (_Vymazat != null)
            {
                base.Vymazat(polozka);
                _Vymazat(polozka);
            }
            else
            {
                base.Vymazat(polozka);
            }
        }
    }
}
