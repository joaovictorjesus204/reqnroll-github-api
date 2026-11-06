# language: pt
@cache
Funcionalidade: Cache e limite de requisições
  Como desenvolvedor que consulta a API com frequência
  Quero reaproveitar respostas que não mudaram e acompanhar meu limite
  Para não ser bloqueado pelo limite de requisições do GitHub

  Cenário: Requisição condicional com ETag responde 304 sem corpo
    Quando eu consulto o usuário "octocat"
    Então o status da resposta é 200
    Quando eu repito a consulta informando o ETag recebido
    Então o status da resposta é 304
    E o corpo da resposta está vazio

  Cenário: A resposta informa o limite de requisições e o formato
    Quando eu consulto o repositório "octocat/Hello-World"
    Então o limite restante de requisições não passa do limite total
    E o cabeçalho "Content-Type" contém "application/json"
    E o cabeçalho "Content-Type" contém "charset=utf-8"
