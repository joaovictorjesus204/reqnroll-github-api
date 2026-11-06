using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace ReqnrollGithubApi.Support;

/// <summary>
/// Cliente da API REST do GitHub. Envia os cabeçalhos recomendados pela documentação
/// e usa o token do ambiente (GITHUB_TOKEN), quando existe, para ter um limite maior de requisições.
/// </summary>
public sealed class GitHubClient
{
    public static readonly Uri Base = new("https://api.github.com/");
    private static readonly HttpClient Http = Criar();

    private static HttpClient Criar()
    {
        var http = new HttpClient { BaseAddress = Base, Timeout = TimeSpan.FromSeconds(30) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("reqnroll-github-api-tests/1.0");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return http;
    }

    public static bool Autenticado => Http.DefaultRequestHeaders.Authorization is not null;

    public async Task<ApiResposta> GetAsync(string caminho, string? etag = null)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, caminho);
        if (etag is not null)
            requisicao.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(etag));

        var cronometro = Stopwatch.StartNew();
        using var resposta = await Http.SendAsync(requisicao);
        var corpo = await resposta.Content.ReadAsStringAsync();
        cronometro.Stop();

        var cabecalhos = resposta.Headers
            .Concat(resposta.Content.Headers)
            .GroupBy(h => h.Key.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => string.Join(", ", g.SelectMany(h => h.Value)));

        var lida = new ApiResposta(resposta.StatusCode, cabecalhos, corpo, cronometro.Elapsed, caminho);
        Console.WriteLine($"GET {caminho} -> {lida.Codigo} em {lida.Tempo.TotalMilliseconds:F0} ms");

        // Limite de requisições esgotado: falha com a explicação, em vez de um erro confuso no passo seguinte.
        if (lida.Status is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests
            && lida.Cabecalho("x-ratelimit-remaining") == "0")
        {
            Assert.Fail("O limite de requisições da API do GitHub acabou (60 por hora sem token). " +
                        "Espere a renovação ou defina a variável GITHUB_TOKEN.");
        }

        return lida;
    }
}
