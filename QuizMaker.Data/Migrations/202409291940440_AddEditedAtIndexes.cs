namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEditedAtIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Questions", "EditedAt", name: "IX_Question_EditedAt");
            CreateIndex("dbo.Quizzes", "EditedAt", name: "IX_Quiz_EditedAt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Quizzes", "IX_Quiz_EditedAt");
            DropIndex("dbo.Questions", "IX_Question_EditedAt");
        }
    }
}
