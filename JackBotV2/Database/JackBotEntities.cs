using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JackBotV2.Database
{
    public partial class JackBotEntities : DbContext // inherit DbContext
    {
        public virtual DbSet<JackBotQuotes> JackBotQuotes { get; set; } // Use key from JackBotQuotes

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = @"C:\Users\camer\source\repos\JackBotV2\JackBotV2\JackBotV2.db" }; // Find database to build connection
            var connectionString = connectionStringBuilder.ToString(); // create connection string with datasource
            var connection = new SqliteConnection(connectionString); // Generate connection
            optionsBuilder.UseSqlite(connection); // Genererate options using the connection
        }
    }
}
