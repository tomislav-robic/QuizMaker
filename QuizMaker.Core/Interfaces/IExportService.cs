using QuizMaker.Core.Entities;
using System.Collections.Generic;

namespace QuizMaker.Core.Interfaces
{
    public interface IExportService
    {
        IEnumerable<IExporter> Exporters { get; }
        List<string> GetAvailableFormats();
        byte[] ExportQuiz(Quiz quiz, string format);

        IExporter GetExporter(string format);
    }
}
