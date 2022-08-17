namespace GvBot.EntityFramework
{
    using System.Diagnostics.CodeAnalysis;
    using GvBot.EntityFramework.Entities;
    using Microsoft.EntityFrameworkCore;

    public class GvBotContext :
        DbContext
    {
        ///// <summary>
        ///// Строка подключения к бд
        ///// </summary>
        //public string ConnectionString { get => this.Database.GetConnectionString(); }

        public GvBotContext([NotNull] DbContextOptions options)
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