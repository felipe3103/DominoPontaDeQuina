using DominoPontaDeQuina.Application;
using DominoPontaDeQuina.Infrastructure;
using DominoPontaDeQuina.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A cadeia de conexão 'DefaultConnection' não foi configurada.");

// Camada de aplicação: registra IPartidaService.
builder.Services.AddDominoApplication();

// Camada de infraestrutura: registra o DominoDbContext e os repositórios.
builder.Services.AddDominoInfrastructure(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Dominó Ponta de Quina API",
        Version = "v1",
        Description = "Endpoints das operações expostas por IPartidaService.",
        Contact = new() { Name = "Felipe Fernandes - RM-554598" }
    });

    var documentacaoXml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(documentacaoXml))
        options.IncludeXmlComments(documentacaoXml);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dominó Ponta de Quina API v1"));
}

app.MapControllers();

app.Run();
