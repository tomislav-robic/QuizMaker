using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace UseCases.Services
{
    [Export(typeof(IExporter))]
    public class CSVExporter : IExporter
    {
        public string ExportFormat => "CSV";
        public string FileExtension => ".csv";

        public byte[] ExportQuiz(Quiz quiz)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Quiz: {quiz.Name}");
            sb.AppendLine();
            sb.AppendLine("Questions:");

            foreach (var question in quiz.QuizQuestions.Select(qq => qq.Question))
            {
                sb.AppendLine($"{question.Text}");
            }

            byte[] utfBytes = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            byte[] exportData = new byte[bom.Length + utfBytes.Length];
            bom.CopyTo(exportData, 0);
            utfBytes.CopyTo(exportData, bom.Length);

            return exportData;
        }
    }
}
