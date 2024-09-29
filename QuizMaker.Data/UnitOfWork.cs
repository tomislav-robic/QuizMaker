using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using QuizMaker.Data.Contexts;
using QuizMaker.Data.Repositories;

namespace QuizMaker.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuizMakerContext _context;
        public IRepository<Quiz> Quizzes { get; private set; }
        public IRepository<Tag> Tags { get; private set; }

        public UnitOfWork(QuizMakerContext context)
        {
            _context = context;
            Quizzes = new Repository<Quiz>(_context);
            Tags = new Repository<Tag>(_context);
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
