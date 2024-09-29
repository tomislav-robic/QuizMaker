using System;

namespace QuizMaker.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        T GetById(int id);
    }
}
