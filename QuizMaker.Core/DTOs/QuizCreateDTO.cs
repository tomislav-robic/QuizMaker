using System.Collections.Generic;

namespace QuizMaker.Core.DTOs
{
    public class QuizCreateDTO
    {
        public string Name { get; set; }
        public List<QuestionCreateDTO> Questions { get; set; } = new List<QuestionCreateDTO>();
    }
}
