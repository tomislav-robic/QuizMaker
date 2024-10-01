using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UseCases.Services
{
    public class QuestionService : IQuestionService
    {
        public void PrepareQuestion(Question question)
        {
            if (question == null || string.IsNullOrEmpty(question.Text))
                throw new ArgumentNullException("Question or its Text cannot be null.");

            question.HashValue = GenerateSHA256Hash(question.Text);
        }

        private string GenerateSHA256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
