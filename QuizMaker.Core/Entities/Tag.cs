using System.Collections.Generic;

namespace QuizMaker.Core.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Veza prema kvizovima kroz pomoćnu klasu QuizTag
        public virtual ICollection<QuizTag> QuizTags { get; set; }

        // Veza prema pitanjima kroz pomoćnu klasu TagQuestion
        public virtual ICollection<QuestionTag> TagQuestions { get; set; }
    }
}
