using System;
using System.Collections.Generic;

namespace WebApp.Data.Entities
{
	public class TagEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public ICollection<QuestionTagEntity> Questions { get; set; }
	}
}
