using QuizMaker.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizMaker.Core.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Dictionary<string, Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames);
        Task<List<Quiz>> GetQuizzesByTagsAsync(List<string> tags, int itemsByPage, int pageNumber);
    }
}
