using System;
using System.Collections.Generic;

namespace QuizMaker.Core.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;

        // Veza prema kvizovima kroz pomoćnu klasu QuizQuestion
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }

        // Veza prema tagovima kroz pomoćnu klasu TagQuestion
        public virtual ICollection<QuestionTag> TagQuestions { get; set; }
    }
}
