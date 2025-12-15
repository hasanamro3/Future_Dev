namespace Application.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetById(int id);
        IQueryable<T> GetAll();
        Task Insert(T entity);
        void Update(T entity);
        Task Delete(T entity);
        Task<int> SaveChanges();
    }
}