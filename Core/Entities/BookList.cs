namespace Nero.Core.Entities
{
    public class BookList
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public List<Book>? Books { get; set; }
    }
}