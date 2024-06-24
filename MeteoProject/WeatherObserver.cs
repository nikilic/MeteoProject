namespace Projekat3
{
    public class WeatherObserver: IObserver<DailyWeather>
    {
        private static readonly string filePath = "weather.log";
        private static readonly object fileLock = new object();
        public string content;
        public bool created;

        public WeatherObserver()
        {
            content = "";
            created = false;
        }

        public void OnNext(DailyWeather weather)
        {
            string value = weather.ToString();
            Console.WriteLine(value);
            content += $"{value}\n";

            string key = weather.WeatherQuery.Latitude + "-" + weather.WeatherQuery.Longitude + "-" + weather.Date.ToString();
            Cache.WriteToCache(key, weather);
        }

        public void OnError(Exception e)
        {

            Console.WriteLine($"Error: {e.Message}");
            content += $"{e.Message}\n";
            created = true;
        }

        public void OnCompleted()
        {
            Console.WriteLine($"Done");
            if(content == "")
            {
                content = "No results found";
            }
            lock (fileLock) {
                File.AppendAllText(filePath, content);
            }
            created = true;
        }
    }
}