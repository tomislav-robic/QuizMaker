namespace QuizMaker.Core.DTOs
{
    public class TextSearchPaginationDTO
    {
        public string SearchText { get; set; }
        public int ItemsByPage { get; set; }
        public int PageNumber { get; set; }
    }
}
