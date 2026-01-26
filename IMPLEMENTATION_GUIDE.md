# Guia de Implementação - Novas Funcionalidades

Este documento explica como usar as novas funcionalidades implementadas no projeto.

## 1. Health Checks

### Endpoints Disponíveis

- **GET /health** - Health check completo de todos os serviços
- **GET /health/ready** - Verifica se a aplicação está pronta para receber tráfego
- **GET /health/live** - Verifica se a aplicação está viva
- **GET /health-ui** - Interface visual dos health checks (apenas em Development)

### Serviços Monitorados

- PostgreSQL (banco de dados principal)
- MongoDB (banco de auditoria)
- Redis (cache, se habilitado)

### Uso

```bash
# Verificar saúde da aplicação
curl http://localhost:5000/health

# Em Kubernetes, configurar liveness e readiness probes:
# livenessProbe:
#   httpGet:
#     path: /health/live
#     port: 8080
# readinessProbe:
#   httpGet:
#     path: /health/ready
#     port: 8080
```

---

## 2. Hangfire (Background Jobs)

### Configuração

Hangfire está configurado com armazenamento em memória (in-memory). Para produção, considere usar PostgreSQL ou SQL Server.

### Dashboard

Acesse o dashboard em: `http://localhost:5000/hangfire` (apenas em Development)

### Criando Jobs

#### Job Recorrente

```csharp
// No Program.cs, após inicialização:
RecurringJob.AddOrUpdate(
    "job-id", 
    () => service.Method(), 
    Cron.Minutely // ou "0 */5 * * *" para a cada 5 minutos
);
```

#### Job com Delay

```csharp
BackgroundJob.Schedule(
    () => service.ProcessDelayedJob("data"),
    TimeSpan.FromMinutes(30)
);
```

#### Job Fire-and-Forget

```csharp
BackgroundJob.Enqueue(() => service.ProcessSomething());
```

### Exemplo de Job

Veja `src/MultiTenantApp.Infrastructure/Jobs/SampleRecurringJob.cs` para um exemplo completo.

---

## 3. FluentValidation

### Validators Implementados

- `CreateProductDtoValidator`
- `UpdateProductDtoValidator`
- `LoginDtoValidator`

### Criando Novos Validators

```csharp
public class MyDtoValidator : AbstractValidator<MyDto>
{
    public MyDtoValidator()
    {
        RuleFor(x => x.Property)
            .NotEmpty().WithMessage("Property is required.")
            .MaximumLength(100).WithMessage("Property must not exceed 100 characters.");
    }
}
```

### Validação Automática

A validação é executada automaticamente nos controllers. Se a validação falhar, retorna `400 Bad Request` com os erros.

### Validação Assíncrona

```csharp
RuleFor(x => x.Email)
    .MustAsync(async (email, cancellation) => 
    {
        // Verificar se email já existe no banco
        return !await _repository.ExistsAsync(email);
    })
    .WithMessage("Email already exists.");
```

---

## 4. Request/Response Logging

### Atributo LogRequestResponse

Use o atributo `[LogRequestResponse]` em controllers ou actions para logar requisições/respostas no MongoDB.

### Exemplo de Uso

```csharp
[HttpPost("create")]
[LogRequestResponse] // Loga request e response
public async Task<IActionResult> Create([FromBody] CreateDto model)
{
    // ...
}

[HttpPost("login")]
[LogRequestResponse(LogRequestBody = true, LogResponseBody = false)] // Não loga response (contém token)
public async Task<IActionResult> Login([FromBody] LoginDto model)
{
    // ...
}
```

### Parâmetros do Atributo

- `LogRequestBody` (default: true) - Se deve logar o corpo da requisição
- `LogResponseBody` (default: true) - Se deve logar o corpo da resposta
- `MaxBodyLength` (default: 10000) - Tamanho máximo do corpo a logar

### Dados Logados

- Método HTTP
- Path e Query String
- Headers (sanitizados - headers sensíveis são removidos)
- Request Body (se habilitado)
- Response Status Code
- Response Body (se habilitado)
- IP do cliente
- User Agent
- Tenant ID e User ID (se autenticado)
- Duração da requisição
- Exceções (se houver)

### Consultando Logs

Os logs são armazenados na coleção `RequestResponseLogs` do MongoDB no banco `MultiTenantAuditDb`.

---

## 5. Gzip Compression

### Configuração

A compressão está habilitada automaticamente para:
- JSON (`application/json`)
- XML (`application/xml`)
- Text (`text/plain`, `text/css`, `text/javascript`)
- E outros tipos MIME padrão

### Níveis de Compressão

- **Brotli**: Otimal (melhor compressão)
- **Gzip**: Otimal (fallback)

### Desabilitando para Endpoints Específicos

A compressão é aplicada automaticamente. Para desabilitar em um endpoint específico, você precisaria criar um middleware customizado.

---

## 6. Soft Delete (Logical Delete)

### Atributo LogicalDelete

Marque entidades com `[LogicalDelete]` para habilitar soft delete.

### Exemplo

```csharp
[LogicalDelete]
public class Product : BaseTenantEntity
{
    // ...
}
```

### Comportamento

Quando `DeleteAsync` é chamado em uma entidade com `[LogicalDelete]`:
- `IsDeleted` é definido como `true`
- `DeletedAt` é definido com a data/hora atual
- `UpdatedAt` é atualizado
- A entidade **não** é removida do banco de dados

### Query Filters Automáticos

Queries automáticas excluem entidades deletadas:
- `GetByIdAsync` - não retorna entidades deletadas
- `GetAllAsync` - não retorna entidades deletadas
- `GetPagedAsync` - não retorna entidades deletadas
- `GetAsync` - não retorna entidades deletadas

### Restaurar Entidade

```csharp
await _repository.RestoreAsync(entity);
```

### Hard Delete (Deletar Permanentemente)

```csharp
await _repository.HardDeleteAsync(entity);
```

### Campos Adicionados em BaseEntity

- `IsDeleted` (bool) - Indica se foi deletado logicamente
- `DeletedAt` (DateTime?) - Quando foi deletado
- `UpdatedAt` (DateTime?) - Quando foi atualizado pela última vez

---

## 7. Migrations Automáticas

### Comportamento

Em **Development**, as migrations são aplicadas automaticamente na inicialização da aplicação.

### Em Produção

**NÃO** use migrations automáticas em produção! Aplique migrations manualmente:

```bash
dotnet ef database update --project src/MultiTenantApp.Infrastructure
```

Ou usando scripts de deploy.

### Desabilitando

Para desabilitar migrations automáticas, remova ou comente o bloco no `Program.cs`:

```csharp
// Auto-migrate database (only in Development)
if (builder.Environment.IsDevelopment())
{
    // ... código de migration
}
```

---

## 8. XML Comments e Documentação

### Swagger

Comentários XML são automaticamente incluídos no Swagger quando você adiciona `/// <summary>` nos métodos.

### Habilitando XML Comments

O projeto já está configurado para gerar arquivos XML. Certifique-se de que o `.csproj` tenha:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### Exemplo de Comentários

```csharp
/// <summary>
/// Creates a new product.
/// Request/Response is logged to MongoDB for auditing purposes.
/// </summary>
/// <param name="model">Product creation data</param>
/// <returns>Created product</returns>
[HttpPost("create")]
public async Task<IActionResult> Create([FromBody] CreateProductDto model)
{
    // ...
}
```

---

## Resumo das Novas Funcionalidades

| Funcionalidade | Status | Endpoint/Arquivo |
|---------------|--------|------------------|
| Health Checks | ✅ | `/health`, `/health-ui` |
| Hangfire | ✅ | `/hangfire` (dev) |
| FluentValidation | ✅ | Auto-validado |
| Request/Response Logging | ✅ | `[LogRequestResponse]` |
| Gzip Compression | ✅ | Automático |
| Soft Delete | ✅ | `[LogicalDelete]` |
| Auto Migrations | ✅ | Auto (dev) |
| XML Comments | ✅ | Swagger |
| Soft Delete Fields | ✅ | IsDeleted, DeletedAt, UpdatedAt |
| Hangfire PostgreSQL | ✅ | Produção |
| Hangfire Auth | ✅ | Role-based |
| Additional Validators | ✅ | 6 novos validators |
| Health Checks Fixed | ✅ | PostgreSQL + MongoDB |
| Unit Tests | ✅ | Validators |
| .NET 10 Migration | ✅ | Completa |

---

## Migração para .NET 10

**Status**: ✅ Implementado

**Data da Migração**: Janeiro 2026

**Alterações Realizadas**:
- Atualização de todos os `TargetFramework` de `net9.0` para `net10.0`
- Correção de ambiguidade no `JsonSerializer.Deserialize` no RedisCacheService
- Compatibilidade verificada com todos os packages NuGet
- Todos os testes passando no .NET 10

**Benefícios do .NET 10**:
- Melhor performance e otimizações
- Novos recursos da linguagem C#
- Correções de segurança
- Suporte a longo prazo

---

## Melhorias Implementadas Recentemente

### 1. Campos de Soft Delete (IsDeleted, DeletedAt, UpdatedAt)

**Status**: ✅ Implementado

**Descrição**: Adicionados campos de soft delete e atualização automática nas entidades que herdam de `BaseEntity` e `BaseTenantEntity`.

**Tabelas Afetadas**:
- Products
- Tenants
- Rules
- UserRules

**Campos Adicionados**:
- `IsDeleted` (bool, default: false) - Indica se o registro foi logicamente deletado
- `DeletedAt` (timestamp with time zone, nullable) - Data/hora da exclusão lógica
- `UpdatedAt` (timestamp with time zone, nullable) - Data/hora da última atualização

**Migration**: `20260121155613_AddSoftDeleteFields.cs`

### 2. Hangfire com PostgreSQL para Produção

**Status**: ✅ Implementado

**Descrição**: Hangfire agora usa PostgreSQL em produção e in-memory em desenvolvimento.

**Configuração**:
```csharp
if (builder.Environment.IsProduction())
{
    builder.Services.AddHangfire(config => config
        .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
}
else
{
    builder.Services.AddHangfire(config => config.UseMemoryStorage());
}
```

**Package Adicionado**: `Hangfire.PostgreSql` v1.20.9

### 3. Autorização Melhorada do Hangfire Dashboard

**Status**: ✅ Implementado

**Descrição**: Implementada autorização baseada em roles para o dashboard do Hangfire.

**Regras de Autorização**:
- Usuário deve estar autenticado
- Deve ter uma das seguintes roles: Admin, SystemAdmin, TenantAdmin
- Ou ter a claim "HangfireAccess" com valor "true"

**Arquivo**: `src/MultiTenantApp.Api/Filters/HangfireAuthorizationFilter.cs`

### 4. Validators FluentValidation Adicionais

**Status**: ✅ Implementado

**Validators Criados**:
- `CreateUserDtoValidator` - Validação completa para criação de usuários
- `UpdateUserDtoValidator` - Validação para atualização de usuários
- `CreateRuleDtoValidator` - Validação para criação de regras
- `UpdateRuleDtoValidator` - Validação para atualização de regras
- `RegisterDtoValidator` - Validação para registro de usuários
- `AssignRuleDtoValidator` - Validação para atribuição de regras

**Regras de Validação Incluem**:
- Validação de email
- Senhas fortes (mínimo 8 caracteres, maiúscula, minúscula, número, especial)
- GUIDs válidos
- Comprimento de strings
- Roles válidas

### 5. Health Checks Corrigidos

**Status**: ✅ Implementado

**Correções**:
- Health check do PostgreSQL usando `AddAsyncCheck` com verificação de conexão real
- Health check do MongoDB usando ping do banco admin
- Remoção do health check problemático do MongoDB que causava erros

### 6. Testes de Validação

**Status**: ✅ Implementado

**Testes Criados**:
- `CreateUserDtoValidatorTests` - Testes unitários para validator de criação de usuários
- Testes cobrem cenários válidos e inválidos

**Frameworks**: xUnit, FluentAssertions, Moq

---

## Próximos Passos Recomendados

### ✅ Implementado Recentemente
1. **Hangfire PostgreSQL**: ✅ Configurado automaticamente para produção
2. **Hangfire Dashboard Auth**: ✅ Implementada autorização baseada em roles
3. **Validators Adicionais**: ✅ Criados 6 validators FluentValidation
4. **Soft Delete**: ✅ Campos IsDeleted, DeletedAt, UpdatedAt adicionados
5. **Health Checks**: ✅ Corrigidos e funcionando corretamente

### Próximas Melhorias Sugeridas
1. **Testes de Integração**: Adicionar testes de API endpoints (WebApplicationFactory)
2. **Monitoramento Avançado**: Configurar alertas baseados em health checks
3. **Rate Limiting**: Implementar rate limiting baseado em tenant
4. **Cache Distribuído**: Melhorar estratégia de cache com Redis
5. **Documentação API**: Expandir documentação OpenAPI/Swagger
6. **Jobs Hangfire**: Adicionar mais jobs de background conforme necessário
7. **Logging Estruturado**: Melhorar logs com campos específicos de negócio
