using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuizMaker.Core.Entities
{
    public class QuizTag
    {
        [Key, Column(Order = 0)]
        public int QuizId { get; set; }

        [Key, Column(Order = 1)]
        public int TagId { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}
