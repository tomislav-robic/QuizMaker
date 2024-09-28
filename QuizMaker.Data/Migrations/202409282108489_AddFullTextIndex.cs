namespace QuizMaker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFullTextIndex : DbMigration
    {
        public override void Up()
        {
            //Napomena : CREATE FULLTEXT CATALOG QuizMakerFullTextCatalog AS DEFAULT; se je moralo ručno izvršiti na bazi jer EF to ne podržava...
            // Kreiranje Full-Text indeksa na `Text` koloni u `Questions` tablici
            Sql(@"CREATE FULLTEXT INDEX ON Questions(Text) 
              KEY INDEX [PK_dbo.Questions] 
              ON QuizMakerFullTextCatalog 
              WITH STOPLIST = SYSTEM;", suppressTransaction: true);
        }
        
        public override void Down()
        {
            // Uklanjanje Full-Text indeksa i kataloga
            Sql("DROP FULLTEXT INDEX ON Questions;");
        }
    }
}
