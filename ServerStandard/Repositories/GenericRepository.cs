using System;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using CommonStandard.Interfaces;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using NLog;

namespace Server.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        internal IMyDbContext context;
        private DbContext myDbContext;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(IMyDbContext context)
        {
            this.context = context;
            this.myDbContext = context.DbContext;
            this.dbSet = myDbContext.Set<TEntity>();
        }

        public virtual Task<List<TEntity>> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                     .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return orderBy != null ? orderBy(query).ToListAsync() : query.ToListAsync();
        }

        public virtual Task<TEntity> GetByID(object id)
        {
            return dbSet.FindAsync(id);
        }

        public virtual Task<TEntity> Insert(TEntity entity)
        {
            var add = dbSet.Add(entity);
            context.DbContext.SaveChanges();
            
            return Task.Run(()=>entity);

        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
            context.DbContext.SaveChanges();
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.DbContext.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
            context.DbContext.SaveChanges();
        }

        public virtual Task<TEntity> Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            myDbContext.Entry(entityToUpdate).State = EntityState.Modified;
            context.DbContext.SaveChanges();
            return Task.Run(() => entityToUpdate);
        }
    }
}