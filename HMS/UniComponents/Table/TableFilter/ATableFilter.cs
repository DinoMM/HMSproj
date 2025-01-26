using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public abstract class ATableFilter<T> : ICloneable
    {
        /// <summary>
        /// Označenie stĺpca, ktorý sa filtruje
        /// </summary>
        public string ID_Prop { get; set; } = "";

        public Func<T, object?>? CellValue { get; set; }

        /// <summary>
        /// Ak je true, tak sa pri filtrovaní bude zohľadňovať veľkosť písmen
        /// </summary>
        public bool MatchCase { get; set; } = false;
        public bool UsesMatchCase { get; set; } = false;

        /// <summary>
        /// Ak je true, tak sa pri filtrovaní zobrazia iba riadky, ktoré neobsahujú filter
        /// </summary>
        public bool Exclude { get; set; } = false;
        public bool UsesExclude { get; set; } = true;

        /// <summary>
        /// Ak je null, tak sa neberú do úvahy hodnoty null, ak je true, tak sa zobrazia iba riadky s hodnotou null, ak je false, tak sa zobrazia iba riadky s hodnotou inou ako null
        /// </summary>
        public bool? NullsOnly { get; set; } = null;
        public bool UsesNullsOnly { get; set; } = true;

        /// <summary>
        /// nastavenie na default hodnoty bez úpravy ID_Prop, CellValue
        /// </summary>
        public virtual void ClearFilter()
        {
            MatchCase = false;
            Exclude = false;
            NullsOnly = null;
        }

        public abstract object Clone();

        public abstract ATableFilter<T> Clon();


        /// <summary>
        /// Vyfiltruje objekt podľa zvoleného filtru. Ak filter pasuje vracia true
        /// </summary>
        /// <param name="obj">object, na ktorý má byť aplikovaný filter</param>
        /// <returns></returns>
        public abstract bool Filter(object? obj);


        /// <summary>
        /// Uzná či je filter aplikovateľný, ak nie je tak nepotrebujeme filtrovať. Treba upraviť ak pridáme nové filtery
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckIfAppliable();

        public static ATableFilter<T> CreateFilter<T>(string pID_Prop, Func<T, object?>? pCellValue)
        {
            ATableFilter<T> newFilter;
            var property = typeof(T).GetProperty(pID_Prop);
            var propType = property.PropertyType;
            if (propType == typeof(int) ||
                propType == typeof(long) ||
                propType == typeof(float) ||
                propType == typeof(double) ||
                propType == typeof(decimal))
            {
                newFilter = new TableFilterNum<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            else if (propType == typeof(DateOnly) ||
                     propType == typeof(DateTime))
            {
                newFilter = new TableFilterDateTime<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            else if (propType == typeof(bool))
            {
                newFilter = new TableFilterBool<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            else
            {
                newFilter = new TableFilter<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            return newFilter;
        }

        public static ATableFilter<T> CreateFilter<T>(string pID_Prop, Func<T, object?>? pCellValue, Type newType)
        {
            ATableFilter<T> newFilter;
            if (newType == typeof(int) ||
                newType == typeof(long) ||
                newType == typeof(float) ||
                newType == typeof(double) ||
                newType == typeof(decimal))
            {
                newFilter = new TableFilterNum<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            else if (newType == typeof(DateOnly) ||
                     newType == typeof(DateTime))
            {
                newFilter = new TableFilterDateTime<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            else if (newType == typeof(bool))
            {
                newFilter = new TableFilterBool<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            else
            {
                newFilter = new TableFilter<T>() { ID_Prop = pID_Prop, CellValue = pCellValue };
            }
            return newFilter;
        }
    }
}
