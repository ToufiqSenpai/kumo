using Shared.Common.Middlewares;
using User.Application;
using User.Infrastructure;
using User.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Register service extensions in all layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddPresentation();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();

public partial class Program;