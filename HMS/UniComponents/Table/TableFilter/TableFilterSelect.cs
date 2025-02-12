using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public class TableFilterSelect<T> : ATableFilter<T>
    {
        /// <summary>
        /// Text, ktorý sa použije na filtrovanie
        /// </summary>
        public string FilteredText { get; set; } = "";

        /// <summary>
        /// Zoznam hodnôt na výber
        /// </summary>
        public List<string> Selections { get; set; } = new();

        public TableFilterSelect()
        {
            UsesNullsOnly = true;
            UsesMatchCase = false;
            UsesExclude = true;
        }

        /// <summary>
        /// nastavenie na default hodnoty bez úpravy ID_Prop, CellValue
        /// </summary>
        public override void ClearFilter()
        {
            base.ClearFilter();
            FilteredText = "";
        }

        public override object Clone()
        {
            return new TableFilterSelect<T>()
            {
                ID_Prop = this.ID_Prop,
                CellValue = this.CellValue,
                FilteredText = this.FilteredText,
                MatchCase = this.MatchCase,
                Exclude = this.Exclude,
                NullsOnly = this.NullsOnly,
                UsesMatchCase = this.UsesMatchCase,
                UsesExclude = this.UsesExclude,
                UsesNullsOnly = this.UsesNullsOnly

            };
        }
        public override TableFilterSelect<T> Clon()
        {
            return (TableFilterSelect<T>)Clone();
        }

        /// <summary>
        /// Vyfiltruje text podľa zvoleného filtru. Ak filter pasuje vracia true
        /// </summary>
        /// <param name="text">Text, na ktorý má byť aplikovaný filter</param>
        /// <returns></returns>
        public override bool Filter(object? text)
        {
            var str = (string?)text;
            if (NullsOnly != null)
            {
                if (string.IsNullOrEmpty(str) && !NullsOnly.Value)
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(str) && NullsOnly.Value)
                {
                    return false;
                }
            }

            if (str == null)
            {
                return string.IsNullOrEmpty(FilteredText);
            }

            var result = str == FilteredText;
            return Exclude ? !result : result;
        }

        /// <summary>
        /// Uzná či je filter aplikovateľný, ak nie je tak nepotrebujeme filtrovať. Treba upraviť ak pridáme nové filtery
        /// </summary>
        /// <returns></returns>
        public override bool CheckIfAppliable()
        {
            return !string.IsNullOrEmpty(FilteredText) || NullsOnly != null;
        }

    }
}
