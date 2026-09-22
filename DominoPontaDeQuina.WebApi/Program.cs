using DominoPontaDeQuina.Application;
using DominoPontaDeQuina.Infrastructure;
using DominoPontaDeQuina.Infrastructure.Security;
using DominoPontaDeQuina.WebApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A cadeia de conexão 'DefaultConnection' não foi configurada.");

// Opções de emissão e validação do JWT, lidas da seção "Jwt".
var opcoesJwt = builder.Configuration.GetSection(OpcoesJwt.Secao).Get<OpcoesJwt>()
    ?? throw new InvalidOperationException($"A seção '{OpcoesJwt.Secao}' não foi configurada.");
opcoesJwt.Validar();
builder.Services.Configure<OpcoesJwt>(builder.Configuration.GetSection(OpcoesJwt.Secao));

// Camada de aplicação: registra IPartidaService e IAutenticacaoService.
builder.Services.AddDominoApplication();

// Camada de infraestrutura: registra o DominoDbContext e os repositórios.
builder.Services.AddDominoInfrastructure(options => options.UseSqlServer(connectionString));

// Hash de senha e emissor dos tokens.
builder.Services.AddDominoSeguranca();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sem o mapeamento legado, as claims chegam com o nome original do token ("sub", "email").
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = opcoesJwt.Issuer,
            ValidateAudience = true,
            ValidAudience = opcoesJwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = opcoesJwt.ChaveDeAssinatura(),
            ValidateLifetime = true,
            // Sem tolerância padrão de 5 minutos: o token expira na hora combinada.
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Email,
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

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
        Description = "Endpoints das operações expostas por IPartidaService, protegidos por autenticação JWT.",
        Contact = new() { Name = "Felipe Fernandes - RM-554598" }
    });

    // Habilita o botão "Authorize" do Swagger UI para colar o token do login.
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token devolvido por POST /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
            }
        }] = Array.Empty<string>()
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
