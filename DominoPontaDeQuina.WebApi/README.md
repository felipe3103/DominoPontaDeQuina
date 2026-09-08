# DominoPontaDeQuina.WebApi

Projeto ASP.NET Core Web API que expõe, via HTTP, as operações do serviço
`IPartidaService` da camada de aplicação.

> Entrega individual — **Felipe Fernandes, RM-554598**.

## Dependências configuradas

| Referência | Motivo |
| --- | --- |
| `DominoPontaDeQuina.Application` | Contratos e implementação dos casos de uso (`IPartidaService`). |
| `DominoPontaDeQuina.Infrastructure` | `DominoDbContext` e implementações dos repositórios exigidos pelo serviço. |
| `Microsoft.EntityFrameworkCore.SqlServer` | Provedor do banco escolhido em `Program.cs`. |
| `Swashbuckle.AspNetCore` | Documentação e interface do Swagger. |

O registro no contêiner de DI acontece em `Program.cs`:

```csharp
builder.Services.AddDominoApplication();
builder.Services.AddDominoInfrastructure(options => options.UseSqlServer(connectionString));
```

`AddDominoApplication` sozinho não é suficiente: `PartidaService` depende dos cinco
repositórios do domínio, cujas implementações vivem na infraestrutura.

## Endpoints

| Método | Rota | Operação do serviço |
| --- | --- | --- |
| `POST` | `/api/partidas` | `IniciarPartidaAsync` |
| `GET` | `/api/partidas` | `ConsultarHistoricoAsync` |
| `GET` | `/api/partidas/{partidaId}` | `VerificarStatusAsync` |
| `POST` | `/api/partidas/{partidaId}/jogadores` | `RegistrarJogadorAsync` |
| `POST` | `/api/partidas/{partidaId}/lances` | `RegistrarLanceAsync` |
| `GET` | `/api/ranking` | `ConsultarRankingAsync` |

As entidades de domínio não são expostas diretamente: cada endpoint devolve um
contrato de `Contracts/Responses.cs`, o que evita os ciclos de navegação do EF Core
na serialização JSON.

## Tratamento de erros

`ApiExceptionHandler` traduz as exceções do serviço em respostas `ProblemDetails`:

| Exceção | Status HTTP |
| --- | --- |
| `KeyNotFoundException` | `404 Not Found` |
| `ArgumentException` (inclui `ArgumentOutOfRangeException`) | `400 Bad Request` |
| `InvalidOperationException` | `409 Conflict` |

Erros de validação dos contratos de entrada são tratados pelo `[ApiController]`,
que devolve `400` com os detalhes por campo.

## Executando

O banco é configurado por `ConnectionStrings:DefaultConnection` em `appsettings.json`
(SQL Server LocalDB por padrão, o mesmo do projeto de migrations). Aplique as
migrations pelo projeto `DominoPontaDeQuina.Migrations` antes do primeiro uso.

```bash
dotnet run --project DominoPontaDeQuina.WebApi
```

O Swagger UI fica em `/swagger` no ambiente de desenvolvimento.

> Em máquinas que só têm o runtime .NET 10 instalado, execute com
> `DOTNET_ROLL_FORWARD=Major`, já que a solução tem como alvo o `net8.0`.
