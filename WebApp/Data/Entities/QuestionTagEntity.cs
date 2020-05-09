using System;

namespace WebApp.Data.Entities
{
	public class QuestionTagEntity
	{
		public int QuestionId { get; set; }
		public QuestionEntity Question { get; set; }

		public int TagId { get; set; }
		public TagEntity Tag { get; set; }
	}
}
