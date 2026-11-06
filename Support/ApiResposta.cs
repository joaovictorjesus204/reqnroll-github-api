using System.Net;
using System.Text.Json;

namespace ReqnrollGithubApi.Support;

/// <summary>Resposta da API já lida: status, cabeçalhos, corpo e tempo.</summary>
public sealed record ApiResposta(
    HttpStatusCode Status,
    Dictionary<string, string> Cabecalhos,
    string Corpo,
    TimeSpan Tempo,
    string Caminho)
{
    public int Codigo => (int)Status;

    public JsonElement Json => JsonDocument.Parse(Corpo).RootElement;

    public string? Cabecalho(string nome) =>
        Cabecalhos.TryGetValue(nome.ToLowerInvariant(), out var valor) ? valor : null;

    /// <summary>Lê um campo pelo caminho com pontos, por exemplo "owner.login".</summary>
    public JsonElement Campo(string caminho)
    {
        var atual = Json;
        foreach (var parte in caminho.Split('.'))
        {
            if (atual.ValueKind != JsonValueKind.Object || !atual.TryGetProperty(parte, out var proximo))
                throw new AssertionException($"O campo \"{caminho}\" não existe na resposta de {Caminho}.");
            atual = proximo;
        }
        return atual;
    }
}
