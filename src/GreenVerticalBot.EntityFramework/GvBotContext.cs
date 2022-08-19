namespace GreenVerticalBot.EntityFramework
{
    using System.Diagnostics.CodeAnalysis;
    using GreenVerticalBot.EntityFramework.Entities;
    using Microsoft.EntityFrameworkCore;

    public class GreenVerticalBotContext :
        DbContext
    {
        ///// <summary>
        ///// Строка подключения к бд
        ///// </summary>
        //public string ConnectionString { get => this.Database.GetConnectionString(); }

        public GreenVerticalBotContext([NotNull] DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<TaskEntity> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskEntity>()
                .ToTable("Tasks")
                .HasKey(e => e.Id);
        }
    }
}