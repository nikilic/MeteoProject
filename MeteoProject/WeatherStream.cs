using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3
{
    class WeatherStream : IObservable<DailyWeather>
    {
        private readonly Subject<DailyWeather> _subject = new Subject<DailyWeather>();
        private readonly IScheduler _scheduler = new EventLoopScheduler();

        public IDisposable Subscribe(IObserver<DailyWeather> observer)
        {
            return _subject.ObserveOn(_scheduler).Subscribe(observer);
        }
        public async Task GetWeather(WeatherQuery weatherQuery)
        {
            try
            {
                using(HttpClient client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                    var response = await client.GetAsync("https://api.open-meteo.com/v1/forecast?" 
                                                                + "latitude=" + weatherQuery.Latitude 
                                                                + "&longitude=" + weatherQuery.Longitude 
                                                                + "&start_date=" + weatherQuery.StartTime.ToString("yyyy-MM-dd") 
                                                                + "&end_date=" + weatherQuery.EndTime.ToString("yyyy-MM-dd") 
                                                                + "&daily=temperature_2m_max,temperature_2m_min,uv_index_max"
                                                                + "&hourly=temperature_2m");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (responseContent == null)
                    {
                        throw new Exception("No results found");
                    }

                    JObject json = Newtonsoft.Json.Linq.JObject.Parse(responseContent);
                    if (json==null)
                    {
                       throw new Exception("No results found");
                    }

                    var temperatures = json["hourly"]!["temperature_2m"];

                    for (int i = 0; i < weatherQuery.Duration; i++)
                    {
                        if (json["daily"]!["time"]![i] == null ||
                            json["daily"]!["temperature_2m_max"]![i] == null ||
                            json["daily"]!["temperature_2m_min"]![i] == null ||
                            json["daily"]!["uv_index_max"]![i] == null)
                        {
                            continue;
                        }

                        double temperature_2m_sum = 0;
                        for (int j = i*24; j < (i+1)*24; j++) {
                            temperature_2m_sum += (double)temperatures[j];
                        }

                        DailyWeather dailyWeather = new DailyWeather
                        {
                            WeatherQuery = weatherQuery,
                            Date = DateTime.Parse((string)json["daily"]!["time"]![i]!),
                            MaxTemperature = (double)json["daily"]!["temperature_2m_max"]![i]!,
                            AvgTemperature = temperature_2m_sum / 24,
                            MinTemperature = (double)json["daily"]!["temperature_2m_min"]![i]!,
                            UvIndex = (double)json["daily"]!["uv_index_max"]![i]!
                        };
                        Console.WriteLine(dailyWeather);
                        _subject.OnNext(dailyWeather);
                    }
                }
                _subject.OnCompleted();
            }
            catch (Exception e)
            {
                _subject.OnError(e);
                Console.WriteLine(e.ToString());
            }
        }
    }
}
