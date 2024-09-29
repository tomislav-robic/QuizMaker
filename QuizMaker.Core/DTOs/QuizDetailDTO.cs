using System.Collections.Generic;
using System;

public class QuizDetailDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public List<string> Tags { get; set; }
}
