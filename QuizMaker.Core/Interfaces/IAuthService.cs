namespace QuizMaker.Core.Interfaces
{
    public interface IAuthService
    {
        string Authenticate(string username, string password);
    }
}
