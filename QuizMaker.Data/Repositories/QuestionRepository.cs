using QuizMaker.Core.Entities;
using QuizMaker.Data.Repositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;

public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(DbContext context) : base(context) { }

    // Full-Text pretraga po tekstu s paginacijom
    public async Task<List<Question>> GetQuestionsByTextAsync(string searchText, int itemsByPage, int pageNumber)
    {
        return await _dbSet
            .Where(q => q.DeletedAt == null && q.Text.Contains(searchText))  // Full-Text pretraga
            .OrderBy(q => q.Text)  // Sortiraj ako je potrebno po Text
            .Skip((pageNumber - 1) * itemsByPage)
            .Take(itemsByPage)
            .ToListAsync();
    }

    // Dohvati pitanja sortirana prema datumu izmjene (EditedAt)
    public async Task<List<Question>> GetQuestionsModifiedSortedAsync(int sortMode, int itemsByPage, int pageNumber)
    {
        return sortMode == 1
            ? await _dbSet.Where(q => q.DeletedAt == null)
                          .OrderBy(q => q.EditedAt)
                          .Skip((pageNumber - 1) * itemsByPage)
                          .Take(itemsByPage)
                          .ToListAsync()
            : await _dbSet.Where(q => q.DeletedAt == null)
                          .OrderByDescending(q => q.EditedAt)
                          .Skip((pageNumber - 1) * itemsByPage)
                          .Take(itemsByPage)
                          .ToListAsync();
    }

    // Dohvati pitanja prema tagovima s paginacijom
    public async Task<List<Question>> GetQuestionsByTagsAsync(List<string> tags, int itemsByPage, int pageNumber)
    {
        var tagEntities = await _context.Set<Tag>()
            .Where(t => tags.Contains(t.Name))
            .Include(t => t.TagQuestions.Select(tq => tq.Question))
            .ToListAsync();

        return tagEntities
            .SelectMany(t => t.TagQuestions)
            .Where(tq => tq.Question.DeletedAt == null)
            .Select(tq => tq.Question)
            .Distinct()
            .Skip((pageNumber - 1) * itemsByPage)
            .Take(itemsByPage)
            .ToList();
    }
}
