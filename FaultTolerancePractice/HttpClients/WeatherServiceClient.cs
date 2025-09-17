namespace FaultTolerancePractice.HttpClients
{
    public class WeatherServiceClient
    {
        private readonly HttpClient _httpClient;

        public WeatherServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WeatherForecastDto>?> GetWeatherForecast()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/WeatherForecast");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");

                        return new List<WeatherForecastDto>();
                    }
                }

                var result = await response.Content.ReadFromJsonAsync<List<WeatherForecastDto>>();

                return result;
            }
            catch (Exception ex)
            {
                return new List<WeatherForecastDto>();
            }
        }

    }

    public class WeatherForecastDto
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF { get; set; }

        public string? Summary { get; set; }
    }
}
