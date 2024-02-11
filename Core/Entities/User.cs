namespace Nero.Core.Entities
{
    public class User
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public List<Collection>? Collections { get; set; }
        public virtual List<BookState>? BookStates { get; set; }
    }
}