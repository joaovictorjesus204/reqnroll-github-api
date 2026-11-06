namespace ReqnrollGithubApi.Support;

/// <summary>Estado compartilhado entre os passos de um mesmo cenário (injeção de contexto do Reqnroll).</summary>
public sealed class ContextoDaApi
{
    public ApiResposta? Ultima { get; private set; }
    public ApiResposta? Anterior { get; private set; }

    public ApiResposta Resposta =>
        Ultima ?? throw new InvalidOperationException("Nenhuma requisição foi feita neste cenário.");

    public void Registrar(ApiResposta resposta)
    {
        Anterior = Ultima;
        Ultima = resposta;
    }
}
