public class WeatherQuery
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Duration
    {
        get
        {
            if (EndTime < StartTime)
            {
                return 0;
            }
            return (EndTime - StartTime).Days;
        }
    }
    public string ToString()
    {
        return $"Latitude: {Latitude}\n" +
               $"Longitude: {Longitude}\n" +
               $"Start time: {StartTime.ToShortDateString()}\n" +
               $"End time: {EndTime.ToShortDateString()}\n";
    }
}