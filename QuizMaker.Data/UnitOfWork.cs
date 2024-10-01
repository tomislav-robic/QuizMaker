using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using QuizMaker.Data.Contexts;
using QuizMaker.Data.Repositories;
using System.Threading.Tasks;

namespace QuizMaker.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuizMakerContext _context;

        public IQuizRepository Quizzes { get; private set; }
        public ITagRepository Tags { get; private set; }
        public IQuestionRepository Questions { get; private set; }

        public UnitOfWork(QuizMakerContext context)
        {
            _context = context;
            Quizzes = new QuizRepository(_context);
            Tags = new TagRepository(_context);
            Questions = new QuestionRepository(_context);
        }

        public ITransaction BeginTransaction()
        {
            return new EFTransaction(_context.Database.BeginTransaction());
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
