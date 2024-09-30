using System.Collections.Generic;
using System;

namespace QuizMaker.Core.DTOs
{
    public class QuestionDetailDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }
        public List<TagSummaryDTO> Tags { get; set; } 
        public List<QuizSummaryDTO> RelatedQuizzes { get; set; } 
    }
}
