# DominoPontaDeQuina.WebApi

Projeto ASP.NET Core Web API que expõe, via HTTP, as operações do serviço
`IPartidaService` da camada de aplicação, protegidas por **autenticação JWT**.

> Entrega individual — **Felipe Fernandes, RM-554598**.

## Dependências configuradas

| Referência | Motivo |
| --- | --- |
| `DominoPontaDeQuina.Application` | Contratos e implementação dos casos de uso (`IPartidaService`). |
| `DominoPontaDeQuina.Infrastructure` | `DominoDbContext` e implementações dos repositórios exigidos pelo serviço. |
| `Microsoft.EntityFrameworkCore.SqlServer` | Provedor do banco escolhido em `Program.cs`. |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Validação do token nas requisições. |
| `Swashbuckle.AspNetCore` | Documentação e interface do Swagger. |

O registro no contêiner de DI acontece em `Program.cs`:

```csharp
builder.Services.AddDominoApplication();
builder.Services.AddDominoInfrastructure(options => options.UseSqlServer(connectionString));
builder.Services.AddDominoSeguranca();
```

`AddDominoApplication` sozinho não é suficiente: `PartidaService` depende dos cinco
repositórios do domínio, cujas implementações vivem na infraestrutura.

## Autenticação JWT

O fluxo é o padrão *bearer token*: a conta é cadastrada, o login devolve um JWT
assinado e as requisições seguintes mandam esse token no cabeçalho.

```
POST /api/auth/registrar   ->  cria a conta (senha vira hash)
POST /api/auth/login       ->  devolve { accessToken, expiraEm, ... }
GET  /api/partidas         ->  Authorization: Bearer <accessToken>
```

### Como as camadas se dividem

| Camada | Papel na autenticação |
| --- | --- |
| `Domain` | `Usuario` e `IUsuarioRepository` (busca por e-mail). |
| `Application` | `IAutenticacaoService` orquestra cadastro e login; depende apenas das abstrações `IHashDeSenha` e `IGeradorDeTokenJwt`. |
| `Infrastructure` | `HashDeSenhaPbkdf2`, `GeradorDeTokenJwt` e `UsuarioRepository` — onde mora a tecnologia. |
| `WebApi` | Configura o `JwtBearer`, protege os controllers e expõe `/api/auth`. |

A camada de aplicação não conhece a biblioteca de JWT: ela pede um token a
`IGeradorDeTokenJwt`. Trocar o algoritmo de assinatura, ou o formato do token,
não toca em `AutenticacaoService`.

### Senhas

Nunca são gravadas em texto puro. `HashDeSenhaPbkdf2` usa PBKDF2/HMAC-SHA256 com
210.000 iterações e um sal aleatório por senha, gravando
`pbkdf2-sha256$iteracoes$sal$hash` — os parâmetros ficam junto do valor, o que
permite aumentá-los depois sem invalidar os hashes antigos. A conferência usa
comparação em tempo fixo.

O login devolve a mesma mensagem para conta inexistente e senha errada, para não
revelar quais e-mails estão cadastrados.

### Claims do token

| Claim | Conteúdo |
| --- | --- |
| `sub` | `Usuario.Id` — é o que `/api/auth/eu` lê para identificar quem chamou. |
| `email` | E-mail da conta. |
| `jti` | Identificador único do token, base para revogação futura. |

A validação confere emissor, destinatário, assinatura e validade, com
`ClockSkew = TimeSpan.Zero` (sem a tolerância padrão de 5 minutos).

### Configuração

A seção `Jwt` do `appsettings.json` alimenta `OpcoesJwt`:

```json
"Jwt": {
  "Issuer": "DominoPontaDeQuina.WebApi",
  "Audience": "DominoPontaDeQuina.Clients",
  "ChaveSecreta": "",
  "MinutosDeExpiracao": 60
}
```

`ChaveSecreta` fica **vazia no `appsettings.json`**: quem assina os tokens de
verdade é um segredo de fora do repositório. Em desenvolvimento, o
`appsettings.Development.json` traz uma chave só para rodar local. Para os demais
ambientes, use variável de ambiente ou *user secrets*:

```bash
dotnet user-secrets set "Jwt:ChaveSecreta" "<chave com pelo menos 32 bytes>" \
  --project DominoPontaDeQuina.WebApi
# ou
export Jwt__ChaveSecreta="<chave com pelo menos 32 bytes>"
```

A aplicação valida a configuração no *startup* e **não sobe** com chave ausente
ou menor que 32 bytes, em vez de emitir tokens fracos silenciosamente.

## Endpoints

| Método | Rota | Operação | Token |
| --- | --- | --- | --- |
| `POST` | `/api/auth/registrar` | Cadastra a conta | — |
| `POST` | `/api/auth/login` | Emite o JWT | — |
| `GET` | `/api/auth/eu` | Conta do token atual | obrigatório |
| `POST` | `/api/partidas` | `IniciarPartidaAsync` | obrigatório |
| `GET` | `/api/partidas` | `ConsultarHistoricoAsync` | obrigatório |
| `GET` | `/api/partidas/{partidaId}` | `VerificarStatusAsync` | obrigatório |
| `POST` | `/api/partidas/{partidaId}/jogadores` | `RegistrarJogadorAsync` | obrigatório |
| `POST` | `/api/partidas/{partidaId}/lances` | `RegistrarLanceAsync` | obrigatório |
| `GET` | `/api/ranking` | `ConsultarRankingAsync` | — |

O ranking é a única leitura que continua pública: é um placar. Todo o resto do
jogo exige conta.

As entidades de domínio não são expostas diretamente: cada endpoint devolve um
contrato de `Contracts/Responses.cs`, o que evita os ciclos de navegação do EF Core
na serialização JSON.

## Tratamento de erros

`ApiExceptionHandler` traduz as exceções do serviço em respostas `ProblemDetails`:

| Exceção | Status HTTP |
| --- | --- |
| `KeyNotFoundException` | `404 Not Found` |
| `UnauthorizedAccessException` | `401 Unauthorized` |
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

O Swagger UI fica em `/swagger` no ambiente de desenvolvimento. O botão
**Authorize** aceita o `accessToken` devolvido pelo login (só o token, sem o
prefixo `Bearer`).

O arquivo `DominoPontaDeQuina.WebApi.http` traz a sequência pronta: cadastrar,
logar e reaproveitar o token nas demais chamadas.

Nenhuma migration nova é necessária — a tabela `Usuarios`, com índice único de
e-mail, já existe desde a `InitialCreate`.

> Em máquinas que só têm o runtime .NET 10 instalado, execute com
> `DOTNET_ROLL_FORWARD=Major`, já que a solução tem como alvo o `net8.0`.
