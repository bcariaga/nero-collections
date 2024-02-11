using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Nero.Core.Entities;
using Nero.Core.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging(logging =>
{
});
//TODO: remove this connection string
var connectionString = "Server=Localhost;Database=Nero;User Id=DoruoAdminUser;Password=DoruoDoruo;";
builder.Services.AddSingleton<ICollectionService, CollectionService>();
builder.Services.AddSingleton<IUserService, UserService>();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication().AddJwtBearer(c =>
{

    var aud = builder.Configuration.GetValue<string>("Auth:Aud") ?? throw new Exception("Missing Auth:Aud");
    var iss = builder.Configuration.GetValue<string>("Auth:Iss") ?? throw new Exception("Missing Auth:Iss");
    var key = builder.Configuration.GetValue<string>("Auth:Key") ?? throw new Exception("Missing Auth:Key");
    c.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidIssuer = iss,
        ValidAudience = aud,
        RequireSignedTokens = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(5) // gap of time for valid token time. Validate with network latency
    };
    c.Events = new JwtBearerEvents()
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine(context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorizationBuilder().AddPolicy("basic_user", policy => policy.RequireAuthenticatedUser());
var app = builder.Build();
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("api/collections", async (ICollectionService service, ClaimsPrincipal user) =>
{
    var userSub = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    // return TypedResults.Ok(new { data = await service.GetAll(), user = userSub });
    return TypedResults.Ok(new { data = userSub != null ? await service.GetBy(userSub) : await service.GetAll(), user = userSub });
}).RequireAuthorization("basic_user");

app.MapGet("/me", async (IUserService userService, ClaimsPrincipal userClaims) =>
{
    var userSub = userClaims.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    if (userSub == null)
    {
        return Results.Unauthorized();
    }
    var user = await userService.GetBy(userSub);
    return TypedResults.Ok(new { username = user?.UserName });
}).RequireAuthorization("basic_user");


app.MapGet("/collections/{id}", async (string id) =>
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new NpgsqlCommand("SELECT id, name FROM collections WHERE id = @id", connection);
    command.Parameters.AddWithValue("@id", id);
    using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
    {
        return Results.NotFound();
    }

    var collection = new Collection
    {
        Id = reader["id"].ToString(),
        Name = reader["name"].ToString(),
    };

    return Results.Ok(collection);
});

app.MapPost("/collections", async (Collection collection) =>
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new NpgsqlCommand("INSERT INTO collections (id, name) VALUES (@id, @name)", connection);
    command.Parameters.AddWithValue("@id", collection.Id);
    command.Parameters.AddWithValue("@name", collection.Name);
    await command.ExecuteNonQueryAsync();

    return Results.Created($"/collections/{collection.Id}", collection);
});

app.MapPut("/collections/{id}", async (string id, Collection collection) =>
{
    if (id != collection.Id)
    {
        return Results.BadRequest();
    }

    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    // Update collection name
    using var command = new NpgsqlCommand("UPDATE collections SET name = @name WHERE id = @id", connection);
    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@name", collection.Name);
    await command.ExecuteNonQueryAsync();

    // Add new lists
    foreach (var list in collection.Lists)
    {
        using var listCommand = new NpgsqlCommand("INSERT INTO book_lists (name, collection_id) VALUES (@name, @collection_id)", connection);
        listCommand.Parameters.AddWithValue("@name", list.Name);
        listCommand.Parameters.AddWithValue("@collection_id", id);
        await listCommand.ExecuteNonQueryAsync();

        // Add new books to the new list
        foreach (var book in list.Books)
        {
            using var bookCommand = new NpgsqlCommand("INSERT INTO books (name, state, book_list_id) VALUES (@name, @state, @book_list_id)", connection);
            bookCommand.Parameters.AddWithValue("@name", book.Name);
            // bookCommand.Parameters.AddWithValue("@state", book.State);
            bookCommand.Parameters.AddWithValue("@book_list_id", list.Id);
            await bookCommand.ExecuteNonQueryAsync();
        }
    }

    return Results.NoContent();
});

app.MapPut("/book_lists/{id}", async (string id, BookList bookList) =>
{
    if (id != bookList.Id)
    {
        return Results.BadRequest();
    }

    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    // Update list name
    using var command = new NpgsqlCommand("UPDATE book_lists SET name = @name WHERE id = @id", connection);
    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@name", bookList.Name);
    await command.ExecuteNonQueryAsync();

    // Add new books to the updated list
    foreach (var book in bookList.Books)
    {
        using var bookCommand = new NpgsqlCommand("INSERT INTO books (name, state, book_list_id) VALUES (@name, @state, @book_list_id)", connection);
        bookCommand.Parameters.AddWithValue("@name", book.Name);
        // bookCommand.Parameters.AddWithValue("@state", book.State);
        bookCommand.Parameters.AddWithValue("@book_list_id", id);
        await bookCommand.ExecuteNonQueryAsync();
    }

    return Results.NoContent();
});


app.MapDelete("/collections/{id}", async (string id) =>
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    using var command = new NpgsqlCommand("DELETE FROM collections WHERE id = @id", connection);
    command.Parameters.AddWithValue("@id", id);
    await command.ExecuteNonQueryAsync();

    return Results.NoContent();
});

if (app.Environment.IsDevelopment())
{
    // add log for sql
    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    NpgsqlLoggingConfiguration.InitializeLogging(loggerFactory, parameterLoggingEnabled: true);
    IdentityModelEventSource.ShowPII = true;

}

app.Run();
