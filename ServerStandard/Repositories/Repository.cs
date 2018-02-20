using System;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;
using Common;
using Common.Interfaces;
using CommonStandard.Interfaces;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Server.Repositories
{
    public abstract class Repository //: IRepository
    {

        protected static Logger Logger = LogManager.GetCurrentClassLogger();

        protected IQueryable<T> AsQueryable<T>(IMyDbContext context) where T : class, IEntity
        {
            return context.DbContext.Set<T>().AsQueryable();
        }

        /// <summary>
        ///     Gets all entities. If maxRows is supplied, the query will throw an exception if more than that number of rows were
        ///     to be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAll<T>(IMyDbContext context) where T : class, IEntity
        {
            return GetAllInner<T>(context);
        }

        protected IQueryable<T> GetAllInner<T>(IMyDbContext context) where T : class, IEntity
        {
            return AsQueryable<T>(context);
        }

        /// <summary>
        ///     Finds the entities matching the supplied specification. If maxRows is supplied, the query will throw an exception
        ///     if more than that number of rows were to be returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">Search condition.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<T> Find<T>(Expression<Func<T, bool>> where, IMyDbContext context) where T : class, IEntity
        {
            return FindInner(where, context);
        }

        protected virtual IQueryable<T> FindInner<T>(Expression<Func<T, bool>> where, IMyDbContext context) where T : class, IEntity
        {
            return AsQueryable<T>(context).Where(where);
        }

        /// <summary>
        ///     Returns specified single entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">Search condition.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual T SingleOrDefault<T>(Expression<Func<T, bool>> where, IMyDbContext context) where T : class, IEntity
        {
            return AsQueryable<T>(context).SingleOrDefault(where);
        }

        /// <summary>
        ///     Deletes the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="context"></param>
        public void Delete<T>(T entity, IMyDbContext context) where T : class, IEntity
        {
            
            context.DbContext.Set<T>().Remove(entity);
        }

        /// <summary>
        ///     Deletes the specified range entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeleteRange<T>(IEnumerable<T> list, IMyDbContext context) where T : class, IEntity
        {
            context.DbContext.Set<T>().RemoveRange(list);
        }

        /// <summary>
        ///     Adds the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="context"></param>
        public virtual void Add<T>(T entity, IMyDbContext context) where T : class, IEntity
        {
            context.DbContext.Set<T>().Add(entity);
        }

        /// <summary>
        ///     Updates the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="context"></param>
        public void Update<T>(T entity, IMyDbContext context) where T : class, IEntity
        {
            context.DbContext.Set<T>().Attach(entity);
        }


        /// <summary>
        ///     Check if entity exists in database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">The where.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool Exists<T>(Expression<Func<T, bool>> where, IMyDbContext context) where T : class, IEntity
        {
            return AsQueryable<T>(context).Any(where);
        }

        protected async void SaveChangesAsync(IMyDbContext context)
        {
            try
            {
                await context.DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                Logger.Debug(e, e.Message);
            }
        }

        protected void SaveChanges(IMyDbContext context)
        {
            try
            {


                context.DbContext.SaveChanges();

            }
            catch (DbUpdateConcurrencyException e)
            {
                Logger.Debug(e, e.Message);
            }
        }
    }
}