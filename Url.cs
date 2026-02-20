public class Url
{
    public long Id { get; private set; }
    public string OriginalUrl { get; private set; }
    public string ShortCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int AccessCount { get; private set; }


    public Url(string originalUrl, string shortCode)
    {
        OriginalUrl = originalUrl;
        ShortCode = shortCode;
        CreatedAt = DateTime.UtcNow;
        AccessCount = 0;
    }

    //Método para incrementar o contador de acessos
    public void NewAccess()
    {
        AccessCount++;
    }

}

//Record para receber a URL original na requisição de criação do link encurtado
//Difrença entre class e record: Record garante que os dados nao sejam alterados depois de criados, ou seja, são imutáveis. 
//Já as classes permitem que os dados sejam alterados depois de criados.
public record CreateUrlRequest
{
    public required string OriginalUrl { get; set; }
}