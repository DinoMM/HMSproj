using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UniComponents
{
    public interface IModal
    {
        public Task<bool> OpenModal(bool stop = false);
        public void UpdateText(string newText);
    }

    public class StringPropertyInfo : System.Reflection.PropertyInfo
    {
        private readonly string _name;
        private readonly string _value;

        public StringPropertyInfo(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public override string Name => _name;

        public override Type PropertyType => typeof(string);

        public override object GetValue(object obj, object[] index)
        {
            return _value;
        }
        #region hide
        // Other members of PropertyInfo can throw NotImplementedException
        public override Type DeclaringType => throw new NotImplementedException();
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override PropertyAttributes Attributes => throw new NotImplementedException();

        public override Type? ReflectedType => throw new NotImplementedException();

        public override void SetValue(object obj, object value, object[] index) => throw new NotImplementedException();

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo? GetGetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            throw new NotImplementedException();
        }

        public override MethodInfo? GetSetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
        public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
