using QuizMaker.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizMaker.Core.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        // Metoda za dohvaćanje postojećih tagova po imenima
        Task<Dictionary<string, Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames);
    }
}
