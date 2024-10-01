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

        public async Task<Dictionary<string, Tag>> GetExistingTagsAsync(IEnumerable<string> tagNames)
        {
            return await _dbSet
                .Where(t => tagNames.Contains(t.Name))
                .ToDictionaryAsync(t => t.Name); 
        }

        public async Task<List<Quiz>> GetQuizzesByTagsAsync(List<string> tags, int itemsByPage, int pageNumber)
        {
            var tagEntities = await _dbSet
                .Where(t => tags.Contains(t.Name))
                .Include(t => t.QuizTags.Select(qt => qt.Quiz))
                .ToListAsync();

            return tagEntities
                .SelectMany(t => t.QuizTags)
                .Where(qt => qt.Quiz.DeletedAt == null)
                .Select(qt => qt.Quiz)
                .Distinct()
                .OrderByDescending(q => q.QuizTags.Count(qt => tags.Contains(qt.Tag.Name)))
                .Skip((pageNumber - 1) * itemsByPage)
                .Take(itemsByPage)
                .ToList();
        }
    }
}
