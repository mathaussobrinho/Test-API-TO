using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string createEpisodeUrl = "https://api.tolife.app/integration/api/v1/Episode";
        string classifyQueueUrl = "https://api.tolife.app/integration/api/v1/Episode?location=S&idLocation=56&isCompleted=true";
        string evictUrl = "https://api.tolife.app/integration/api/v1/Episode/idEpisode/8107202";
        string authorizationToken = "14c31e19-1a49-48c9-ae31-2ebc86927844";
        int waitTimeInSeconds = 30;

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", authorizationToken);

        string postData = "{\"idFlow\":1081,\"episodeTicket\":{\"ticketInitials\":\"RECP032\",\"ticketSequence\":1},\"patient\":{\"patientName\":\"Mathaus cp New\"}}";
        StringContent content = new StringContent(postData, Encoding.UTF8, "application/json");

        try
        {
            // Criar episódio
            HttpResponseMessage createEpisodeResponse = await client.PostAsync(createEpisodeUrl, content);
            string createEpisodeResponseText = await createEpisodeResponse.Content.ReadAsStringAsync();
            Console.WriteLine(createEpisodeResponseText);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Falha ao criar episódio: " + ex.Message);
            return;
        }

        string idEpisode = "8107202"; // ID do episódio que deseja evadir

        while (true)
        {
            try
            {
                // Listar fila de classificação
                HttpResponseMessage classifyQueueResponse = await client.GetAsync(classifyQueueUrl);
                string classifyQueueResponseText = await classifyQueueResponse.Content.ReadAsStringAsync();
                Console.WriteLine("Lista de episódios na fila de classificação:");
                Console.WriteLine(classifyQueueResponseText);

                // Verificar se o episódio está na fila de classificação
                if (classifyQueueResponseText.Contains(idEpisode))
                {
                    // Evadir episódio
                    string conclusionNote = "Nota de conclusão sobre a evasão do episódio";
                    string evictData = "{\"conclusionNote\":\"" + conclusionNote + "\"}";
                    StringContent evictContent = new StringContent(evictData, Encoding.UTF8, "application/json");
                    HttpResponseMessage evictResponse = await client.PutAsync(evictUrl, evictContent);

                    if (evictResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Episódio evadido com sucesso.");
                        return; // Encerra o programa
                    }
                    else
                    {
                        Console.WriteLine("Falha ao evadir episódio. Código de status: " + evictResponse.StatusCode);
                        return; // Encerra o programa
                    }
                }

                Console.WriteLine("Episódio não encontrado na fila de classificação. Aguardando " + waitTimeInSeconds + " segundos...");
                await Task.Delay(waitTimeInSeconds * 1000); // Espera waitTimeInSeconds segundos
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
