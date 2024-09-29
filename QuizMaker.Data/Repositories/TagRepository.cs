using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace QuizMaker.Data.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(DbContext context) : base(context)
        {
        }

        // Implementacija metode za dohvaćanje postojećih tagova
        public async Task<Dictionary<string, Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames)
        {
            return await _dbSet
                .Where(t => tagNames.Contains(t.Name))
                .ToDictionaryAsync(t => t.Name); // Ovdje koristi asinkronu verziju ToDictionary
        }
    }
}
