using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Vrati DisplayName pre enum hodnotu [Display(Name = "hodnota")]
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns>Ak má enum [Display(Name = "hodnota")] tak vtári "hodnota" inak vráti default string z názvu </returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayName = enumValue.GetType()
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            return displayName?.Name ?? enumValue.ToString();
        }
    }
}
