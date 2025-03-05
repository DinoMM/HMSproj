using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class IQueryableExtensions    //AI
{
    public static IQueryable<T> IncludeAll<T>(this IQueryable<T> query, DbContext context) where T : class
    {
        // Get the entity type metadata from the model
        var entityType = context.Model.FindEntityType(typeof(T));

        if (entityType == null)
            return query;

        // Get all the navigation properties for this entity
        var navigations = entityType.GetNavigations();

        foreach (var navigation in navigations)
        {
            query = query.Include(navigation.Name);
        }

        return query;
    }

    public static void IncludeForThis<T>(this T item, DbContext context) where T : class
    {
        if (item == null || context == null)
        {
            return;
        }

        var entry = context.Entry(item);
        var navigations = entry.Metadata.GetNavigations();

        foreach (var navigation in navigations)
        {
            // Check if the navigation represents a collection
            if (navigation.IsCollection)
            {
                entry.Collection(navigation.Name).Load();
            }
            else
            {
                entry.Reference(navigation.Name).Load();
            }
        }
    }
}
