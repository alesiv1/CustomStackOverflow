using System;

namespace WebApp.Data.Entities
{
	public class BaseVisitorEntity
	{
		public int Id { get; set; }
		public int CustomerId { get; set; }
		public bool IsVotes { get; set; } 
	}
}
