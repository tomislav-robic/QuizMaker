using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizMaker.Core.DTOs
{
    public class QuestionCreateDTO
    {
        public string Text { get; set; }
        public string Answer { get; set; }
        public int? QuizId { get; set; } // Opcionalno, može biti null ako pitanje nije vezano za kviz
    }
}
