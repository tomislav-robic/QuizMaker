namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDeletedAt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quizzes", "DeletedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quizzes", "DeletedAt");
        }
    }
}
