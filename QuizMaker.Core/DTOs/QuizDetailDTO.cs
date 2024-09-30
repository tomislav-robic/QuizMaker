using System.Collections.Generic;
using System;

namespace QuizMaker.Core.DTOs
{
    public class QuizDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public List<QuestionSummaryDTO> Questions { get; set; }
        public List<TagSummaryDTO> Tags { get; set; }
    }
}
