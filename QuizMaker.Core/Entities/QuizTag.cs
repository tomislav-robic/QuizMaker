namespace QuizMaker.Core.Entities
{
    public class QuizTag
    {
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
