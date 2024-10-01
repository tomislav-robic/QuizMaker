using QuizMaker.Core.Entities;
using System;
using System.Threading.Tasks;

namespace QuizMaker.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IQuizRepository Quizzes { get; }
        ITagRepository Tags { get; }
        IRepository<Question> Questions { get; }
        Task<int> CompleteAsync();
        ITransaction BeginTransaction();
    }
}
