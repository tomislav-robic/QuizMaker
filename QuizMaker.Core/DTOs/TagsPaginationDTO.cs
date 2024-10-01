namespace QuizMaker.Core.DTOs
{
    public class TagsPaginationDTO
    {
        public string Tags { get; set; } 
        public int ItemsByPage { get; set; }
        public int PageNumber { get; set; }
    }

}
