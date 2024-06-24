public class DailyWeather
{
    public WeatherQuery WeatherQuery { get; set; }
    public DateTime Date { get; set; }
    public double MaxTemperature { get; set; }
    public double AvgTemperature { get; set; }
    public double MinTemperature { get; set; }
    public double UvIndex { get; set; }
    public override string ToString()
    {
        return $"Latitude: {WeatherQuery.Latitude} " +
               $"Longitude: {WeatherQuery.Longitude} " +
               $"Date: {Date.ToShortDateString()} " +
               $"Max temperature: {MaxTemperature} " +
               $"Avg temperature: {Math.Round(AvgTemperature, 2)} " +
               $"Min temperature: {MinTemperature} " +
               $"UV index: {UvIndex} ";
    }
}