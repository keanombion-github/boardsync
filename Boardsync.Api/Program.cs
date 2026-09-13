using Boardsync.Api.Common.Database;
using Boardsync.Api.Common.Extensions;
using Boardsync.Api.Common.Middleware;
using Boardsync.Api.Features.Boards.CreateBoard;
using Boardsync.Api.Features.Boards.GetBoards;
using Boardsync.Api.Features.Boards.GetBoardById;
using Boardsync.Api.Features.Columns.CreateColumn;
using Boardsync.Api.Features.Columns.DeleteColumn;
using Boardsync.Api.Features.Cards.CreateCard;
using Boardsync.Api.Features.Cards.UpdateCard;
using Boardsync.Api.Features.Cards.DeleteCard;
using Boardsync.Api.Features.Cards.MoveCard;
using Boardsync.Api.Features.Columns.ReorderColumn;

var builder = WebApplication.CreateBuilder(args);

// 1. Get Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Add services to the container
builder.Services.AddDatabase(connectionString);
builder.Services.AddValidation();

// Cors localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Register Vertical Slice Handlers
builder.Services.AddScoped<CreateBoardHandler>();
builder.Services.AddScoped<GetBoardsHandler>();
builder.Services.AddScoped<CreateColumnHandler>();
builder.Services.AddScoped<DeleteColumnHandler>();
builder.Services.AddScoped<GetBoardByIdHandler>();  
builder.Services.AddScoped<CreateCardHandler>();  
builder.Services.AddScoped<UpdateCardHandler>();
builder.Services.AddScoped<DeleteCardHandler>();
builder.Services.AddScoped<MoveCardHandler>();
builder.Services.AddScoped<ReorderColumnHandler>();

var app = builder.Build();

// 3. Run Database Migrations on startup
DatabaseMigrator.Migrate(connectionString);

// 4. Configure the HTTP request pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("LocalDev");

app.UseHttpsRedirection();

// Map Vertical Slice Endpoints
app.MapCreateBoardEndpoint();
app.MapGetBoardsEndpoint();
app.MapCreateColumnEndpoint();
app.MapDeleteColumnEndpoint();
app.MapGetBoardByIdEndpoint();
app.MapCreateCardEndpoint();
app.MapUpdateCardEndpoint();
app.MapDeleteCardEndpoint();
app.MapMoveCardEndpoint();
app.MapReorderColumnEndpoint();

app.Run();
