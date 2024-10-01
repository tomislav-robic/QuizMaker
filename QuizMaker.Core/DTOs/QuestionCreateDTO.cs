namespace QuizMaker.Core.DTOs
{
    public class QuestionCreateDTO
    {
        public string Text { get; set; }
        public string Answer { get; set; }
        public int? QuizId { get; set; } // Optional, can be null if the question is not linked to a quiz
    }
}
