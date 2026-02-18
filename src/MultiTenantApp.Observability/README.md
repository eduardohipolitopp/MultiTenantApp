# MultiTenantApp.Observability

Este projeto centraliza toda a lógica de observabilidade da solução, unificando o uso de **Serilog** (para logs estruturados e console) e **OpenTelemetry** (para métricas, traces e exportação OTLP).

## Funcionalidades

-   **Auto-discovery**: Identifica automaticamente o nome do serviço e versão a partir do assembly de entrada.
-   **Configuração Híbrida**: Permite configurar via `appsettings.json` ou diretamente via código (Program.cs).
-   **Enriquecimento**: Adiciona automaticamente `MachineName`, `ExceptionDetails` e propriedades do serviço aos logs.
-   **Instrumentação Abrangente**:
    -   **HTTP**: AspNetCore e HttpClient.
    -   **Bancos de Dados**: Entity Framework Core, SQL Client, Npgsql (PostgreSQL), MongoDB.
    -   **Cache**: StackExchange.Redis.
    -   **Runtime**: Métricas de memória, CPU, GC, etc.
-   **Ponto único de exportação**: Utiliza o SDK do OpenTelemetry para enviar logs, métricas e traces via protocolo OTLP (gRPC).

> [!NOTE]
> **Sobre Duplicação SQL**: Ao utilizar `EF Core` + `Npgsql/SqlClient`, você verá spans aninhados (o EF inicia o span e o driver cria um interno com o comando real). Isso provê o máximo de detalhamento, mostrando tanto a intenção do ORM quanto a execução no driver.

## Uso

### 1. Configuração no Program.cs

Existem dois métodos principais que devem ser chamados:

```csharp
// 1. Configura o Serilog (Logs no console e ponte para OpenTelemetry)
builder.Host.UseSerilogObservability();

// 2. Configura o SDK do OpenTelemetry (Traces, Metrics e Exporters)
builder.Services.AddOpenTelemetryObservability(builder.Configuration);
```

### 2. Formas de Configuração

O projeto segue uma ordem de precedência: **Código (Invoke) > AppSettings > Defaults (Auto-discovery)**.

#### Opção A: via `appsettings.json` (Recomendado)

```json
{
  "Observability": {
    "ServiceName": "MyAwesomeApi",
    "ServiceVersion": "2.1.0",
    "OtlpEndpoint": "http://otel-collector:4317",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true,
    "Console": {
      "Enabled": true
    }
  }
}
```

#### Opção B: via Código (Overrides)

```csharp
builder.Services.AddOpenTelemetryObservability(builder.Configuration, options => {
    options.ServiceName = "OverrideName";
    options.OtlpEndpoint = "http://custom-collector:4317";
});
```

## Tratamento de Erros e Validação

O projeto possui validação ativa. Se você habilitar Tracing, Metrics ou Logging mas **não** fornecer um `OtlpEndpoint` (seja via appsettings ou código), a aplicação lançará uma `ArgumentException` clara logo na inicialização, evitando falhas silenciosas em produção.

## Stack de Infraestrutura

A stack de observabilidade agora está separada em seu próprio arquivo `docker-compose.yml`, permitindo que ela seja iniciada de forma independente da aplicação.

### Como Iniciar

1.  **Observabilidade**: Vá até a pasta de infraestrutura e inicie os containers:
    ```bash
    docker-compose -f src/MultiTenantApp.Observability/Infrastructure/docker-compose.yml up -d
    ```

2.  **Aplicação**: Inicie a aplicação e bancos de dados a partir da raiz:
    ```bash
    docker-compose up -d
    ```

### Serviços Incluídos

-   **otel-collector**: Receptor OTLP (portas 4317/4318).
-   **prometheus**: Armazenamento de métricas.
-   **loki**: Armazenamento de logs.
-   **tempo**: Armazenamento de traces.
-   **grafana**: Visualização (http://localhost:3000).

---

## Estrutura de Arquivos

Os arquivos de configuração das ferramentas estão centralizados na pasta `Infrastructure/`:

-   `docker-compose.yml`: Definição dos containers de observabilidade.
-   `otel-collector-config.yaml`: Configuração do Collector.
-   `prometheus.yml`: Configuração do Prometheus.
-   `loki-config.yaml`: Configuração do Grafana Loki.
-   `tempo.yaml`: Configuração do Grafana Tempo.
