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
            Nacitavanie = true;
            ZoznamPoloziek.CollectionChanged -= OnCollectionChanged;
            await NacitajZoznamyAsync();
            ZoznamPoloziek.CollectionChanged += OnCollectionChanged;
            Nacitavanie = false;
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
}
