using QuizMaker.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizMaker.Core.Interfaces
{
    public interface IQuizRepository : IRepository<Quiz>
    {
        Task<List<Quiz>> GetQuizzesNameSortedAsync(int sortMode, int itemsByPage, int pageNumber);
        Task<List<Quiz>> GetQuizzesModifiedSortedAsync(int sortMode, int itemsByPage, int pageNumber);
        Task<List<Quiz>> GetQuizzesByTagsAsync(List<string> tags, int itemsByPage, int pageNumber);
    }
}
