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

        // Veza prema pitanjima kroz pomoćnu klasu QuizQuestion
        public virtual ICollection<QuizQuestion> QuizQuestions { get; set; }

        // Veza prema tagovima kroz pomoćnu klasu QuizTag
        public virtual ICollection<QuizTag> QuizTags { get; set; }
    }

}
