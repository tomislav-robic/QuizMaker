using System;

namespace QuizMaker.Core.DTOs
{
    public class QuestionSummaryDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }
    }
}
