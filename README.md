# CShortener - URL Shortener API

O **CShortener** é uma Web API para encurtamento de URLs. Este projeto foi desenvolvido com o objetivo de colocar em prática conceitos de arquitetura, uso de cache em memória e infraestrutura conteinerizada, focando em performance e organização do código.

## Tecnologias e Arquitetura

* **.NET 10 (C#) / Minimal APIs:** Estrutura leve e otimizada para alta performance.
* **PostgreSQL:** Persistência de dados relacionais.
* **Redis:** Cache em memória distribuído para contagem ultrarrápida de acessos.
* **Entity Framework Core (EF Core):** ORM para mapeamento de dados e migrações.
* **Docker & Docker Compose:** Orquestração e padronização do ambiente local.

## Diferenciais Arquiteturais

Para lidar com a contagem de cliques sem sobrecarregar o banco de dados principal, o projeto implementa as seguintes soluções:

1. **Micro-batching com BackgroundService:** Quando um usuário acessa um link encurtado, a contagem de cliques é registrada instantaneamente no Redis, evitando acessos diretos de escrita no PostgreSQL a cada requisição. Um serviço em segundo plano (BackgroundService) roda periodicamente para coletar esses dados do Redis e atualizar o banco de dados relacional de uma só vez (processamento em lotes).

2. **Migrações Automáticas:** A inicialização da aplicação inclui a execução de db.Database.Migrate(). Isso garante que, ao rodar o projeto via Docker pela primeira vez, a estrutura do banco de dados (tabelas e colunas) seja criada automaticamente, facilitando a configuração do ambiente

## 🏁 Como Rodar o Projeto (Docker)

Todo o ecossistema (API + Postgres + Redis) está empacotado e pronto para rodar.

**Pré-requisitos:** `Docker` e `Docker Compose` instalados.

1. Clone o repositório:
   ```bash
   git clone https://github.com/IamPedrin/CShortener.git
   cd CShortener

2. Inicie os contêineres em segundo plano:
   ```bash
   docker compose up --build -d

A API estará disponível em: `http://localhost:5000`

**Nota:** Para acompanhar o `BackgroundService` sincronizando os dados do Redis com o PostgreSQL em tempo real, você pode verificar os logs do contêiner da API com o comando `docker logs cshortener-api -f`.

## Endpoints

Para manter a organização e permitir futuras evoluções, as rotas de criação utilizam versionamento, enquanto o redirecionamento fica na raiz para manter os links os menores possíveis.

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/v1/shorten` | Recebe a URL original e retorna o código encurtado gerado. |
| `GET` | `/{shortCode}`   | Busca a URL original, atualiza o contador de acessos via Redis e realiza o redirecionamento (HTTP 302). |
| `GET` | `/{shortcode}/stats` | Retorna informações sobre o código encurtado gerado. |
