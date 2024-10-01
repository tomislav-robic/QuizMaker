namespace QuizMaker.Core.DTOs
{
    public class SortedPaginationDTO
    {
        public int SortMode { get; set; } // 1 for Ascending, 2 for Descending
        public int ItemsByPage { get; set; }
        public int PageNumber { get; set; }
    }
}
