using Boardsync.Api.Common.Database;
using Boardsync.Api.Common.Extensions;
using Boardsync.Api.Common.Middleware;
using Boardsync.Api.Features.Boards.CreateBoard;
using Boardsync.Api.Features.Boards.GetBoards;

var builder = WebApplication.CreateBuilder(args);

// 1. Get Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Add services to the container
builder.Services.AddDatabase(connectionString);
builder.Services.AddValidation();
// Register Vertical Slice Handlers
builder.Services.AddScoped<CreateBoardHandler>();
builder.Services.AddScoped<GetBoardsHandler>();

var app = builder.Build();

// 3. Run Database Migrations on startup
DatabaseMigrator.Migrate(connectionString);

// 4. Configure the HTTP request pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

// Map Vertical Slice Endpoints
app.MapCreateBoardEndpoint();
app.MapGetBoardsEndpoint();

app.Run();
