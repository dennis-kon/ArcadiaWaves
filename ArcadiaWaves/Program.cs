using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web; // For form-data parsing

class SimpleWebServer : IDisposable
{
    private readonly HttpListener _listener = new HttpListener();
    private readonly string logFilePath = "webserver.log"; // Log file path
    private bool _disposed = false; // To detect redundant calls

    public SimpleWebServer(string[] prefixes)
    {
        if (!HttpListener.IsSupported)
        {
            throw new NotSupportedException("HttpListener is not supported on this system.");
        }

        foreach (string prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }

        // Set up Trace listeners to log both to console and a file
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
        Trace.AutoFlush = true; // Ensure logs are written immediately
    }

    public void Start()
    {
        _listener.Start();
        Log("Web server started... Listening for connections...");
        Task.Run(() => HandleIncomingConnections());
    }

    public void Stop()
    {
        Dispose(); // Ensure that resources are released
    }

    private async Task HandleIncomingConnections()
    {
        while (_listener.IsListening)
        {
            try
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                Log($"Received {request.HttpMethod} request for: {request.Url.AbsolutePath}");
                LogRequestDetails(request);

                if (request.HttpMethod == "GET")
                {
                    await HandleGetRequest(request, response);
                }
                else if (request.HttpMethod == "POST")
                {
                    await HandlePostRequest(request, response);
                }
                else
                {
                    SendErrorResponse(response, 405, "Method Not Allowed");
                }
            }
            catch (Exception ex)
            {
                Log($"Exception: {ex.Message}");
                HttpListenerResponse response = _listener.GetContext().Response;
                SendErrorResponse(response, 500, "Internal Server Error");
            }
        }
    }

    private async Task HandleGetRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        string responseString;
        switch (request.Url.AbsolutePath)
        {
            case "/":
                responseString = "<html><body><h1>Home Page - Welcome to ArcadiaWaves! </h1></body></html>";
                break;

            case "/about":
                responseString = "<html><body><h1>About Page - GET</h1></body></html>";
                break;

            case "/contact":
                responseString = "<html><body><h1>Contact Page - GET</h1><form method='post' enctype='multipart/form-data'><input type='text' name='message' placeholder='Enter your message'/><br/><input type='file' name='file'/><br/><input type='submit' value='Submit'/></form></body></html>";
                break;

            default:
                SendErrorResponse(response, 404, "Page Not Found");
                return;
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        Log($"Sent GET response for {request.Url.AbsolutePath}");
    }

    private async Task HandlePostRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (request.Url.AbsolutePath == "/contact")
        {
            if (request.ContentType == "application/json")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = await reader.ReadToEndAsync();
                    Log($"Received JSON data: {body}");

                    var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                    string responseString = $"<html><body><h1>Received JSON data</h1><pre>{JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true })}</pre></body></html>";
                    await WriteResponse(response, responseString, "text/html", request.Url.AbsolutePath);
                }
            }
            else if (request.ContentType.Contains("multipart/form-data"))
            {
                var boundary = GetBoundaryFromContentType(request.ContentType);
                var multipartFormData = await ParseMultipartFormDataAsync(request, boundary);
                Log($"Received multipart form-data: {multipartFormData}");

                string responseString = $"<html><body><h1>Form Data Received</h1><pre>{multipartFormData}</pre></body></html>";
                await WriteResponse(response, responseString, "text/html", request.Url.AbsolutePath);
            }
            else if (request.ContentType == "application/x-www-form-urlencoded")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = await reader.ReadToEndAsync();
                    Log($"Received form-urlencoded data: {body}");

                    var formData = HttpUtility.ParseQueryString(body); // Use HttpUtility for .NET Framework
                    string responseString = $"<html><body><h1>Form Data Received</h1><pre>{body}</pre></body></html>";
                    await WriteResponse(response, responseString, "text/html", request.Url.AbsolutePath);
                }
            }
            else
            {
                SendErrorResponse(response, 415, "Unsupported Media Type");
            }
        }
        else
        {
            SendErrorResponse(response, 404, "Page Not Found");
        }
    }

    // Helper method to write a response
    private async Task WriteResponse(HttpListenerResponse response, string responseString, string contentType, string requestPath)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.ContentType = contentType;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        Log($"Sent POST response for {requestPath}");
    }

    // Parse multipart/form-data content (simplified example)
    private async Task<string> ParseMultipartFormDataAsync(HttpListenerRequest request, string boundary)
    {
        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
        {
            string formData = await reader.ReadToEndAsync();
            return formData;  // This is a simple case, advanced parsing needed for production
        }
    }

    private string GetBoundaryFromContentType(string contentType)
    {
        // Extract the boundary from the content-type header
        string[] parts = contentType.Split(new[] { "boundary=" }, StringSplitOptions.None);
        if (parts.Length > 1)
        {
            return "--" + parts[1];
        }
        throw new ArgumentException("Boundary not found in content-type header.");
    }

    // Helper method for sending error responses
    private void SendErrorResponse(HttpListenerResponse response, int statusCode, string message)
    {
        response.StatusCode = statusCode;
        string responseString = $"<html><body><h1>{statusCode} - {message}</h1></body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
        Log($"Sent error response: {statusCode} - {message}");
    }

    // Helper method to log request details
    private void LogRequestDetails(HttpListenerRequest request)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"--- Request Details ---");
        sb.AppendLine($"URL: {request.Url}");
        sb.AppendLine($"Method: {request.HttpMethod}");
        sb.AppendLine($"Headers: ");
        foreach (string key in request.Headers.AllKeys)
        {
            sb.AppendLine($"{key}: {request.Headers[key]}");
        }
        Log(sb.ToString());
    }

    // Logging method to handle both console and file logging
    private void Log(string message)
    {
        Trace.WriteLine($"{DateTime.Now}: {message}");
    }

    // Implement IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (_listener != null)
                {
                    _listener.Stop();
                    _listener.Close();
                }
            }

            // Dispose unmanaged resources if any

            _disposed = true;
        }
    }

    public static void Main(string[] args)
    {
        string[] prefixes = { "http://localhost:8080/" };

        using (var server = new SimpleWebServer(prefixes))
        {
            server.Start();
            Console.WriteLine("Press Enter to stop the server...");
            Console.ReadLine();
            server.Stop();
        }
    }
}
