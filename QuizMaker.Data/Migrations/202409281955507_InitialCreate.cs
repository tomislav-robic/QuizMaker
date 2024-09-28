namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false, maxLength: 1000),
                        CreatedAt = c.DateTime(nullable: false),
                        EditedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Text, unique: true, name: "IX_Question_Text");
            
            CreateTable(
                "dbo.QuizQuestions",
                c => new
                    {
                        QuizId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.QuizId, t.QuestionId })
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.Quizzes", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.Quizzes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        CreatedAt = c.DateTime(nullable: false),
                        EditedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "IX_Quiz_Name");
            
            CreateTable(
                "dbo.QuizTags",
                c => new
                    {
                        QuizId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.QuizId, t.TagId })
                .ForeignKey("dbo.Quizzes", t => t.QuizId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .Index(t => t.QuizId)
                .Index(t => t.TagId);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "IX_Tag_Name");
            
            CreateTable(
                "dbo.QuestionTags",
                c => new
                    {
                        QuestionId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.QuestionId, t.TagId })
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .Index(t => t.QuestionId)
                .Index(t => t.TagId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizQuestions", "QuizId", "dbo.Quizzes");
            DropForeignKey("dbo.QuizTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.QuestionTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.QuestionTags", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.QuizTags", "QuizId", "dbo.Quizzes");
            DropForeignKey("dbo.QuizQuestions", "QuestionId", "dbo.Questions");
            DropIndex("dbo.QuestionTags", new[] { "TagId" });
            DropIndex("dbo.QuestionTags", new[] { "QuestionId" });
            DropIndex("dbo.Tags", "IX_Tag_Name");
            DropIndex("dbo.QuizTags", new[] { "TagId" });
            DropIndex("dbo.QuizTags", new[] { "QuizId" });
            DropIndex("dbo.Quizzes", "IX_Quiz_Name");
            DropIndex("dbo.QuizQuestions", new[] { "QuestionId" });
            DropIndex("dbo.QuizQuestions", new[] { "QuizId" });
            DropIndex("dbo.Questions", "IX_Question_Text");
            DropTable("dbo.QuestionTags");
            DropTable("dbo.Tags");
            DropTable("dbo.QuizTags");
            DropTable("dbo.Quizzes");
            DropTable("dbo.QuizQuestions");
            DropTable("dbo.Questions");
        }
    }
}
