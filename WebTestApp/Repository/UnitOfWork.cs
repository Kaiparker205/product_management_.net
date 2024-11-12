using WebTestApp.Data;
using WebTestApp.Models;
using WebTestApp.Repository.Base;

namespace WebTestApp.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(ProductContext context)
        {
            _context = context;
            employers = new MainRepository<Employer>(_context);
            products = new MainRepository<Product>(_context);
            orders = new MainRepository<Order>(_context);

        }

        private readonly ProductContext _context;

        public IRepository<Product> products { get; private set; }

        public IRepository<Employer> employers { get; private set; }

        public IRepository<Order> orders { get; private set; }



        public int CommitChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
