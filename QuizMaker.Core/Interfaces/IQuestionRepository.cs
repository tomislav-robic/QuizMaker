using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IQuestionRepository : IRepository<Question>
{
    Task<List<Question>> GetQuestionsByTextAsync(string searchText, int itemsByPage, int pageNumber);
    Task<List<Question>> GetQuestionsModifiedSortedAsync(int sortMode, int itemsByPage, int pageNumber);
    Task<List<Question>> GetQuestionsByTagsAsync(List<string> tags, int itemsByPage, int pageNumber);
}
