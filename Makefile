.PHONY: help up down restart logs clean build run-transactions-web run-transactions-worker run-consolidation-web run-consolidation-consumer add-migration-transactions add-migration-consolidation migrate-transactions migrate-consolidation test stop clear restore rebuild

help:
	@echo "CashFlow - Comandos disponíveis:"
	@echo ""
	@echo "🐳 Docker:"
	@echo "  make up              - Levantar PostgreSQL e RabbitMQ"
	@echo "  make down            - Parar containers"
	@echo "  make restart         - Reiniciar containers"
	@echo "  make logs            - Ver logs dos containers"
	@echo "  make clean           - Limpar containers e volumes"
	@echo ""
	@echo "🔨 Build:"
	@echo "  make restore         - Restaurar dependências NuGet"
	@echo "  make build           - Build de todos os projetos"
	@echo "  make rebuild         - Clean + Restore + Build"
	@echo "  make clear           - Limpar bin/ e obj/"
	@echo ""
	@echo "🚀 Executar:"
	@echo "  make run-transactions-web         - Rodar Web API (Transactions)"
	@echo "  make run-transactions-worker      - Rodar Outbox Worker"
	@echo "  make run-consolidation-web        - Rodar Consolidation Web"
	@echo "  make run-consolidation-consumer   - Rodar Consolidation Consumer"
	@echo ""
	@echo "📊 Database:"
	@echo "  make add-migration-transactions NAME=<nome>     - Criar migration (Transactions)"
	@echo "  make add-migration-consolidation NAME=<nome>    - Criar migration (Consolidation)"
	@echo "  make migrate-transactions                       - Atualizar migrations (Transactions)"
	@echo "  make migrate-consolidation                      - Atualizar migrations (Consolidation)"
	@echo ""
	@echo "🧪 Testes:"
	@echo "  make test                         - Rodar testes"
	@echo ""
	@echo "⛔ Utilitários:"
	@echo "  make stop                         - Parar processos .NET em execução"

# Docker
up:
	docker-compose up -d
	@echo "✅ Containers iniciados"

down:
	docker-compose down
	@echo "✅ Containers parados"

restart: down up
	@echo "✅ Containers reiniciados"

logs:
	docker-compose logs -f

clean:
	docker-compose down -v
	@echo "✅ Containers e volumes removidos"

# Build
restore:
	dotnet restore
	@echo "✅ Dependências restauradas"

build: 
	dotnet build CashFlow.sln
	@echo "✅ Build completo"

rebuild: clear restore build
	@echo "✅ Rebuild completo"

clear:
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	@echo "✅ Diretórios bin/ e obj/ removidos"

# Executar
run-transactions-web:
	cd services/transactions/src/CashFlow.Transactions.Web && dotnet run

run-transactions-worker:
	cd services/transactions/src/CashFlow.Transactions.Outbox && dotnet run

run-consolidation-web:
	cd services/consolidation/src/CashFlow.Consolidation.Web && dotnet run

run-consolidation-consumer:
	cd services/consolidation/src/CashFlow.Consolidation.Consumer && dotnet run

# Database
add-migration-transactions:
	@if [ -z "$(NAME)" ]; then echo "❌ Use: make add-migration-transactions NAME=MigrationName"; exit 1; fi
	dotnet ef migrations add $(NAME) --project services/transactions/src/CashFlow.Transactions.Infrastructure --startup-project services/transactions/src/CashFlow.Transactions.Web
	@echo "✅ Migration '$(NAME)' criada em Transactions"

add-migration-consolidation:
	@if [ -z "$(NAME)" ]; then echo "❌ Use: make add-migration-consolidation NAME=MigrationName"; exit 1; fi
	dotnet ef migrations add $(NAME) --project services/consolidation/src/CashFlow.Consolidation.Infrastructure --startup-project services/consolidation/src/CashFlow.Consolidation.Web
	@echo "✅ Migration '$(NAME)' criada em Consolidation"

migrate-transactions:
	dotnet ef database update --project services/transactions/src/CashFlow.Transactions.Infrastructure --startup-project services/transactions/src/CashFlow.Transactions.Web
	@echo "✅ Migrations de Transactions aplicadas"

migrate-consolidation:
	dotnet ef database update --project services/consolidation/src/CashFlow.Consolidation.Infrastructure --startup-project services/consolidation/src/CashFlow.Consolidation.Web
	@echo "✅ Migrations de Consolidation aplicadas"

# Testes
test:
	dotnet test CashFlow.sln
	@echo "✅ Testes executados"

# Utilitários
stop:
	pkill -f dotnet || true
	@echo "✅ Processos .NET parados"
