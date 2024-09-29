using QuizMaker.Core.Entities;
using System;
using System.Threading.Tasks;

namespace QuizMaker.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IQuizRepository Quizzes { get; }
        ITagRepository Tags { get; }
        Task<int> CompleteAsync();
    }
}
