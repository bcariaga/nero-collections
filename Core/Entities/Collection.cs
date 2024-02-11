using System.Linq;

namespace Nero.Core.Entities
{
    public class Collection
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public List<BookList>? Lists { get; set; }

        public IEnumerable<string> GetListIds() =>
            from list in Lists
            where list != null
            select list.Id;

        public BookList? GetListById(string id) =>
            Lists?.FirstOrDefault(l => l != null && l.Id == id);
    }
}