using WebTestApp.Models;

namespace WebTestApp.Repository.Base
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Product> products { get; }
        IRepository<Employer> employers { get; }
        IRepository<Order> orders { get; }

        int CommitChanges();
    }
}
