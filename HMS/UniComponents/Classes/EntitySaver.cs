using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UniComponents
{
    public class EntitySaver<T> : IEntitySaver where T : class, ICloneable, new()
    {
        /// <summary>
        /// Original. Default null
        /// </summary>
        public T? Original { get; private set; } = null;
        /// <summary>
        /// Input, ktorý sa zobrazuje v UI a je možné ho meniť. Default new T()
        /// </summary>
        public T Input { get; set; } = new T();

        /// <summary>
        /// Tuple obsahujuci DbContext, DbSet a metodu na najdenie entity -> pre ukladanie do Contextu. Default null. Napr. (T x) => db.dbs.FirstOrDefault(y => y.ID == x.ID)
        /// </summary>
        public (DbContext, DbSet<T>, Func<T, T?>) SavingTuple { get; set; }

        /// <summary>
        /// Ak bola [ObservableProperty] property v entite zmenena. Default true
        /// </summary>
        public bool Modified { get; private set; } = true;
        /// <summary>
        /// Ak už existuje entita (Original ma priradenú hodnotu). Default false
        /// </summary>
        public bool Exist { get; private set; } = false;

        /// <summary>
        /// Konštruktor, ktorý musí nastaviť SavingTuple
        /// </summary>
        /// <param name="db">Context, v ktorom sa prejavia zmeny</param>
        /// <param name="dbs">DataSet, v ktorom sa prejavia zmeny </param>
        /// <param name="mth">Metóda na nájdenie existujúcej Entity v DataSete napr. (T x) => db.dbs.FirstOrDefault(y => y.ID == x.ID)</param>
        public EntitySaver(in DbContext db, in DbSet<T> dbs, Func<T, T?> mth)
        {
            SavingTuple = (db, dbs, mth);

            BindInput();
        }

        /// <summary>
        /// Nastaví Input savera, nutnosť ak Entity už existuje
        /// </summary>
        /// <param name="original"></param>
        public void SetEntity(T original)
        {
            Original = original;
            Input = (T)original.Clone();
            Modified = false;
            Exist = true;

            BindInput();
        }

        /// <summary>
        /// Nastavi danu entitu, ak je null, vrati false
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public bool CanSetEntity(T? original)
        {
            if (original == null)
            {
                return false;
            }
            SetEntity(original);
            return true;
        }

        /// <summary>
        /// Nastavenie na východzie hodnoty
        /// </summary>
        public void Clear()
        {
            Original = null;
            Input = new T();
            Modified = true;
            Exist = false;
            BindInput();
        }

        /// <summary>
        /// Uloží entitu do databázy (ak sú zmeny). Ak neexistuje, pridá ju, inak upraví. Musí byť nastavený predom SavingTuple
        /// </summary>
        /// <returns>Odpoveď pre chybové hlášky</returns>
        public ValidationResult SaveEntity()
        {
            if (Input is not ObservableObject)  //kontrola pre triedy, ktore nie su ObservableObject
            {
                if (Original == null)
                {
                    Modified = true;
                }
                else
                {
                    Modified = !ArePropertiesEqual(Original, Input);
                }
            }

            if (Modified)
            {
                if (Original == null)
                {
                    SavingTuple.Item2.Add(Input);
                    Original = Input;
                    Input = (T)Original.Clone();
                    BindInput();
                }
                else
                {
                    var founded = SavingTuple.Item3(Input);
                    if (founded == null)
                    {
                        return new ValidationResult($"Nebola nájdená entita: {Input.GetType().Name}");
                    }
                    founded.CopyPropertiesFrom(Input);      //prekopiruje vsetky hodnoty z Input do founded
                }

                SavingTuple.Item1.SaveChanges();        //uloženie zmien do DB
                Modified = false;
                Exist = true;
            }
            return ValidationResult.Success;
        }

        /// <summary>
        /// Uloží entitu podľa vlastne zadaného SavingTuple, nemení po ukončení SavingTuple
        /// </summary>
        /// <param name="db">Context, v ktorom sa prejavia zmeny</param>
        /// <param name="dbs">DataSet, v ktorom sa prejavia zmeny </param>
        /// <param name="mth">Metóda na nájdenie existujúcej Entity v DataSete napr. (T x) => db.dbs.FirstOrDefault(y => y.ID == x.ID)</param>
        /// <returns></returns>
        public ValidationResult SaveEntity(in DbContext db, in DbSet<T> dbs, Func<T, T?> mth)
        {
            var savingTuple = (db, dbs, mth);
            var tmp = SavingTuple;
            SavingTuple = savingTuple;
            var result = SaveEntity();
            SavingTuple = tmp;
            return result;
        }

        /// <summary>
        /// Čo sa má stať ak sa zmení property v [ObservableObject]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Modified && Exist && (Original != null))
            {
                Modified = !ArePropertiesEqual(Original, Input);
            }
            else
            {
                Modified = true;
            }
        }

        /// <summary>
        /// Porobvná property z objektov ak ak sa rovnajú vráti true
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        private bool ArePropertiesEqual(T obj1, T obj2) //pomoc od AI
        {
            if (obj1 == null || obj2 == null)
                return false;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);
                if (!Equals(value1, value2))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Pre každú Entitu, ktorá je [ObservableObject] sa nastaví event na zmenu property. Tato entity by mala mať [ObservableProperty] na určené property, kde sa budu trakovať zmeny
        /// </summary>
        private void BindInput()
        {
            if (Input is ObservableObject inputObservable)
            {
                inputObservable.PropertyChanged += OnPropertyChanged;
            }
            else
            {
                Modified = true;
            }
        }

    }
}
