using Microsoft.EntityFrameworkCore﻿;

namespace awing_fullstack_test_backend.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<Input> Inputs { get; set; }
        public DbSet<Output> Outputs { get; set; }
        public DbSet<MatrixElement> MatrixElements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1-n
            modelBuilder.Entity<Input>()
                .HasMany(i => i.MatrixElements)
                .WithOne(me => me.Input)
                .HasForeignKey(me => me.InputId);
            // 1-1
            modelBuilder.Entity<Input>()
                .HasOne(i => i.Output)
                .WithOne(o => o.Input)
                .HasForeignKey<Output>(o => o.InputId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
