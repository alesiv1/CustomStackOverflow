using System;
using System.Collections.Generic;

namespace WebApp.Data.Entities
{
	public class QuestionEntity : BaseEntity
	{
		public string Title { get; set; }
		public int Views { get; set; }
		public ICollection<QuestionVisitorEntity> Visitors { get; set; }
		public ICollection<AnswerEntity> Answers { get; set; }
		public ICollection<QuestionTag> Tags { get; set; }
	}
}
