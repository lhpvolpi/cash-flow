# ADR 0001 — Idempotência do consumidor de eventos e segurança sob concorrência

## Status

Aceito.

## Contexto

O serviço de Consolidação (`CashFlow.Consolidation.Consumer`) aprende sobre lançamentos exclusivamente
via evento assíncrono publicado pelo Outbox do serviço de Transações na fila `daily-balance-updates`
(RabbitMQ). Isso é proposital: o serviço de Transações nunca fica indisponível por causa do
Consolidado, e vice-versa.

Esse desenho tem uma consequência direta: mensagens podem ser **entregues mais de uma vez** — por
redelivery do próprio broker (nack, queda de conexão antes do ack, restart do consumer) ou pelo
mecanismo de retry com DLQ que implementamos sobre a fila principal (ver
`RabbitMqMessageBrokerConsumer`). Sem tratamento, isso contaria o mesmo lançamento duas vezes no
saldo diário.

O RabbitMQ também já suporta *competing consumers* nativamente — nada impede rodar múltiplas
instâncias de `CashFlow.Consolidation.Consumer` no futuro para paralelizar o processamento. Isso
introduz uma segunda fonte de concorrência: duas instâncias processando mensagens diferentes ao
mesmo tempo, cada uma podendo colidir num dos dois índices únicos do banco (`TransactionId` em
`processed_transactions`, `Date` em `daily_balances`).

## Decisão

Implementamos o padrão **Inbox** do lado do Consolidado (`ProcessedTransaction`,
`processed_transactions`): antes de aplicar um evento, o handler verifica se aquele `TransactionId`
já foi processado; se sim, é um no-op. A marca de "processado" é gravada **na mesma transação de
banco** que atualiza o `DailyBalance`, garantindo atomicidade.

Essa checagem sozinha é uma leitura seguida de escrita ("check-then-act"), não uma operação atômica
a nível de banco — duas instâncias podem passar pela checagem antes de qualquer uma commitar. **Não
adicionamos nenhum tratamento extra para essa corrida no handler.** A razão: se o commit falhar por
colisão de unicidade, a exceção sobe normalmente pelo `catch` genérico já existente
(`rollback; throw;`), o `RabbitMqMessageBrokerConsumer` faz nack da mensagem, e ela é reprocessada
depois de um intervalo pelo mecanismo de retry com DLQ que **já existe** para qualquer falha
transitória (ver `RabbitMqMessageBrokerConsumer`). Quando a mensagem for reprocessada, a transação
concorrente já terá commitado, e a checagem de idempotência resolve o resto normalmente.

Cogitamos adicionar um retry em processo (capturar a violação de unicidade e tentar de novo
imediatamente, sem esperar o ciclo de retry da fila) para evitar a latência do `RetryDelayMilliseconds`
nesse cenário específico. Decidimos não fazer isso: seria um segundo mecanismo de retry cobrindo a
mesma classe de falha (transitória, resolvível ao reprocessar) que o retry por mensagem já cobre — e
o Consolidado já é eventualmente consistente por natureza (outbox → broker → consumer), então
alguns segundos a mais num cenário raro de corrida entre instâncias não têm custo real. Duplicar o
mecanismo de retry só para ganhar velocidade num caso raro seria complexidade sem benefício
mensurável (overengineering).

## Consequências

- Positivo: idempotente e seguro tanto com uma única instância do consumer (cenário atual) quanto
  com múltiplas instâncias competindo pela mesma fila (cenário futuro), sem precisar de nenhum
  código adicional — o mecanismo de retry com DLQ que já existe absorve o conflito de concorrência
  como mais um caso de falha transitória.
- Negativo: numa colisão de concorrência, a mensagem leva o `RetryDelayMilliseconds` completo (5s
  por padrão) para ser reprocessada com sucesso, em vez de resolver instantaneamente dentro do mesmo
  request. Aceitável dado que o sistema já opera com consistência eventual em todo o resto do fluxo.
