# language: pt
@paginacao
Funcionalidade: Paginação e busca
  Como desenvolvedor que consome listas grandes da API
  Quero navegar por páginas e buscar com filtros
  Para trazer só o que preciso, sem repetir itens

  @smoke
  Cenário: A segunda página traz outros repositórios, na mesma ordem
    Quando eu listo os repositórios do usuário "octocat" com 2 por página na página 1
    Então o status da resposta é 200
    E a lista tem 2 itens
    E o cabeçalho Link aponta para a próxima página
    Quando eu listo os repositórios do usuário "octocat" com 2 por página na página 2
    Então a lista tem 2 itens
    E nenhum repositório se repete em relação à página anterior
    E os repositórios estão em ordem alfabética, continuando a página anterior

  Cenário: Busca filtra pela linguagem e ordena por estrelas
    Quando eu busco repositórios com "language:elixir" ordenados por estrelas
    Então o status da resposta é 200
    E todos os resultados têm a linguagem "Elixir"
    E os resultados estão em ordem decrescente de estrelas
