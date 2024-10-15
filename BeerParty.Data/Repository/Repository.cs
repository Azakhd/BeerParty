using BeerParty.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerParty.Data.Repository
{
    public class Repository<TEntity> : SimpleRepository<TEntity>
    where TEntity : BaseEntity
    {
        public Repository(ApplicationContext context) : base(context)
        {
        }

        public virtual TEntity? GetById(long id)
        {
            return DbSet.Find(id);
        }

        public virtual void Delete(long id)
        {
            var entity = GetById(id);
            if (entity == null)
            {
                throw new NotImplementedException($"Entity with id {id} not found");
            }
            DbSet.Remove(entity);
        }
    }
}
