# C# + Reqnroll | API REST do GitHub

[![Testes de API](https://github.com/joaovictorjesus204/reqnroll-github-api/actions/workflows/reqnroll.yml/badge.svg)](https://github.com/joaovictorjesus204/reqnroll-github-api/actions/workflows/reqnroll.yml)

**📊 Relatório da última execução (documentação viva):** https://joaovictorjesus204.github.io/reqnroll-github-api/

Testes de API em **C# (.NET 10)** com **Reqnroll** (o sucessor do SpecFlow), **NUnit** e **HttpClient** na **[API REST do GitHub](https://docs.github.com/pt/rest)**, com cenários **BDD escritos em português**.

Autor: João Victor de Jesus

---

## Números

| | |
|---|---|
| Funcionalidades | 4 (usuários, repositórios, paginação e busca, cache e limite de requisições) |
| Cenários | 14 (com Esquema do Cenário orientado a dados) |
| O que é validado | Status, campos, tipos (contrato), datas, paginação, ordenação, busca, ETag e limite de requisições |
| Execução automática | A cada push e diariamente, nos dias úteis, pelo GitHub Actions |
| Relatório | HTML do Reqnroll (documentação viva) publicado no GitHub Pages |

---

## Exemplo de cenário

```gherkin
# language: pt
Funcionalidade: Paginação e busca

  Cenário: A segunda página traz outros repositórios, na mesma ordem
    Quando eu listo os repositórios do usuário "octocat" com 2 por página na página 1
    Então o status da resposta é 200
    E a lista tem 2 itens
    E o cabeçalho Link aponta para a próxima página
    Quando eu listo os repositórios do usuário "octocat" com 2 por página na página 2
    Então a lista tem 2 itens
    E nenhum repositório se repete em relação à página anterior
    E os repositórios estão em ordem alfabética, continuando a página anterior
```

---

## Como rodar

Pré-requisito: **.NET 10 SDK** (https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/joaovictorjesus204/reqnroll-github-api.git
cd reqnroll-github-api
dotnet test
```

Relatório: `reports\index.html`.

| Comando | O que faz |
|---|---|
| `dotnet test` | Roda todos os cenários |
| `dotnet test --filter "Category=smoke"` | Só os cenários críticos |
| `dotnet test --filter "Category=repositorios"` | Só uma funcionalidade |

### Ver cada passo no terminal

```bash
dotnet test --logger "console;verbosity=detailed"
```

Mostra cada cenário, cada passo do Gherkin com ✓ ou ✗, e cada requisição com status e tempo.

### Limite de requisições

Sem token, a API do GitHub aceita 60 requisições por hora por IP. Uma execução usa cerca de 20. Para rodar várias vezes seguidas, crie um token sem nenhuma permissão em https://github.com/settings/tokens e defina antes de rodar:

```bash
$env:GITHUB_TOKEN = "seu_token"
```

No GitHub Actions, o token automático já é usado (1.000 requisições por hora).

---

## Cobertura

| Funcionalidade | Cenário | Tag |
|---|---|---|
| Usuários | Usuário existente: login, id, tipo, data de criação e tempo de resposta | `@smoke` |
| Usuários | Contrato: campos obrigatórios e seus tipos | |
| Usuários | Usuário inexistente retorna 404 e "Not Found" | |
| Repositórios | Hello-World: id, nome, dono, fork, privado e branch padrão | `@smoke` |
| Repositórios | Linguagem principal de Linux, Elixir, CPython e VS Code (Esquema do Cenário) | |
| Repositórios | Elixir: licença Apache 2.0, mais de 20 mil estrelas e não arquivado | |
| Repositórios | Repositório inexistente retorna 404 | |
| Paginação | Página 2 não repete itens e continua a ordem da página 1 | `@smoke` |
| Busca | Filtro por linguagem e ordenação por estrelas | |
| Cache | Requisição condicional com ETag responde 304 sem corpo | |
| Limites | Cabeçalhos de limite de requisições e Content-Type com UTF-8 | |

---

## Estrutura

```
Features/                 cenários BDD em português (.feature)
Steps/ApiSteps.cs         passos: ligam cada frase do Gherkin ao código
Support/GitHubClient.cs   cliente HTTP com os cabeçalhos recomendados pelo GitHub
Support/ApiResposta.cs    resposta lida: status, cabeçalhos, corpo, tempo e leitura de campos
Support/ContextoDaApi.cs  estado do cenário, compartilhado entre os passos
reqnroll.json             idioma do Gherkin e relatório HTML
```

---

## Decisões técnicas

| Decisão | Por quê |
|---|---|
| **Reqnroll em vez de SpecFlow** | O SpecFlow foi descontinuado. O Reqnroll é o sucessor de código aberto, mantido pela mesma comunidade, com a mesma sintaxe |
| **Gherkin em português** (`# language: pt`) | Produto, negócio e QA leem e validam os mesmos cenários |
| **Passos genéricos e reutilizáveis** | "o campo X é Y" serve para qualquer endpoint. Novos cenários quase não precisam de código novo |
| **Campos aninhados por caminho** (`owner.login`, `license.spdx_id`) | O passo lê qualquer nível do JSON sem criar uma classe para cada resposta |
| **Contrato por tabela de campos e tipos** | Valida a estrutura da resposta sem depender de valores que mudam |
| **Dados estáveis** | `octocat` e `Hello-World` são os exemplos oficiais do GitHub e não mudam. Números que crescem (estrelas) são validados com "maior que", e não com valor fixo |
| **Injeção de contexto** (`ContextoDaApi`) | Os passos compartilham a resposta pelo construtor, sem variáveis estáticas |
| **HttpClient único** | Reaproveita conexões, como a Microsoft recomenda |
| **Cabeçalhos recomendados pelo GitHub** (`User-Agent`, `Accept`, `X-GitHub-Api-Version`) | A API exige User-Agent e versiona o contrato pelo cabeçalho |
| **Token opcional** (`GITHUB_TOKEN`) | Sem token funciona no computador local; no CI, o token automático aumenta o limite |
| **Falha clara quando o limite acaba** | Em vez de um erro confuso no passo seguinte, o teste explica que o limite de requisições acabou e como resolver |
| **Teste de ETag** | Requisição condicional é como um cliente bem feito economiza o limite: a resposta 304 não conta no limite |

## Limitações

- API pública de terceiros: os dados dos repositórios podem mudar (por exemplo, a linguagem principal). Os cenários usam projetos grandes e estáveis para reduzir esse risco.
- Os testes só leem dados. Criar ou alterar repositórios exigiria um token com permissão de escrita e uma conta de teste.

## Próximos passos

- Validação de contrato com JSON Schema a partir da especificação OpenAPI publicada pelo GitHub.
- Cenários autenticados de escrita (issues e comentários) numa conta de teste.
- Execução paralela dos cenários.
