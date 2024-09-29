using System;
using System.Linq.Expressions;

namespace QuizMaker.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        T GetById(int id);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
    }
}
