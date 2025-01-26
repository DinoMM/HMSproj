using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace UniComponents
{
    public class TableFilterBool<T> : ATableFilter<T>
    {
        public bool? FilterB { get; set; } = null;

        public TableFilterBool()
        {
            UsesMatchCase = false;
            UsesExclude = false;
            UsesNullsOnly = true;
        }

        /// <summary>
        /// nastavenie na default hodnoty bez úpravy ID_Prop, CellValue
        /// </summary>
        public override void ClearFilter()
        {
            base.ClearFilter();
            FilterB = null;
        }

        public override object Clone()
        {
            return new TableFilterBool<T>()
            {
                ID_Prop = this.ID_Prop,
                CellValue = this.CellValue,
                FilterB = this.FilterB,
                MatchCase = this.MatchCase,
                Exclude = this.Exclude,
                NullsOnly = this.NullsOnly,
                UsesMatchCase = this.UsesMatchCase,
                UsesExclude = this.UsesExclude,
                UsesNullsOnly = this.UsesNullsOnly
            };
        }
        public override TableFilterBool<T> Clon()
        {
            return (TableFilterBool<T>)Clone();
        }

        /// <summary>
        /// Vyfiltruje hodnotu podľa zvoleného filtru. Ak filter pasuje vracia true
        /// </summary>
        /// <param name="check">Bool, na ktorý má byť aplikovaný filter</param>
        /// <returns></returns>
        public override bool Filter(object? check)
        {
            if (NullsOnly != null)
            {
                if (check == null && !NullsOnly.Value)
                {
                    return false;
                }
                if (check != null && NullsOnly.Value)
                {
                    return false;
                }
            }

            bool? value = (bool?)check;
            return value == FilterB;
        }

        /// <summary>
        /// Uzná či je filter aplikovateľný, ak nie je tak nepotrebujeme filtrovať. Treba upraviť ak pridáme nové filtery
        /// </summary>
        /// <returns></returns>
        public override bool CheckIfAppliable()
        {
            return FilterB != null || NullsOnly != null;
        }

    }
}
