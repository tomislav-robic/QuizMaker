using QuizMaker.Core.Entities;

namespace QuizMaker.Core.Interfaces
{
    public interface IQuestionService
    {
        void PrepareQuestion(Question question);
    }
}
