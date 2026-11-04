using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace TDE03.Services;

public class FirebaseService
{
    private readonly HttpClient _httpClient;

    public FirebaseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Dictionary<string, T>> Listar<T>(string tabela)
    {
        var resposta = await _httpClient.GetAsync(CriarUrl(tabela));
        await VerificarResposta(resposta);
        string json = await resposta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return new Dictionary<string, T>();

        if (json.Contains("\"error\""))
            throw new Exception("Firebase recusou a consulta: " + json);

        var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        if (json.TrimStart().StartsWith("["))
        {
            var lista = JsonSerializer.Deserialize<List<T?>>(json, opcoes) ?? new List<T?>();
            var registros = new Dictionary<string, T>();
            for (int i = 0; i < lista.Count; i++)
            {
                if (lista[i] != null)
                    registros[i.ToString()] = lista[i]!;
            }
            return registros;
        }

        return JsonSerializer.Deserialize<Dictionary<string, T>>(json, opcoes) ?? new Dictionary<string, T>();
    }

    public async Task<T?> Obter<T>(string tabela, string id)
    {
        var resposta = await _httpClient.GetAsync(CriarUrl(tabela + "/" + id));
        await VerificarResposta(resposta);
        string json = await resposta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json) || json == "null") return default;
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task Salvar<T>(string tabela, string id, T registro)
    {
        var resposta = await _httpClient.PutAsJsonAsync(CriarUrl(tabela + "/" + id), registro);
        await VerificarResposta(resposta);
    }

    public async Task Excluir(string tabela, string id)
    {
        var resposta = await _httpClient.DeleteAsync(CriarUrl(tabela + "/" + id));
        await VerificarResposta(resposta);
    }

    public async Task<bool> Existe(string tabela, string id)
    {
        var registro = await Obter<JsonElement>(tabela, id);
        return registro.ValueKind != JsonValueKind.Null && registro.ValueKind != JsonValueKind.Undefined;
    }

    private string CriarUrl(string caminho)
    {
        string baseUrl = Preferences.Default.Get("FirebaseDatabaseUrl", "").Trim().TrimEnd('/');
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new Exception("Configure uma URL HTTPS válida do Firebase na tela Configuração.");
        string caminhoSeguro = string.Join("/", caminho.Split('/').Select(Uri.EscapeDataString));
        return baseUrl + "/" + caminhoSeguro + ".json";
    }

    private static async Task VerificarResposta(HttpResponseMessage resposta)
    {
        if (resposta.IsSuccessStatusCode) return;
        string detalhe = await resposta.Content.ReadAsStringAsync();
        if (resposta.StatusCode == HttpStatusCode.Unauthorized || resposta.StatusCode == HttpStatusCode.Forbidden)
            throw new Exception("Acesso negado pelo Firebase. Verifique as regras do Realtime Database.");
        throw new Exception("Erro do Firebase: " + detalhe);
    }
}
