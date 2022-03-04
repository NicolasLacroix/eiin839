class RenduClient
{
    static async Task Main()
    {
        var client = new HttpClient();
        int number = Convert.ToInt32(Console.ReadLine());
        var result = await client.GetAsync($"http://localhost:8080/m2m/incr?param1={number}");
        if (result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"Error : {result.ReasonPhrase}");
            return;
        }
        Console.WriteLine($"increment result is {result.Content.ReadAsStringAsync()}");
    }
}