using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizMaker.Core.Entities
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public virtual ICollection<QuizTag> QuizTags { get; set; }

        public virtual ICollection<QuestionTag> TagQuestions { get; set; }
    }
}
