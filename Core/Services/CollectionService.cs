using Dapper;
using Nero.Core.Entities;
using Npgsql;

namespace Nero.Core.Services
{
    public class CollectionService(IConfiguration configuration) : ICollectionService
    {

        private readonly string GET_COLLECTIONS_WITH_LISTS = @"
        select
                c.id, 
                c.name,
                l.id,
                l.name
            from nero.collection c 
                left join nero.collection_list cl  
                    on c.id = cl.collection_id 
                left join nero.list l 
                    on cl.list_id = l.id;
        ";

        private readonly string GET_COLLECTIONS_WITH_LISTS_BY_USER = @"
        select
                c.id, 
                c.name,
                l.id,
                l.name
            from nero.collection c
                inner join nero.user_collection uc 
                    on uc.collection_id = c.id 
                left join nero.collection_list cl  
                    on c.id = cl.collection_id
                left join nero.list l
                    on cl.list_id = l.id
            where uc.user_id = @userId;
        ";

        private readonly string GET_BOOKS_OF_LIST = @"
            select
                    lb.list_id, 
                    b.id,
                    b.name
                from nero.list_book lb 
                    left join nero.book b 
                        on lb.book_id = b.id 
                where lb.list_id = any (@ids)
        ";

        private readonly string connString = configuration["Database:ConnectionString"] ?? throw new Exception("Database connection string not found");

        public async Task<List<Collection>> GetAll()
        {
            using var connection = await GetConnection();
            var collections = await connection.QueryAsync<Collection, BookList, Collection>(GET_COLLECTIONS_WITH_LISTS, (collection, list) =>
            {
                collection.Lists ??= [];
                if (list != null)
                {
                    collection.Lists.Add(list);
                }
                return collection;

            });
            if (collections.Any(c => c.Lists != null))
            {
                var listIds = collections.SelectMany(c => c.GetListIds()).Distinct().ToArray();
                var books = await connection.QueryAsync<BookListDto, Book, BookListDto>(GET_BOOKS_OF_LIST, (bookList, book) =>
                {
                    bookList.Book = book;
                    return bookList;
                }, new { ids = listIds });
                foreach (var book in books)
                {
                    var lists = collections
                        .Where(c => c.Lists != null)
                        .Select(c => c.GetListById(book.List_id));
                    foreach (var list in lists)
                    {
                        if (list != null)
                        {
                            list.Books ??= [];
                            list.Books.Add(book.Book);
                        }
                    }
                }
            }

            return collections.ToList();
        }

        //TODO: remove duplicate code
        public async Task<List<Collection>> GetBy(string userId)
        {
            using var connection = await GetConnection();
            var collections = await connection.QueryAsync<Collection, BookList, Collection>(GET_COLLECTIONS_WITH_LISTS_BY_USER, (collection, list) =>
            {
                collection.Lists ??= [];
                if (list != null)
                {
                    collection.Lists.Add(list);
                }
                return collection;

            }, new
            {
                userId
            });
            if (collections.Any(c => c.Lists != null))
            {
                var listIds = collections.SelectMany(c => c.GetListIds()).Distinct().ToArray();
                var books = await connection.QueryAsync<BookListDto, Book, BookListDto>(GET_BOOKS_OF_LIST, (bookList, book) =>
                {
                    bookList.Book = book;
                    return bookList;
                }, new { ids = listIds });
                foreach (var book in books)
                {
                    var lists = collections
                        .Where(c => c.Lists != null)
                        .Select(c => c.GetListById(book.List_id));
                    foreach (var list in lists)
                    {
                        if (list != null)
                        {
                            list.Books ??= [];
                            list.Books.Add(book.Book);
                        }
                    }
                }
            }

            return collections.ToList();
        }

        private async Task<NpgsqlConnection> GetConnection()
        {
            var connection = new NpgsqlConnection(connString);
            await connection.OpenAsync();
            return connection;
        }
    }

    public class BookListDto
    {
        public required string List_id { get; set; }
        public required Book Book { get; set; }
    }
}