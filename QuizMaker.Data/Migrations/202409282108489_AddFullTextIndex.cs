namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFullTextIndex : DbMigration
    {
        public override void Up()
        {
            // Note: `CREATE FULLTEXT CATALOG QuizMakerFullTextCatalog AS DEFAULT;` needed to be manually executed on the database since EF does not support it.
            // Create Full-Text index on the `Text` column in the `Questions` table

            // If Full-Text index is not installed on the server, comment out this part.
            
            Sql(@"CREATE FULLTEXT INDEX ON Questions(Text) 
              KEY INDEX [PK_dbo.Questions] 
              ON QuizMakerFullTextCatalog 
              WITH STOPLIST = SYSTEM;", suppressTransaction: true);
        }
        
        public override void Down()
        {
            // Removing the Full-Text index and catalog

            // If Full-Text index is not installed on the server, comment out this part.

            Sql("DROP FULLTEXT INDEX ON Questions;");
        }
    }
}
