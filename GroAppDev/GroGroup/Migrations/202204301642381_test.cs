namespace GroGroup.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            Sql("ALTER TABLE dbo.temppds ADD manufacturer_from_home_tab VARCHAR(150) NULL;");
        }
        
        public override void Down()
        {
            Sql("ALTER TABLE dbo.temppds DROP COLUMN manufacturer_from_home_tab;");
        }
    }
}
