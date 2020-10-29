using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NRTA.Core.Models;

namespace NRTA.Core.Data
{
    public class NRTADbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=exam.db");
        }

        public DbSet<ExamQuestion> Questions { get; set; }
    }
}
