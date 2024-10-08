﻿using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace QuizMaker.Data.Repositories
{
    public class QuizRepository : Repository<Quiz>, IQuizRepository
    {
        public QuizRepository(DbContext context) : base(context) { }

        public async Task<List<Quiz>> GetQuizzesNameSortedAsync(int sortMode, int itemsByPage, int pageNumber)
        {
            return sortMode == 1
                ? await _dbSet.Where(q => q.DeletedAt == null)
                              .OrderBy(q => q.Name)
                              .Skip((pageNumber - 1) * itemsByPage)
                              .Take(itemsByPage)
                              .ToListAsync()
                : await _dbSet.Where(q => q.DeletedAt == null)
                              .OrderByDescending(q => q.Name)
                              .Skip((pageNumber - 1) * itemsByPage)
                              .Take(itemsByPage)
                              .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesModifiedSortedAsync(int sortMode, int itemsByPage, int pageNumber)
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
    }
}
