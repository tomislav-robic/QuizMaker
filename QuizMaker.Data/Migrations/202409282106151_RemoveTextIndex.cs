namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTextIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Questions", "IX_Question_Text");
        }
        
        public override void Down()
        {
        }
    }
}
