# language: pt
@usuarios
Funcionalidade: Consulta de usuários
  Como desenvolvedor que integra um sistema com o GitHub
  Quero consultar perfis públicos de usuários
  Para exibir os dados de quem contribui nos meus projetos

  @smoke
  Cenário: Consultar um usuário existente
    Quando eu consulto o usuário "octocat"
    Então o status da resposta é 200
    E o campo "login" é "octocat"
    E o campo "id" é o número 583231
    E o campo "type" é "User"
    E o campo "created_at" é uma data no formato ISO 8601
    E o tempo de resposta é menor que 5000 ms

  Cenário: Contrato do perfil de usuário
    Quando eu consulto o usuário "octocat"
    Então a resposta tem os campos:
      | campo        | tipo   |
      | login        | texto  |
      | id           | número |
      | avatar_url   | texto  |
      | html_url     | texto  |
      | public_repos | número |
      | followers    | número |
      | created_at   | texto  |

  Cenário: Usuário inexistente
    # Nomes de usuário do GitHub não podem começar com hífen: este nunca vai existir.
    Quando eu consulto o usuário "-usuario-inexistente"
    Então o status da resposta é 404
    E a mensagem de erro é "Not Found"
