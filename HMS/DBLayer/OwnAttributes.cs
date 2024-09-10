using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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


}
