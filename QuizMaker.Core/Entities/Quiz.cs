using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizMaker.Core.Entities
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }

        public virtual ICollection<QuizTag> QuizTags { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

}
