using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using QuizMaker.Data.Contexts;
using QuizMaker.Data.Repositories;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Transactions;

namespace QuizMaker.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuizMakerContext _context;

        public IQuizRepository Quizzes { get; private set; }
        public ITagRepository Tags { get; private set; }
        public IRepository<Question> Questions { get; private set; }

        public UnitOfWork(QuizMakerContext context)
        {
            _context = context;
            Quizzes = new QuizRepository(_context);
            Tags = new TagRepository(_context);
            Questions = new Repository<Question>(_context);
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
