namespace Nero.Core.Entities
{
    public class BookState
    {
        public required string UserId { get; set; }
        public required string BookId { get; set; }
        public string? State { get; set; }
    }
}