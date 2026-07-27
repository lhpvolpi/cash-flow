# Arquitetura

Este documento complementa o [README](../README.md) com uma visão mais detalhada do desenho da
solução: como os dois serviços se relacionam, como cada um é organizado internamente, o fluxo
temporal de um lançamento até aparecer no saldo consolidado, e como tudo isso é implantado em
containers.

## 1. Visão geral do sistema

```mermaid
flowchart LR
    Merchant(["Comerciante"])

    subgraph TX["Serviço de Transações"]
        TXWeb["Transactions.Web\n(API REST)"]
        TXOutbox["Transactions.Outbox\n(worker)"]
        TXDB[("Postgres\ntransactions")]
    end

    subgraph MQ["RabbitMQ"]
        Queue[["daily-balance-updates"]]
    end

    subgraph CO["Serviço de Consolidação"]
        COConsumer["Consolidation.Consumer\n(worker)"]
        COWeb["Consolidation.Web\n(API REST)"]
        CODB[("Postgres\nconsolidation")]
    end

    Merchant -->|"POST /api/transactions"| TXWeb
    Merchant -->|"GET /api/daily-balances"| COWeb
    TXWeb --> TXDB
    TXOutbox --> TXDB
    TXOutbox -->|publica evento| Queue
    Queue -->|consome| COConsumer
    COConsumer --> CODB
    COWeb --> CODB
```

Dois bancos de dados independentes, um por serviço — nenhum dos dois acessa o schema do outro
diretamente. A única via de comunicação entre eles é o evento assíncrono publicado na fila
`daily-balance-updates`. Essa é a decisão central do desenho: o serviço de Transações nunca fala
com o RabbitMQ de forma síncrona (só grava no próprio banco), então ele nunca fica indisponível por
causa do Consolidado ou do broker.

## 2. Arquitetura em camadas (Clean Architecture)

Ambos os serviços seguem a mesma organização interna — a diferença entre eles é só o conteúdo de
cada camada, não a forma:

```mermaid
flowchart TD
    subgraph Presentation["Web / Worker"]
        Endpoints["Minimal API Endpoints\nou BackgroundService"]
    end

    subgraph Application["Application"]
        Handlers["Commands / Queries\n(MediatR)"]
        Validators["Validators\n(FluentValidation)"]
        Ports["Abstrações\n(IRepository, IUnitOfWork, IMessageBroker...)"]
    end

    subgraph Domain["Domain"]
        Entities["Entidades e regras de negócio\n(Transaction, DailyBalance...)"]
    end

    subgraph Infrastructure["Infrastructure"]
        EFCore["EF Core / Postgres"]
        Rabbit["RabbitMQ.Client"]
    end

    Endpoints --> Handlers
    Handlers --> Entities
    Handlers -.usa.-> Ports
    EFCore -.implementa.-> Ports
    Rabbit -.implementa.-> Ports
    Infrastructure --> Entities
    Endpoints -.composition root.-> Infrastructure
```

- **Domain** não depende de nada — só as regras de negócio (ex.: `Transaction` valida seu próprio
  valor e tipo no construtor; `DailyBalance.Apply` soma crédito/débito).
- **Application** depende só de Domain, e define os *ports* (interfaces) que a Infrastructure
  implementa — a direção da dependência é invertida de propósito (Dependency Inversion), então
  Application nunca sabe que existe Postgres ou RabbitMQ.
- **Infrastructure** implementa os ports de Application usando EF Core e RabbitMQ.Client.
- **Web/Worker** é só o ponto de entrada do processo: mapeia endpoints ou inicia o
  `BackgroundService`, e no *composition root* (`Program.cs`/`DependencyInjection.cs`) registra qual
  implementação concreta de Infrastructure satisfaz cada port de Application.

## 3. Fluxo de um lançamento até o saldo consolidado

O diagrama abaixo mostra por que existe uma janela de consistência eventual entre criar um
lançamento e ele aparecer no saldo — e onde cada peça de resiliência (Outbox, retry, idempotência)
entra:

```mermaid
sequenceDiagram
    actor M as Comerciante
    participant TW as Transactions.Web
    participant TDB as Postgres (transactions)
    participant TO as Transactions.Outbox
    participant MQ as RabbitMQ
    participant CC as Consolidation.Consumer
    participant CDB as Postgres (consolidation)
    participant CW as Consolidation.Web

    M->>TW: POST /api/transactions
    TW->>TDB: INSERT Transaction + OutboxMessage (mesma transação)
    TDB-->>TW: OK
    TW-->>M: 200 OK (lançamento criado)

    loop a cada 500ms
        TO->>TDB: SELECT próximo OutboxMessage (FOR UPDATE SKIP LOCKED)
        TO->>MQ: publica evento TransactionCreated
        TO->>TDB: DELETE OutboxMessage publicado
    end

    MQ->>CC: entrega a mensagem
    CC->>CDB: SELECT processed_transactions WHERE transaction_id = ?

    alt já processado (redelivery)
        CC->>MQ: ack (no-op)
    else novo
        CC->>CDB: UPDATE/INSERT DailyBalance + INSERT processed_transactions (mesma transação)
        CC->>MQ: ack
    else falha ao processar
        CC->>MQ: nack (requeue:false)
        MQ->>MQ: fila .retry (TTL) → volta pra fila principal
        Note over MQ,CC: após MaxRetries, vai para .failed (DLQ)
    end

    M->>CW: GET /api/daily-balances/{date}
    CW->>CDB: SELECT DailyBalance
    CDB-->>CW: saldo atualizado
    CW-->>M: 200 OK
```

Pontos que esse diagrama explica:

- O lançamento é confirmado ao comerciante (`200 OK`) **antes** de qualquer contato com o
  RabbitMQ — é por isso que a queda do Consolidado ou do broker não afeta a criação de lançamentos.
- O saldo consolidado não muda instantaneamente; ele reflete a mudança assim que o outbox publica e
  o consumer processa (na prática, milissegundos em condições normais).
- Se a mesma mensagem for entregue duas vezes (redelivery do broker), a checagem de idempotência
  (padrão Inbox) garante que o saldo não seja contado em duplicidade.
- Se o processamento falhar de forma persistente, a mensagem não fica reentregue para sempre — ela
  circula pela fila de retry (com atraso) até esgotar as tentativas e ir para a fila de
  dead-letter, para inspeção manual.

## 4. Deployment (containers)

```mermaid
flowchart TB
    subgraph Docker["docker-compose · rede cash-flow-network"]
        Postgres[("postgres:16-alpine\n:5432")]
        RabbitMQ[["rabbitmq:3.13-management\nAMQP :5672 · UI :15672"]]
        TXWeb["transactions-web\n:5222"]
        TXOutbox["transactions-outbox"]
        COWeb["consolidation-web\n:5224"]
        COConsumer["consolidation-consumer"]
    end

    TXWeb --> Postgres
    TXOutbox --> Postgres
    TXOutbox --> RabbitMQ
    COConsumer --> RabbitMQ
    COConsumer --> Postgres
    COWeb --> Postgres
```

`postgres` e `rabbitmq` sobem sempre (`make up`); os quatro serviços .NET ficam no profile `app` do
Compose e só sobem com `make up-all` — isso preserva o fluxo de desenvolvimento local (rodar os
serviços via `dotnet run` contra a infra em container) sem conflito de porta com a stack
inteira em containers. Veja a seção **Como rodar localmente** do README para as duas opções.

## Decisões arquiteturais detalhadas

Trade-offs específicos (idempotência, retry/DLQ, escalabilidade horizontal do consumer) estão
documentados como ADRs em [docs/adr](adr/).
