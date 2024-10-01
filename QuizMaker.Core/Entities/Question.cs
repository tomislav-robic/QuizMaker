using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizMaker.Core.Entities
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; }
        [Required]
        [StringLength(1000)]
        public string Answer { get; set; }
        // this is computed hash for question Text because Text need to be unique (no duplicate questions)
        public string HashValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }
        public virtual ICollection<QuestionTag> TagQuestions { get; set; }
    }
}
