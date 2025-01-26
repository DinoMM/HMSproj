using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public class TableFilterNum<T> : ATableFilter<T>
    {
        public decimal? FilterOd { get; set; } = null;

        public decimal? FilterDo { get; set; } = null;

        public TableFilterNum()
        {
            UsesMatchCase = false;
        }

        /// <summary>
        /// nastavenie na default hodnoty bez úpravy ID_Prop, CellValue
        /// </summary>
        public override void ClearFilter()
        {
            base.ClearFilter();
            FilterOd = null;
            FilterDo = null;
        }

        public override object Clone()
        {
            return new TableFilterNum<T>()
            {
                ID_Prop = this.ID_Prop,
                CellValue = this.CellValue,
                FilterOd = this.FilterOd,
                FilterDo = this.FilterDo,
                MatchCase = this.MatchCase,
                Exclude = this.Exclude,
                NullsOnly = this.NullsOnly,
                UsesMatchCase = this.UsesMatchCase,
                UsesExclude = this.UsesExclude,
                UsesNullsOnly = this.UsesNullsOnly
            };
        }
        public override TableFilterNum<T> Clon()
        {
            return (TableFilterNum<T>)Clone();
        }

        /// <summary>
        /// Vyfiltruje hodnotu podľa zvoleného filtru. Ak filter pasuje vracia true
        /// </summary>
        /// <param name="number">Číslo, na ktorý má byť aplikovaný filter</param>
        /// <returns></returns>
        public override bool Filter(object? number)
        {
            decimal? val = 0;
            if (decimal.TryParse(number?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedValue))
            {
                val = parsedValue;
                if (number == null)
                {
                    val = null;
                }
            }

            if (NullsOnly != null)
            {
                if (val == null && !NullsOnly.Value)
                {
                    return false;
                }
                if (val != null && NullsOnly.Value)
                {
                    return false;
                }
            }

            var value = val ?? 0;
            val = Math.Round(value, 4);     //zaokruhlenie na 4 desatine miesta

            var result = true;
            if (FilterOd != null)
            {
                result = value >= FilterOd;
            }
            if (result && FilterDo != null)
            {
                result = value <= FilterDo;
            }

            return Exclude ? !result : result;
        }

        /// <summary>
        /// Uzná či je filter aplikovateľný, ak nie je tak nepotrebujeme filtrovať. Treba upraviť ak pridáme nové filtery
        /// </summary>
        /// <returns></returns>
        public override bool CheckIfAppliable()
        {
            return FilterOd != null || FilterDo != null || NullsOnly != null;
        }

    }
}
