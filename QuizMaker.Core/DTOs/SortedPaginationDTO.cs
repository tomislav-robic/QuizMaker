namespace QuizMaker.Core.DTOs
{
    public class SortedPaginationDTO
    {
        public int SortMode { get; set; } // 1 za Ascending, 2 za Descending
        public int ItemsByPage { get; set; }
        public int PageNumber { get; set; }
    }
}
