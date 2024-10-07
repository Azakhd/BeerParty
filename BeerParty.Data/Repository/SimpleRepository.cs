using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Repository
{
    public class SimpleRepository<TEntity> where TEntity : class
    {
        protected readonly DbSet<TEntity> DbSet;
        protected readonly ApplicationContext Context;

        public SimpleRepository(ApplicationContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
            CheckEntityPresence();
        }

        private void CheckEntityPresence()
        {
            try
            {
                var _ = DbSet.EntityType;
            }
            catch
            {
                throw new NotImplementedException($"DbSet {typeof(TEntity).Name} is not present in the db");
            }
        }

        public virtual List<TEntity> GetAll()
        {
            return DbSet.ToList();
        }

        public virtual TEntity Add(TEntity entity)
        {
            var createdEntity = DbSet.Add(entity).Entity;
            return createdEntity;
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void SaveChanges()
        {
            Context.SaveChanges();
        }
    }
}
