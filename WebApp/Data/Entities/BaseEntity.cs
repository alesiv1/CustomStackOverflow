using System;

namespace WebApp.Data.Entities
{
	public class BaseEntity
	{
		public int Id { get; set; }
		public DateTime Created { get; set; }
		public string Author { get; set; }
		public string Content { get; set; }
		public string AuthorId { get; set; }
	}
}
