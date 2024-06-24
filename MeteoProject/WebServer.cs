using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3
{
    class WebServer
    {
        private static readonly string REQUEST_OK = "OK";
        private static readonly string REQUEST_NOT_GET = "Request method is not GET.";
        private static readonly string REQUEST_NO_COORDS = "Invalid query.";
        private string _urlServer;
        private object _lockConsole = new object();
        private int _requestCount = 0;

        public WebServer(string urlServer)
        {
            _urlServer = urlServer;
        }
        public async Task Run()
        {
            Console.WriteLine("WebServer started.");

            Thread server = new Thread(async ()=>
            {
                using (HttpListener listener = new HttpListener())
                {
                    string urlListener = _urlServer;
                    listener.Prefixes.Add(urlListener);
                    listener.Start();

                    Console.WriteLine("Server is listening on:" + urlListener);
                    while (listener.IsListening)
                    {
                        HttpListenerContext context = await listener.GetContextAsync();
                        _= ProcessRequestAsync(context,_requestCount++);
                    }
                }
            });
            server.Start();
            server.Join();
        }
        private async Task ProcessRequestAsync(HttpListenerContext con,int requestNumber)
        {
            try
            {
                HttpListenerContext context = (HttpListenerContext)con;
                if (context == null)
                    throw new Exception("Can't parse given object to HttpListenerContext object!");
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                logRequest(request, requestNumber);
                string validation = ValidateRequest(context);
                if (!validation.Equals(REQUEST_OK))
                {
                    WriteToConsole(validation);
                    SendResponse(response, requestNumber, REQUEST_OK, validation);
                    return;
                }

                WeatherQuery query = GetWeatherQuery(request);
                if (query == null)
                {
                    SendResponse(response, requestNumber, REQUEST_NO_COORDS, REQUEST_NO_COORDS);
                    return;
                }
                Console.WriteLine(query.ToString());

                WeatherObserver observer = new WeatherObserver();
                WeatherStream weatherStream = new WeatherStream();
                var subscription = weatherStream.Subscribe(observer);
                await weatherStream.GetWeather(query);
                subscription.Dispose();

                SendResponse(response, requestNumber, REQUEST_OK, observer.content);

            }
            catch(Exception e)
            {
                WriteToConsole(e.Message);
            }
           
        }

        private WeatherQuery GetWeatherQuery(HttpListenerRequest req)
        {
            WeatherQuery query = new WeatherQuery();
            string lat = req.QueryString.Get("lat");
            string lng = req.QueryString.Get("lng");
            string startTime = req.QueryString.Get("start_time");
            string endTime = req.QueryString.Get("end_time");

            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng) || string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            {
                return null;
            }

            query.Latitude = double.Parse(lat);
            query.Longitude = double.Parse(lng);
            query.StartTime = DateTime.Parse(startTime);
            query.EndTime = DateTime.Parse(endTime);

            if (query.Duration <= 0) {
                return null;
            }

            return query;
        }

        private async Task SendResponse(HttpListenerResponse response, int responseId, string error, string responseString)
        {
            try
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                if (error.Equals(REQUEST_NOT_GET) || error.Equals(REQUEST_NO_COORDS))
                {
                    response.StatusCode = 400;
                }
                else
                {
                    response.StatusCode = 200;
                }
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                output.Close();
                logResponse(response, responseId);
            }
            catch(Exception e)
            {
                WriteToConsole(e.Message);
            }
        }
        private string ValidateRequest(HttpListenerContext context)
        {
            if (!context.Request.HttpMethod.Equals("GET"))
                return REQUEST_NOT_GET;
            return REQUEST_OK;
        }
        private void logRequest(HttpListenerRequest request, int requestId)
        {
            lock(_lockConsole)
            {
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("Request number: " + requestId);
                Console.WriteLine("Request URL: " + request.Url.ToString());
                Console.WriteLine("Request HTTP method: " + request.HttpMethod);
                Console.WriteLine("Request User-agent: " + request.UserAgent);
                Console.WriteLine("Request Content-type: " + request.ContentType);
                Console.WriteLine("Request Content-length: " + request.ContentLength64);
                Console.WriteLine("Request Accept-encoding: " + request.Headers["Accept-encoding"]);
                Console.WriteLine("Request Accept-language: " + request.Headers["Accept-language"]);
                Console.WriteLine("----------------------------------------------------");
            }
        }
        private void logResponse(HttpListenerResponse response,int responseId)
        {
            lock (_lockConsole)
            {
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("Response number: " + responseId);
                Console.WriteLine("Response status code: " + response.StatusCode);
                Console.WriteLine("Response status description: " + response.StatusDescription);
                Console.WriteLine("Response Content-type: " + response.ContentType);
                Console.WriteLine("Response Content-length: " + response.ContentLength64);
                Console.WriteLine("Response Content-encoding: " + response.ContentEncoding);
                Console.WriteLine("----------------------------------------------------");
            }
        }
        private void WriteToConsole(string message)
        {
            lock (_lockConsole)
            {
                Console.WriteLine(message);
            }
        }
    }
}
