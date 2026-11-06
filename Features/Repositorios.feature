# language: pt
@repositorios
Funcionalidade: Consulta de repositórios
  Como desenvolvedor que integra um sistema com o GitHub
  Quero consultar dados públicos de repositórios
  Para mostrar linguagem, licença e popularidade de projetos

  @smoke
  Cenário: Consultar o primeiro repositório de exemplo do GitHub
    Quando eu consulto o repositório "octocat/Hello-World"
    Então o status da resposta é 200
    E o campo "id" é o número 1296269
    E o campo "full_name" é "octocat/Hello-World"
    E o campo "owner.login" é "octocat"
    E o campo "fork" é falso
    E o campo "private" é falso
    E o campo "default_branch" é "master"

  Esquema do Cenário: Linguagem principal de projetos conhecidos
    Quando eu consulto o repositório "<repositorio>"
    Então o status da resposta é 200
    E o campo "language" é "<linguagem>"

    Exemplos:
      | repositorio        | linguagem  |
      | torvalds/linux     | C          |
      | elixir-lang/elixir | Elixir     |
      | python/cpython     | Python     |
      | microsoft/vscode   | TypeScript |

  Cenário: Elixir, linguagem criada pelo brasileiro José Valim, usa licença Apache 2.0
    Quando eu consulto o repositório "elixir-lang/elixir"
    Então o campo "license.spdx_id" é "Apache-2.0"
    E o campo "stargazers_count" é maior que 20000
    E o campo "archived" é falso

  Cenário: Repositório inexistente
    Quando eu consulto o repositório "octocat/repositorio-que-nao-existe"
    Então o status da resposta é 404
    E a mensagem de erro é "Not Found"
