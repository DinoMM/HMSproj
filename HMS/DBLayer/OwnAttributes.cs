using DBLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer
{
    public class DecimalNonNegativeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)  //pomoc od AI
        {
            var ContextMemberNames = new List<string>() { validationContext.MemberName ?? "" };
            if (value == null)
            {
                return new ValidationResult(ErrorMessage ?? "Null.", ContextMemberNames);

            }

            if (!(value is decimal decNum))
            {
                return new ValidationResult(ErrorMessage ?? "Zlý formát pri hodnote ID.", ContextMemberNames);
            }

            if (decNum < 0)
            {
                return new ValidationResult(ErrorMessage ?? "Len kladné hodnoty.", ContextMemberNames);
            }

            return ValidationResult.Success;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class BooleanStringValuesAttribute : Attribute  //pomoc od AI
    {
        public string TrueValue { get; }
        public string FalseValue { get; }

        public BooleanStringValuesAttribute(string falseValue, string trueValue)
        {
            FalseValue = falseValue;
            TrueValue = trueValue;
        }
    }

    public class IsForeignKeyRezervationAttribute : ValidationAttribute      //vlastny atribut pre kontrolu existujucej rezervacie. Pomoc od AI
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Rezervačné ID je potrebné.");
            }

            if (!(value is long resId))
            {
                return new ValidationResult("Zlý formát pri kontrolovaní rezervačného ID.");
            }

            var dbContext = (DataContext)validationContext.GetService(typeof(DataContext));
            if (dbContext == null)
            {
                throw new InvalidOperationException("DataContext context neni dostopný.");
            }

            if (dbContext.Rezervations.Any(u => u.Id == resId)) //kontrola existencie
            {
                return ValidationResult.Success;

            }
            return new ValidationResult("Priradená rezervácia neexistuje.");
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DisplayAndValueAttribute<TResult, TypeClass> : Attribute
    {
        public string Display { get; }
        //public Func< TResult> Value { get; }
        public bool GetID { get; }

        public DisplayAndValueAttribute(string display, bool getid = false/*, Func< TResult> value*/)
        {
            Display = display;
            GetID = getid;
            //Value = value;
        }

        public TResult? GetValue(TypeClass instance)
        {
            if (!GetID)
            {
                return default(TResult);
            }
            Type type = instance.GetType();
            MethodInfo methodInfo = type.GetMethod("GetID");

            if (methodInfo != null)
            {
                return (TResult?)methodInfo.Invoke(instance, null);
            }
            return default(TResult);
        }
    }

    //public static class AttributeHelpers
    //{
    //    public static string GetID(PolozkaSkladuConItemPoklDokladu instance)
    //    {
    //        if (instance is PolozkaSkladuConItemPoklDokladu item)
    //        {
    //            return item.GetID();
    //        }
    //        throw new ArgumentException("Invalid type", nameof(instance));
    //    }
    //}

}
