using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Data.Entities;

namespace WebApp.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<QuestionTagEntity>().HasKey(sc => new { sc.QuestionId, sc.TagId });

			modelBuilder.Entity<QuestionTagEntity>()
				.HasOne<QuestionEntity>(sc => sc.Question)
				.WithMany(s => s.Tags)
				.HasForeignKey(sc => sc.QuestionId);


			modelBuilder.Entity<QuestionTagEntity>()
				.HasOne<TagEntity>(sc => sc.Tag)
				.WithMany(s => s.Questions)
				.HasForeignKey(sc => sc.TagId);
		}

		public DbSet<QuestionEntity> Questions { get; set; }
		public DbSet<AnswerEntity> Answers { get; set; }
		public DbSet<CommentEntity> Comments { get; set; }
		public DbSet<TagEntity> Tags { get; set; }
		public DbSet<QuestionTagEntity> QuestionTags { get; set; }
		public DbSet<QuestionVisitorEntity> QuestionVisitors { get; set; }
		public DbSet<AnswerVisitorEntity> AnswerVisitors { get; set; }
	}
}
