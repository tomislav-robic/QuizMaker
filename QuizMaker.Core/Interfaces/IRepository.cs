using System.Threading.Tasks;

namespace QuizMaker.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        Task<T> GetByIdAsync(int id);
    }
}
