using System.Globalization;
using System.Text.Json;
using Reqnroll;
using ReqnrollGithubApi.Support;

namespace ReqnrollGithubApi.Steps;

[Binding]
public sealed class ApiSteps(ContextoDaApi contexto)
{
    private readonly GitHubClient _github = new();

    // ---------- Requisições ----------

    [When(@"^eu consulto o usuário ""(.*)""$")]
    public async Task ConsultarUsuario(string usuario) =>
        contexto.Registrar(await _github.GetAsync($"users/{usuario}"));

    [When(@"^eu consulto o repositório ""(.*)""$")]
    public async Task ConsultarRepositorio(string repositorio) =>
        contexto.Registrar(await _github.GetAsync($"repos/{repositorio}"));

    [When(@"^eu listo os repositórios do usuário ""(.*)"" com (\d+) por página na página (\d+)$")]
    public async Task ListarRepositorios(string usuario, int porPagina, int pagina) =>
        contexto.Registrar(await _github.GetAsync(
            $"users/{usuario}/repos?sort=full_name&per_page={porPagina}&page={pagina}"));

    [When(@"^eu busco repositórios com ""(.*)"" ordenados por estrelas$")]
    public async Task BuscarRepositorios(string consulta) =>
        contexto.Registrar(await _github.GetAsync(
            $"search/repositories?q={Uri.EscapeDataString(consulta)}&sort=stars&order=desc&per_page=10"));

    [When(@"^eu repito a consulta informando o ETag recebido$")]
    public async Task RepetirComEtag()
    {
        var anterior = contexto.Resposta;
        var etag = anterior.Cabecalho("etag");
        Assert.That(etag, Is.Not.Null.And.Not.Empty, "A primeira resposta não trouxe ETag.");
        contexto.Registrar(await _github.GetAsync(anterior.Caminho, etag));
    }

    // ---------- Status, cabeçalhos e tempo ----------

    [Then(@"^o status da resposta é (\d+)$")]
    public void StatusDaResposta(int esperado) =>
        Assert.That(contexto.Resposta.Codigo, Is.EqualTo(esperado),
            $"Corpo recebido: {Resumo(contexto.Resposta.Corpo)}");

    [Then(@"^o cabeçalho ""(.*)"" contém ""(.*)""$")]
    public void CabecalhoContem(string nome, string trecho) =>
        Assert.That(contexto.Resposta.Cabecalho(nome), Does.Contain(trecho));

    [Then(@"^o tempo de resposta é menor que (\d+) ms$")]
    public void TempoDeResposta(int limite) =>
        Assert.That(contexto.Resposta.Tempo.TotalMilliseconds, Is.LessThan(limite));

    [Then(@"^o limite restante de requisições não passa do limite total$")]
    public void LimiteDeRequisicoes()
    {
        var limite = int.Parse(contexto.Resposta.Cabecalho("x-ratelimit-limit") ?? "-1");
        var restante = int.Parse(contexto.Resposta.Cabecalho("x-ratelimit-remaining") ?? "-1");
        var esperado = GitHubClient.Autenticado ? 1000 : 60;
        Assert.Multiple(() =>
        {
            Assert.That(limite, Is.GreaterThanOrEqualTo(esperado), "Limite total por hora");
            Assert.That(restante, Is.InRange(0, limite), "Requisições restantes");
        });
    }

    [Then(@"^o corpo da resposta está vazio$")]
    public void CorpoVazio() => Assert.That(contexto.Resposta.Corpo, Is.Empty);

    // ---------- Campos ----------

    [Then(@"^o campo ""(.*)"" é ""(.*)""$")]
    public void CampoTexto(string campo, string esperado) =>
        Assert.That(contexto.Resposta.Campo(campo).GetString(), Is.EqualTo(esperado));

    [Then(@"^o campo ""(.*)"" é o número (\d+)$")]
    public void CampoNumero(string campo, long esperado) =>
        Assert.That(contexto.Resposta.Campo(campo).GetInt64(), Is.EqualTo(esperado));

    [Then(@"^o campo ""(.*)"" é (verdadeiro|falso)$")]
    public void CampoBooleano(string campo, string valor) =>
        Assert.That(contexto.Resposta.Campo(campo).GetBoolean(), Is.EqualTo(valor == "verdadeiro"));

    [Then(@"^o campo ""(.*)"" é maior que (\d+)$")]
    public void CampoMaiorQue(string campo, long minimo) =>
        Assert.That(contexto.Resposta.Campo(campo).GetInt64(), Is.GreaterThan(minimo));

    [Then(@"^o campo ""(.*)"" é uma data no formato ISO 8601$")]
    public void CampoData(string campo)
    {
        var texto = contexto.Resposta.Campo(campo).GetString();
        Assert.That(DateTimeOffset.TryParseExact(texto, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out _), Is.True, $"Data recebida: {texto}");
    }

    [Then(@"^a resposta tem os campos:$")]
    public void RespostaTemOsCampos(Table tabela)
    {
        Assert.Multiple(() =>
        {
            foreach (var linha in tabela.Rows)
            {
                var campo = contexto.Resposta.Campo(linha["campo"]);
                Assert.That(Tipo(campo), Is.EqualTo(linha["tipo"]), $"Tipo do campo {linha["campo"]}");
            }
        });
    }

    [Then(@"^a mensagem de erro é ""(.*)""$")]
    public void MensagemDeErro(string mensagem) => CampoTexto("message", mensagem);

    // ---------- Listas ----------

    [Then(@"^a lista tem (\d+) itens$")]
    public void ListaTemItens(int quantidade) =>
        Assert.That(contexto.Resposta.Json.GetArrayLength(), Is.EqualTo(quantidade));

    [Then(@"^o cabeçalho Link aponta para a próxima página$")]
    public void LinkProximaPagina() =>
        Assert.That(contexto.Resposta.Cabecalho("link"), Does.Contain("rel=\"next\""));

    [Then(@"^nenhum repositório se repete em relação à página anterior$")]
    public void PaginasSemRepeticao()
    {
        var anterior = contexto.Anterior ?? throw new InvalidOperationException("Não há página anterior.");
        var atuais = Nomes(contexto.Resposta.Json);
        Assert.That(atuais, Is.Not.Empty);
        Assert.That(atuais.Intersect(Nomes(anterior.Json)), Is.Empty);
    }

    [Then(@"^os repositórios estão em ordem alfabética, continuando a página anterior$")]
    public void OrdemAlfabetica()
    {
        var anteriores = Nomes(contexto.Anterior!.Json);
        var todos = anteriores.Concat(Nomes(contexto.Resposta.Json)).ToList();
        Assert.That(todos, Is.Ordered.Using((IComparer<string>)StringComparer.OrdinalIgnoreCase));
    }

    [Then(@"^todos os resultados têm a linguagem ""(.*)""$")]
    public void ResultadosComLinguagem(string linguagem)
    {
        var itens = contexto.Resposta.Campo("items").EnumerateArray().ToList();
        Assert.That(itens, Is.Not.Empty);
        Assert.That(itens.Select(i => i.GetProperty("language").GetString()), Has.All.EqualTo(linguagem));
    }

    [Then(@"^os resultados estão em ordem decrescente de estrelas$")]
    public void OrdemDeEstrelas()
    {
        var estrelas = contexto.Resposta.Campo("items").EnumerateArray()
            .Select(i => i.GetProperty("stargazers_count").GetInt64()).ToList();
        Assert.That(estrelas, Is.Ordered.Descending);
    }

    // ---------- Auxiliares ----------

    private static List<string> Nomes(JsonElement lista) =>
        lista.EnumerateArray().Select(r => r.GetProperty("full_name").GetString()!).ToList();

    private static string Tipo(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => "texto",
        JsonValueKind.Number => "número",
        JsonValueKind.True or JsonValueKind.False => "booleano",
        JsonValueKind.Object => "objeto",
        JsonValueKind.Array => "lista",
        JsonValueKind.Null => "nulo",
        _ => e.ValueKind.ToString()
    };

    private static string Resumo(string corpo) => corpo.Length > 300 ? corpo[..300] + "..." : corpo;
}
