using QuizMaker.Core.Entities;
using System;

namespace QuizMaker.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Quiz> Quizzes { get; }
        IRepository<Tag> Tags { get; }
        int Complete();
    }
}
