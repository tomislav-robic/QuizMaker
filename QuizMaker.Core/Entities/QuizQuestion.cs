﻿namespace QuizMaker.Core.Entities
{
    // Pomoćne klase za mnogostruke veze
    public class QuizQuestion
    {
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
