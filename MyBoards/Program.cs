using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBoards.Dto;
using MyBoards.Entities;
using MyBoards.Sieve;
using Sieve.Models;
using Sieve.Services;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


builder.Services.AddDbContext<MyBoardsContext>(
        option => option
        //.UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsConnectionString"))
        );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<MyBoardsContext>();

var pendingMigrations = dbContext.Database.GetPendingMigrations();
if (pendingMigrations.Any())
{
    dbContext.Database.Migrate();
}

var users = dbContext.Users.ToList();
if (!users.Any())
{
    var user1 = new User()
    {
        Email = "user1@test.com",
        FullName = "User One",
        Address = new Address()
        {
            City = "Warszawa",
            Street = "Szeroka"
        }
    };
    var user2 = new User()
    {
        Email = "user2@test.com",
        FullName = "User Two",
        Address = new Address()
        {
            City = "Krakow",
            Street = "D³uga"
        }
    };

    dbContext.Users.AddRange(user1, user2);
    dbContext.SaveChanges();
}


app.MapGet("pagination", async (MyBoardsContext db) =>
{
    //user input
    var filter = "a";
    string sortBy = "FullName";
    bool sortByDescending = false;
    int pageNumber = 1;
    int pageSize = 10;
    //

    var query = db.Users
        .Where(u => filter == null || (u.Email.ToLower().Contains(filter.ToLower()) || u.FullName.ToLower().Contains(filter.ToLower())));

    var totalCount = query.Count();

    if(sortBy != null)
    {
        var columnSelector = new Dictionary<string, Expression<Func<User, object>>>
        {
            {nameof(User.Email), user => user.Email},
            {nameof(User.FullName), user => user.FullName}
        };

        var sortByExpression = columnSelector[sortBy];

        query = sortByDescending ? query.OrderByDescending(sortByExpression) : query.OrderBy(sortByExpression);
    }

    var result = query.Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToList();

    var pagedResult = new PagedResult<User>(result, totalCount, pageSize, pageNumber);

    return pagedResult;

});

app.MapGet("data", async (MyBoardsContext db) =>
{
    var usersComments = await db.Users
                .Include(u => u.Address)
                .Include(u => u.Comments)
                .Where(u => u.Address.Country == "Albania")
                .SelectMany(u => u.Comments)
                .Select(c => c.Message)
                .ToListAsync();



    return usersComments;
});

app.MapPost("update", async (MyBoardsContext db) =>
{
    var epic = await db.Epic.FirstAsync(epic => epic.Id == 1);

    var rejectedState = await db.States.FirstAsync(a => a.Value == "Rejected");

    epic.State = rejectedState;


    await db.SaveChangesAsync();
    return epic;

});
app.MapPost("create", async (MyBoardsContext db) =>
{
    Tag mvcTag = new Tag()
    {
        Value = "MVC"
    };
    Tag aspTag = new Tag()
    {
        Value = "ASP"
    };

    //await db.AddAsync(tag);
    await db.Tags.AddRangeAsync(mvcTag, aspTag);
    await db.SaveChangesAsync();
});

app.MapDelete("delete", async (MyBoardsContext db) =>
{
    var user = await db.Users
    .Include(u => u.Comments)
    .FirstAsync(u => u.Id == Guid.Parse("2073271A-3DFC-4A63-CBE5-08DA10AB0E61"));

    db.Users.Remove(user);

    await db.SaveChangesAsync();
});

app.MapPost("sieve", async ([FromBody] SieveModel query, ISieveProcessor sieveProcessor, MyBoardsContext db) =>
{
    var epics = db.Epic
        .Include(e => e.Author)
        .AsQueryable();

    var dtos = await sieveProcessor
        .Apply(query, epics)
        .Select(e => new EpicDto()
        {
            Id = e.Id,
            Area = e.Area,
            Priority = e.Priority,
            StartDate = e.StartDate,
            AuthorFullName = e.Author.FullName
        })
        .ToListAsync();

    var totalCount = await sieveProcessor
        .Apply(query, epics, applyPagination: false, applySorting: false)
        .CountAsync();

    var result = new PagedResult<EpicDto>(dtos, totalCount, query.PageSize.Value, query.Page.Value);
    
    return result;
});

app.Run();
