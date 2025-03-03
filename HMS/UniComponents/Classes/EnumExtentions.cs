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
        /// <returns>Ak má enum [Display(Name = "hodnota")] tak vráti "hodnota" inak vráti default string z názvu </returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayName = enumValue.GetType()
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            return displayName?.Name ?? enumValue.ToString();
        }

        /// <summary>
        /// Vráti list s hodnotami DisplayName pre daný enum.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static List<string> GetDisplayNames<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                   .Cast<TEnum>()
                   .Select(x => x.GetDisplayName())
                   .ToList();
        }

        /// <summary>
        /// Vráti enum hodnotu podľa DisplayName (alebo ak neni tak podla string reprezentacie)
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TEnum GetEnumValue<TEnum>(string value) where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                   .Cast<TEnum>()
                   .First(x => x.GetDisplayName() == value);
                   
        }
    }
}
