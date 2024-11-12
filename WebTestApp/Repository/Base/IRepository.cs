using System.Linq.Expressions;
using WebTestApp.Bl;

namespace WebTestApp.Repository.Base
{
    public interface IRepository<T> where T : class
    {
        // Sync methods
        T FindById<TKey>(TKey id);
        T SelectOne(Expression<Func<T, bool>> match);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate, bool executeImmediately);
        IEnumerable<T> FindAll();
        IEnumerable<T> FindAll(params string[] agers);
        void AddOne(T myItem);
        void UpdateOne(T myItem);
        void DeleteOne(T myItem);
        void AddList(IEnumerable<T> myList);
        void UpdateList(IEnumerable<T> myList);
        void DeleteList(IEnumerable<T> myList);
        bool Exists(int id);

        // Async methods
        Task<List<StatisticsViewModel>> GetOrderStatisticsAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> FindByIdAsync<TKey>(TKey id);
        Task<T> FindByIdAsync<TKey>(TKey id, params string[] agers);
        Task<IEnumerable<T>> FindAllAsync();
        Task<IEnumerable<T>> FindAllAsync(params string[] agers);
        Task AddOneAsync(T myItem);
        Task UpdateOneAsync(T entity);
        Task DeleteOneAsync(T entity);
        Task AddListAsync(IEnumerable<T> entities);
        Task UpdateListAsync(IEnumerable<T> entities);
        Task DeleteListAsync(IEnumerable<T> entities);
        IQueryable<T> Find();
    }
}
