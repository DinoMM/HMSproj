using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HMSModels.Models
{
    public class FilterHtml<T>
    {
        public List<FilterCondition> Conditions { get; private set; } = new();

        public void AddCondition(string propertyName, FilterOperation operation, object value)
        {
            Conditions.Add(new FilterCondition
            {
                PropertyName = propertyName,
                Operation = operation,
                Value = value,
                //ValueType = valuetype
            });
        }

        //public static List<Func<T, bool>> GetPredicates(List<FilterCondition> filters)
        //{
        //    List<Func<T, bool>> predicates = new();
        //    foreach (var item in filters)
        //    {
        //        var itemValue = item.GetValueFromJsonElement((JsonElement?)item.Value, Type.GetType(item.ValueType ?? "") ?? typeof(T));
        //        switch (item.Operation)
        //        {
        //            case FilterOperation.Equals:
        //                predicates.Add(x => x?.GetType().GetProperty(item.PropertyName)?.GetValue(x)
        //                == itemValue);
        //                break;
        //            default: break;
        //        }
        //    }
        //    return predicates;
        //}

        public static Expression<Func<T, bool>> GetExpression(List<FilterCondition> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return x => true;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? body = null;

            foreach (var filter in filters)
            {
                var property = Expression.Property(parameter, filter.PropertyName);
                var proptype = property.Type;
                var itemValue = FilterCondition.GetValueFromJsonElement((JsonElement?)filter.Value, proptype);
                var constant = Expression.Constant(itemValue, itemValue?.GetType() ?? typeof(string));

                Expression comparison = filter.Operation switch
                {
                    FilterOperation.Equals => Expression.Equal(property, constant),
                    FilterOperation.NotEquals => Expression.NotEqual(property, constant),
                    FilterOperation.GreaterThan => Expression.GreaterThan(property, constant),
                    FilterOperation.LessThan => Expression.LessThan(property, constant),
                    FilterOperation.Contains => Expression.Call(property, "Contains", null, constant),
                    _ => throw new NotImplementedException($"Operation '{filter.Operation}' is not implemented.")
                };

                body = body == null ? comparison : Expression.AndAlso(body, comparison);
            }

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
    public enum FilterOperation
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        Contains
    }

    public class FilterCondition
    {
        public string PropertyName { get; set; } = string.Empty;
        public FilterOperation Operation { get; set; }
        public object? Value { get; set; } = null;
        //public string? ValueType { get; set; } = null;
        public static object? GetValueFromJsonElement(JsonElement? element, Type? targetType)
        {
            if (element == null)
            {
                return null;
            }
            if (targetType != null)
            {
                return JsonSerializer.Deserialize(element.Value.GetRawText(), targetType);
            }
            return element.Value.ValueKind switch
            {
                JsonValueKind.String => element.Value.GetString(),
                JsonValueKind.Number => element.Value.TryGetInt64(out long l) ? l : element.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => throw new InvalidOperationException($"Unsupported JsonValueKind: {element.Value.ValueKind}")
            };
        }
    }
}
