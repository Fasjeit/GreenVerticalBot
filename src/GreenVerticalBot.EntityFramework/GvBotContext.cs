namespace GreenVerticalBot.EntityFramework
{
    using GreenVerticalBot.EntityFramework.Entities;
    using GreenVerticalBot.EntityFramework.Entities.Tasks;
    using Microsoft.EntityFrameworkCore;
    using System.Diagnostics.CodeAnalysis;

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

            modelBuilder.Entity<UserEntity>()
                .ToTable("Users")
                .HasKey(e => e.Id);
        }
    }
}