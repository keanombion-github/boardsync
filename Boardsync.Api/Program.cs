using Boardsync.Api.Common.Database;
using Boardsync.Api.Common.Extensions;
using Boardsync.Api.Common.Middleware;
using Boardsync.Api.Features.Boards.CreateBoard;
using Boardsync.Api.Features.Boards.GetBoards;
using Boardsync.Api.Features.Boards.GetBoardById;
using Boardsync.Api.Features.Boards.UpdateBoard;
using Boardsync.Api.Features.Boards.DeleteBoard;
using Boardsync.Api.Features.Columns.CreateColumn;
using Boardsync.Api.Features.Columns.DeleteColumn;
using Boardsync.Api.Features.Cards.CreateCard;
using Boardsync.Api.Features.Cards.UpdateCard;
using Boardsync.Api.Features.Cards.DeleteCard;
using Boardsync.Api.Features.Cards.MoveCard;
using Boardsync.Api.Features.Cards.CardActivity;
using Boardsync.Api.Features.Cards.AssignCard;
using Boardsync.Api.Features.Boards.Members;
using Boardsync.Api.Features.Columns.ReorderColumn;
using Boardsync.Api.Features.Auth.Register;
using Boardsync.Api.Features.Auth.Login;
using Boardsync.Api.Features.Auth.Refresh;
using Boardsync.Api.Features.Auth.Logout;
using Boardsync.Api.Features.Auth.GetCurrentUser;
using Boardsync.Api.Common.Health;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Local development uses ConnectionStrings:DefaultConnection. Render supplies DATABASE_URL.
// Resolve both formats once at the application boundary so handlers stay platform-agnostic.
var connectionString = DatabaseConnectionString.Resolve(builder.Configuration);

// 2. Add services to the container
builder.Services.AddDatabase(connectionString);
builder.Services.AddValidation();
builder.Services.AddBoardsyncAuthentication(builder.Configuration);
builder.Services.AddScoped<
    IPasswordHasher<string>,
    PasswordHasher<string>>();

var frontendOrigin = builder.Configuration["FrontendOrigin"]
    ?? throw new InvalidOperationException(
        "FrontendOrigin configuration is required.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(frontendOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// Register Vertical Slice Handlers
builder.Services.AddScoped<CreateBoardHandler>();
builder.Services.AddScoped<GetBoardsHandler>();
builder.Services.AddScoped<CreateColumnHandler>();
builder.Services.AddScoped<DeleteColumnHandler>();
builder.Services.AddScoped<GetBoardByIdHandler>();  
builder.Services.AddScoped<UpdateBoardHandler>();
builder.Services.AddScoped<DeleteBoardHandler>();
builder.Services.AddScoped<CreateCardHandler>();  
builder.Services.AddScoped<UpdateCardHandler>();
builder.Services.AddScoped<DeleteCardHandler>();
builder.Services.AddScoped<MoveCardHandler>();
builder.Services.AddScoped<CardActivityHandler>();
builder.Services.AddScoped<AssignCardHandler>();
builder.Services.AddScoped<BoardMembersHandler>();
builder.Services.AddScoped<ReorderColumnHandler>();
builder.Services.AddScoped<RegisterHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RefreshHandler>();
builder.Services.AddScoped<LogoutHandler>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("postgresql");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(
            Boardsync.Api.Common.Models.ApiResponse<object>.Fail(
                "RATE_LIMITED",
                "Too many requests. Please try again shortly."),
            cancellationToken);
    };

    options.AddPolicy("auth-sensitive", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth-session", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

// 3. Run Database Migrations on startup
DatabaseMigrator.Migrate(connectionString);

// 4. Configure the HTTP request pipeline
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseRouting();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Map Vertical Slice Endpoints
app.MapCreateBoardEndpoint();
app.MapGetBoardsEndpoint();
app.MapCreateColumnEndpoint();
app.MapDeleteColumnEndpoint();
app.MapGetBoardByIdEndpoint();
app.MapUpdateBoardEndpoint();
app.MapDeleteBoardEndpoint();
app.MapCreateCardEndpoint();
app.MapUpdateCardEndpoint();
app.MapDeleteCardEndpoint();
app.MapMoveCardEndpoint();
app.MapCardActivityEndpoints();
app.MapAssignCardEndpoint();
app.MapBoardMembersEndpoints();
app.MapReorderColumnEndpoint();
app.MapRegisterEndpoint();
app.MapLoginEndpoint();
app.MapRefreshEndpoint();
app.MapLogoutEndpoint();
app.MapGetCurrentUserEndpoint();
app.MapHealthChecks("/health").AllowAnonymous();



app.Run();
