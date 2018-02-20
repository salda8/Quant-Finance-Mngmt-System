using Common.Interfaces;
using Common.Utils;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using CommonStandard.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Server
{
    public static class ExtensionMethods
    {
        /// <summary>
        ///     Updates a collection, adding/removing values.
        ///     All newValues must already exist in the database.
        ///     Does not update the fields of each element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="newValues"></param>
        /// <param name="context"></param>
        public static void UpdateCollection<T>(this ICollection<T> collection, IEnumerable<T> newValues,
            IMyDbContext context) where T : class, IEntity
        {
            LambdaEqualityComparer<T> comparer = new LambdaEqualityComparer<T>((x, y) => x.ID == y.ID, x => x.ID);
            IEnumerable<T> enumerable = newValues as IList<T> ?? newValues.ToList();
            List<T> toAdd = enumerable.Except(collection, comparer).ToList();
            List<T> toRemove = collection.Except(enumerable, comparer).ToList();
            //grab everything so we don't need an individual query for every item
            Dictionary<int, T> allItems = context.DbContext.Set<T>().ToList().ToDictionary(x => x.ID, x => x);

            foreach (T element in toAdd)
            {
                T entry = allItems[element.ID];
                collection.Add(entry);
            }
            foreach (T element in toRemove)
            {
                T entry = allItems[element.ID];
                collection.Remove(entry);
            }
        }

        /// <summary>
        ///     Updates a collection, adding/removing values. The entities in the collection are assumed to be attached/tracked.
        ///     newValues need not already exist.
        ///     Also updates the fields of each element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="newValues"></param>
        /// <param name="context"></param>
        public static void UpdateCollectionAndElements<T>(this ICollection<T> collection, IEnumerable<T> newValues,
            IMyDbContext context) where T : class, IEntity
        {
            //remove those that need to be removed
            List<T> toRemove = collection.Where(x => newValues.All(y => y.ID != x.ID)).ToList();
            foreach (T item in toRemove)
            {
                context.DbContext.Entry(item).State=EntityState.Deleted;
            }

            //find the ones that overlap and add or update them
            foreach (T item in newValues)
            {
                if (item.ID == 0) //this means it's newly added
                    collection.Add(item);
                else //this is an existing entity, update it
                {
                    T attached = collection.FirstOrDefault(x => x.ID == item.ID);
                    //no need for Entry()/Attach() -  these are already in the ObjectStateManager
                    if (attached == null)
                        continue;
                    //if the collection on the server has been changed and the client tries to update a deleted element, you can end up in this scenario...just skip it
                    context.UpdateEntryValues(attached, item);
                }
            }
        }

        /// <summary>
        ///     Tries to get an entity from the Local collection, without hitting the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T GetLocal<T>(this IMyDbContext context, T entity) where T : class, IEntity
        {
            return entity == null ? null : context.DbContext.Set<T>().Local.FirstOrDefault(x => x.ID == entity.ID);
        }

        /// <summary>
        ///     Gets an attached entity from the ID of a detached entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T GetAttachedEntity<T>(this IMyDbContext context, T entity) where T : class, IEntity
        {
            if (entity == null) return null;

            return context.GetLocal(entity) ?? context.DbContext.Set<T>().FirstOrDefault(x => x.ID == entity.ID);
        }

        public static bool IsUniqueKeyException(this SqlException ex)
        {
            return ex.Errors.Cast<SqlError>().Any(x => x.Number == 2601 || x.Number == 2627);
        }
    }
}