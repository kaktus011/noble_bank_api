namespace NobleBank.Application.Features.Posts.Queries.GetAllPosts
{
    public record PostDto
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Body { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; }

        public DateTime UpdatedAt { get; init; }
    }
}
