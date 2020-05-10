using System;

namespace WebApp.Data.Entities
{
	public class BaseVisitorEntity
	{
		public int Id { get; set; }
		public string CustomerId { get; set; }
		public bool IsVotes { get; set; } 
	}
}
