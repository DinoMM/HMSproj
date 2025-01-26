using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public static class ObjectExtensions
    {
        #region CopyPropertiesFrom
        /// <summary>
        /// Copies all public properties from the source object to the target object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CopyPropertiesFrom<T>(this T target, T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(source);
                    property.SetValue(target, value);
                }
            }
        }

        /// <summary>
        /// Copies all public + CopyPropertiesAttribute/IgnoreAttribute - properties/fields (+ private props) from the source object to the target object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CopyPropertiesFromX<T>(this T target, T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);  //ziska vsetky public properties

            foreach (var prop in properties)    //tie s atributom CopyPropertiesAttribute sa nekopiruju ale len ich vnutro
            {
                if (Attribute.IsDefined(prop, typeof(CopyPropertiesAttribute)))
                {
                    var sourceValue = prop.GetValue(source, null);
                    var targetValue = prop.GetValue(target, null);
                    if (sourceValue != null && targetValue != null)
                    {
                        //spustí metódu CopyPropertiesFromX na property s týmto atribútom (rekurzia)
                        var copyMethod = typeof(ObjectExtensions).GetMethod(nameof(CopyPropertiesFromX), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                               .MakeGenericMethod(prop.PropertyType);
                        copyMethod.Invoke(null, new object[] { targetValue, sourceValue });
                    }
                }
                else if (Attribute.IsDefined(prop, typeof(IgnoreCopyAttribute)))
                {
                    continue;
                }
                else
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        var value = prop.GetValue(source, null);
                        prop.SetValue(target, value, null);
                    }
                }
            }

            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance); //ziska vsetky public fields (ktore nemaju {get; set;})
            foreach (var field in fields)
            {
                var value = field.GetValue(source);
                field.SetValue(target, value);
            }
        }

        /// <summary>
        /// Copies all public/private + CopyPropertiesAttribute/IgnoreAttribute - properties/fields from the source object to the target object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CopyAllPropertiesFromX<T>(this T target, T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);  //ziska vsetky public properties

            var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance); //ziska vsetky public + private fields

            var newlist = fields.ToList();  //odstrani property z fields lebo vo fields su vsetky jak public tak aj private fields
            foreach (var prop in properties)    //tie s atributom CopyPropertiesAttribute sa nekopiruju ale len ich vnutro
            {
                if (Attribute.IsDefined(prop, typeof(CopyPropertiesAttribute)))
                {
                    var sourceValue = prop.GetValue(source, null);
                    var targetValue = prop.GetValue(target, null);
                    if (sourceValue != null && targetValue != null)
                    {
                        //spustí metódu CopyPropertiesFromX na property s týmto atribútom (rekurzia)
                        var copyMethod = typeof(ObjectExtensions).GetMethod(nameof(CopyAllPropertiesFromX), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                               .MakeGenericMethod(prop.PropertyType);
                        copyMethod.Invoke(null, new object[] { targetValue, sourceValue });

                        newlist.RemoveAll(x => x.Name == $"<{prop.Name}>k__BackingField" && x.FieldType == prop.PropertyType);
                        fields = newlist.ToArray();
                    }
                }
                else if (Attribute.IsDefined(prop, typeof(IgnoreCopyAttribute)))
                {
                    newlist.RemoveAll(x => x.Name == $"<{prop.Name}>k__BackingField" && x.FieldType == prop.PropertyType);
                    fields = newlist.ToArray();
                }
                else
                {
                    if (!prop.CanRead || !prop.CanWrite)
                    {
                        newlist.RemoveAll(x => x.Name == $"<{prop.Name}>k__BackingField" && x.FieldType == prop.PropertyType);
                        fields = newlist.ToArray();
                    }
                }
            }
            foreach (var field in fields) //prekopiruje všetko, private fields s CopyPropertiesAttribute/IgnoreCopyAttribute sa ignoruju lebo to nefungovalo
            {
                if (Attribute.IsDefined(field, typeof(CopyPropertiesAttribute)))
                {
                    var sourceValue = field.GetValue(source);
                    var targetValue = field.GetValue(target);
                    if (sourceValue != null && targetValue != null)
                    {
                        //spustí metódu CopyPropertiesFromX na property s týmto atribútom (rekurzia)
                        var copyMethod = typeof(ObjectExtensions).GetMethod(nameof(CopyAllPropertiesFromX), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                               .MakeGenericMethod(field.FieldType);
                        copyMethod.Invoke(null, new object[] { targetValue, sourceValue });
                    }
                }
                else if (Attribute.IsDefined(field, typeof(IgnoreCopyAttribute)))
                {
                    continue;
                }
                else
                {
                    var value = field.GetValue(source);
                    field.SetValue(target, value);
                }
            }
        }

        /// <summary>
        /// Copies all public/private + CopyPropertiesAttribute/IgnoreAttribute - properties/fields from the source object to the target object.<para/>
        /// Len hlavný 'source' je prejdený public + private, podmnožiny CopyPropertiesAttribute sú len public
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CopyAllPropertiesFromX_1level<T>(this T target, T source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);  //ziska vsetky public properties

            var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance); //ziska vsetky public + private fields

            var newlist = fields.ToList();  //odstrani property z fields lebo vo fields su vsetky jak public tak aj private fields
            foreach (var prop in properties)    //tie s atributom CopyPropertiesAttribute sa nekopiruju ale len ich vnutro
            {
                if (Attribute.IsDefined(prop, typeof(CopyPropertiesAttribute)))
                {
                    var sourceValue = prop.GetValue(source, null);
                    var targetValue = prop.GetValue(target, null);
                    if (sourceValue != null && targetValue != null)
                    {
                        //spustí metódu CopyPropertiesFromX na property s týmto atribútom (rekurzia)
                        var copyMethod = typeof(ObjectExtensions).GetMethod(nameof(CopyPropertiesFromX), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                               .MakeGenericMethod(prop.PropertyType);
                        copyMethod.Invoke(null, new object[] { targetValue, sourceValue });

                        newlist.RemoveAll(x => x.Name == $"<{prop.Name}>k__BackingField" && x.FieldType == prop.PropertyType);
                        fields = newlist.ToArray();
                    }
                }
                else if (Attribute.IsDefined(prop, typeof(IgnoreCopyAttribute)))
                {
                    newlist.RemoveAll(x => x.Name == $"<{prop.Name}>k__BackingField" && x.FieldType == prop.PropertyType);
                    fields = newlist.ToArray();
                }
                if (!prop.CanRead || !prop.CanWrite)
                {
                    newlist.RemoveAll(x => x.Name == $"<{prop.Name}>k__BackingField" && x.FieldType == prop.PropertyType);
                    fields = newlist.ToArray();
                }
            }
            foreach (var field in fields) //prekopiruje všetko, private fields s CopyPropertiesAttribute/IgnoreCopyAttribute sa ignoruju lebo to nefungovalo
            {
                if (Attribute.IsDefined(field, typeof(CopyPropertiesAttribute)))
                {
                    var sourceValue = field.GetValue(source);
                    var targetValue = field.GetValue(target);
                    if (sourceValue != null && targetValue != null)
                    {
                        //spustí metódu CopyPropertiesFromX na property s týmto atribútom (rekurzia)
                        var copyMethod = typeof(ObjectExtensions).GetMethod(nameof(CopyPropertiesFromX), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                               .MakeGenericMethod(field.FieldType);
                        copyMethod.Invoke(null, new object[] { targetValue, sourceValue });
                    }
                }
                else if (Attribute.IsDefined(field, typeof(IgnoreCopyAttribute)))
                {
                    continue;
                }
                else
                {
                    var value = field.GetValue(source);
                    field.SetValue(target, value);
                }

            }
        }

        #endregion

        
    }

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Extendovana metoda
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToStringInvariant(this DateTime dateTime, string? format)
        {
            return dateTime.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parsne string ktorý je v danom formáte na DateTime?
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime? ParseToDateTime(this string? dateTime, string? format, string? format2 = "")
        {
            if (string.IsNullOrEmpty(dateTime))
            {
                return null;
            }
            if (DateTime.TryParseExact(
                dateTime,
                format,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }
            if (!string.IsNullOrEmpty(format2) && DateTime.TryParseExact(
                dateTime,
                format2,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime result2))
            {
                return result2;
            }
            return null;
        }
    }

    public static class NumberExtensions
    {
        /// <summary>
        /// Parsne text na decimal s určitým počtom desatinných miest.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decNum"></param>
        /// <returns></returns>
        public static decimal? ParseToDecimal(this string? number, int decNum = 0)
        {
            if (number == null)
            {
                return null;
            }
            if (decimal.TryParse(number, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedValue))
            {
                return Math.Round(parsedValue, decNum);
            }
            return null;

        }
    }

    /// <summary>
    /// Atribut ak nechceme aky sa kopirovala referencia na property ale len jej vnutro. 
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class CopyPropertiesAttribute : Attribute
    {
    }

    /// <summary>
    /// Ignoruje sa pri kopirovani.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class IgnoreCopyAttribute : Attribute
    {
    }
}
