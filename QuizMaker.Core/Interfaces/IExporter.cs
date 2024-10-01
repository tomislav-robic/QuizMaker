namespace QuizMaker.Core.Interfaces
{
    public interface IExporter
    {
        string ExportFormat { get; } 
        string FileExtension { get; } 
        byte[] ExportQuiz(Entities.Quiz quiz); 
    }
}
