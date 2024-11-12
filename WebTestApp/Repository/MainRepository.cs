using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebTestApp.Bl;
using WebTestApp.Data;
using WebTestApp.Repository.Base;

namespace WebTestApp.Repository
{
    public class MainRepository<T> : IRepository<T> where T : class
    {
        protected readonly ProductContext context;

        public MainRepository(ProductContext context)
        {
            this.context = context;
        }

        // Parameterless Find method implementation
        public IQueryable<T> Find()
        {
            return context.Set<T>();
        }

        // Find method with a predicate
        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return context.Set<T>().Where(predicate);
        }

        // Overloaded Find method for immediate execution (returns List<T>)
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate, bool executeImmediately)
        {
            return context.Set<T>().Where(predicate).ToList();
        }

        // Find by Id with support for int or string key types
        public T FindById<TKey>(TKey id)
        {
            IQueryable<T> query = context.Set<T>();
            if (typeof(TKey) == typeof(int))
            {
                return query.SingleOrDefault(e => EF.Property<int>(e, "Id") == (int)(object)id);
            }
            else if (typeof(TKey) == typeof(string))
            {
                return query.SingleOrDefault(e => EF.Property<string>(e, "Id") == (string)(object)id);
            }
            else
            {
                throw new ArgumentException("Unsupported key type");
            }
        }

        public T SelectOne(Expression<Func<T, bool>> match)
        {
            return context.Set<T>().SingleOrDefault(match);
        }

        public IEnumerable<T> FindAll()
        {
            return context.Set<T>().ToList();
        }

        public IEnumerable<T> FindAll(params string[] agers)
        {
            IQueryable<T> query = context.Set<T>();
            foreach (var ager in agers)
            {
                query = query.Include(ager);
            }
            return query.ToList();
        }

        public void AddOne(T myItem)
        {
            context.Set<T>().Add(myItem);
            context.SaveChanges();
        }

        public void UpdateOne(T myItem)
        {
            context.Set<T>().Update(myItem);
            context.SaveChanges();
        }

        public void DeleteOne(T myItem)
        {
            context.Set<T>().Remove(myItem);
            context.SaveChanges();
        }

        public void AddList(IEnumerable<T> myList)
        {
            context.Set<T>().AddRange(myList);
            context.SaveChanges();
        }

        public void UpdateList(IEnumerable<T> myList)
        {
            context.Set<T>().UpdateRange(myList);
            context.SaveChanges();
        }

        public void DeleteList(IEnumerable<T> myList)
        {
            context.Set<T>().RemoveRange(myList);
            context.SaveChanges();
        }

        // Async methods

        public async Task<List<StatisticsViewModel>> GetOrderStatisticsAsync()
        {
            var orders = await context.Orders
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(o => (decimal)(o.Price * o.Quantity)),
                    TotalQuantity = g.Sum(o => o.Quantity)
                })
                .OrderBy(stat => stat.Date)
                .ToListAsync();

            var viewModel = new StatisticsViewModel
            {
                Labels = orders.Select(o => o.Date.ToString("yyyy-MM-dd")).ToList(),
                TotalRevenue = orders.Select(o => o.TotalRevenue).ToList(),
                TotalQuantity = orders.Select(o => o.TotalQuantity).ToList()
            };

            return new List<StatisticsViewModel> { viewModel };
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<T> FindByIdAsync<TKey>(TKey id)
        {
            IQueryable<T> query = context.Set<T>();
            if (typeof(TKey) == typeof(int))
            {
                return await query.SingleOrDefaultAsync(e => EF.Property<int>(e, "Id") == (int)(object)id);
            }
            else if (typeof(TKey) == typeof(string))
            {
                return await query.SingleOrDefaultAsync(e => EF.Property<string>(e, "Id") == (string)(object)id);
            }
            else
            {
                throw new ArgumentException("Unsupported key type");
            }
        }

        public async Task<T> FindByIdAsync<TKey>(TKey id, params string[] agers)
        {
            IQueryable<T> query = context.Set<T>();
            foreach (var ager in agers)
            {
                query = query.Include(ager);
            }

            if (typeof(TKey) == typeof(int))
            {
                return await query.SingleOrDefaultAsync(e => EF.Property<int>(e, "Id") == (int)(object)id);
            }
            else if (typeof(TKey) == typeof(string))
            {
                return await query.SingleOrDefaultAsync(e => EF.Property<string>(e, "Id") == (string)(object)id);
            }
            else
            {
                throw new ArgumentException("Unsupported key type");
            }
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            return await context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAllAsync(params string[] agers)
        {
            IQueryable<T> query = context.Set<T>();
            foreach (var ager in agers)
            {
                query = query.Include(ager);
            }
            return await query.ToListAsync();
        }

        public async Task AddOneAsync(T myItem)
        {
            await context.Set<T>().AddAsync(myItem);
            await context.SaveChangesAsync();
        }

        public async Task UpdateOneAsync(T entity)
        {
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteOneAsync(T entity)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task AddListAsync(IEnumerable<T> entities)
        {
            await context.Set<T>().AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        public async Task UpdateListAsync(IEnumerable<T> entities)
        {
            context.Set<T>().UpdateRange(entities);
            await context.SaveChangesAsync();
        }

        public async Task DeleteListAsync(IEnumerable<T> entities)
        {
            context.Set<T>().RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        public bool Exists(int id)
        {
            return context.Set<T>().Any(e => EF.Property<int>(e, "Id") == id);
        }
    }
}
