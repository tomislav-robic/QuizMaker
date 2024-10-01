namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHashToQuestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "HashValue", c => c.String(nullable: false, maxLength: 64));
            CreateIndex("dbo.Questions", "HashValue", unique: true, name: "IX_Question_HashValue");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Questions", "IX_Question_HashValue");
            DropColumn("dbo.Questions", "HashValue");
        }
    }
}
