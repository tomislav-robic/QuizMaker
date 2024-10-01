namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDeletedAtToQuestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "DeletedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "DeletedAt");
        }
    }
}
