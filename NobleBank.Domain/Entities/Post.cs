using System;

namespace NobleBank.Domain.Entities
{
	public class Post : BaseEntity
	{
		public string Title { get; private set; } = string.Empty;

		public string Body { get; private set; } = string.Empty;

		public string UserId { get; private set; } = string.Empty;

		public ApplicationUser User { get; private set; } = null!;

		public string? LastModifiedBy { get; private set; }

		public string? CreatedBy { get; private set; }

		private Post() { }

		public static Post Create(
			string title,
			string body,
			string userId,
			string createdBy)
		{
			return new Post
			{
				Title = title,
				Body = body,
				UserId = userId,
				CreatedBy = createdBy
			};
		}

		public void Update(string title, string body, string performedBy)
		{
			Title = title;
			Body = body;
			UpdatedAt = DateTime.UtcNow;
			LastModifiedBy = performedBy;
		}
	}
}