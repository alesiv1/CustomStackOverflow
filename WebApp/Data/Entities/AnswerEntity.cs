using System;
using System.Collections.Generic;

namespace WebApp.Data.Entities
{
	public class AnswerEntity : BaseEntity
	{
		public ICollection<AnswerVisitorEntity> Visitors { get; set; }
		public ICollection<CommentEntity> Comments { get; set; }
	}
}
