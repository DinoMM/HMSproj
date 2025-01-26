using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public class TableFilterDateTime<T> : ATableFilter<T>
    {
        public DateTime? FilterOd { get; set; } = null;

        public DateTime? FilterDo { get; set; } = null;

        public TableFilterDateTime()
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
            return new TableFilterDateTime<T>()
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
        public override TableFilterDateTime<T> Clon()
        {
            return (TableFilterDateTime<T>)Clone();
        }

        /// <summary>
        /// Vyfiltruje dátum podľa zvoleného filtru. Ak filter pasuje vracia true
        /// </summary>
        /// <param name="date">Dátum, na ktorý má byť aplikovaný filter</param>
        /// <returns></returns>
        public override bool Filter(object? date)
        {
            var datstr = (string?)date;
            var dat = datstr.ParseToDateTime("dd.MM.yyyy", "dd.MM.yyyy HH:mm:ss");
            if (NullsOnly != null)
            {
                if (dat == null && !NullsOnly.Value)
                {
                    return false;
                }
                if (dat != null && NullsOnly.Value)
                {
                    return false;
                }
            }

            var value = dat ?? new DateTime();

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
