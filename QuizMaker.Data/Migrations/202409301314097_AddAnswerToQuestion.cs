namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAnswerToQuestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "Answer", c => c.String(nullable: false, maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "Answer");
        }
    }
}
