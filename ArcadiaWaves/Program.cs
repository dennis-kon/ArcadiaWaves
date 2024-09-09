To create a web UI for managing the features of the web server, we will need to extend the server's capabilities to serve dynamic HTML content. This web UI will allow you to manage features like:

1. **Starting/Stopping the Server**: Control the server's state (running or stopped).
2. **Viewing and Clearing Logs**: Display logs, and provide the ability to clear them.
3. **Uploading and Managing Files**: Provide a file upload feature (via a form).
4. **Adjusting Server Configuration**: Update the server's listening port or other settings dynamically.

The general idea is to:
- Create an admin dashboard served on a specific route (e.g., `/admin`).
- Provide various functionalities like log viewing, server control, and configuration updates via this dashboard.
- Implement form handling and file upload mechanisms to interact with the server.

### Plan:
1. Add a new route `/admin` to serve the UI.
2. Create a basic HTML template for the admin dashboard.
3. Provide functionality to view logs, stop/start the server, and upload files through this dashboard.
4. Adjust the backend to handle these interactions (e.g., POST requests for server control).

### **Updated Code with Admin Web UI:**

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web; // For form-data parsing

class SimpleWebServer
{
    private readonly HttpListener _listener = new HttpListener();
    private readonly string logFilePath = "webserver.log"; // Log file path

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
        _listener.Stop();
        _listener.Close();
        Log("Web server stopped.");
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
                responseString = "<html><body><h1>Home Page - GET</h1></body></html>";
                break;

            case "/about":
                responseString = "<html><body><h1>About Page - GET</h1></body></html>";
                break;

            case "/contact":
                responseString = "<html><body><h1>Contact Page - GET</h1><form method='post' enctype='multipart/form-data'><input type='text' name='message' placeholder='Enter your message'/><br/><input type='file' name='file'/><br/><input type='submit' value='Submit'/></form></body></html>";
                break;

            case "/admin":
                responseString = await GenerateAdminDashboard();
                break;

            case "/logs":
                responseString = File.ReadAllText(logFilePath);
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
        if (request.Url.AbsolutePath == "/admin/action")
        {
            string responseString;

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string body = await reader.ReadToEndAsync();
                var formData = HttpUtility.ParseQueryString(body);
                string action = formData["action"];

                if (action == "stop_server")
                {
                    responseString = "<html><body><h1>Server Stopped</h1></body></html>";
                    await WriteResponse(response, responseString, "text/html");
                    Stop(); // Stop the server after responding
                }
                else if (action == "clear_logs")
                {
                    File.WriteAllText(logFilePath, string.Empty); // Clear the logs
                    responseString = "<html><body><h1>Logs Cleared</h1></body></html>";
                    await WriteResponse(response, responseString, "text/html");
                }
            }
        }
        else if (request.Url.AbsolutePath == "/contact")
        {
            if (request.ContentType == "application/json")
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string body = await reader.ReadToEndAsync();
                    Log($"Received JSON data: {body}");

                    var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
                    string responseString = $"<html><body><h1>Received JSON data</h1><pre>{JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true })}</pre></body></html>";
                    await WriteResponse(response, responseString, "text/html");
                }
            }
            else if (request.ContentType.Contains("multipart/form-data"))
            {
                var boundary = GetBoundaryFromContentType(request.ContentType);
                var multipartFormData = await ParseMultipartFormDataAsync(request, boundary);
                Log($"Received multipart form-data: {multipartFormData}");

                string responseString = $"<html><body><h1>Form Data Received</h1><pre>{multipartFormData}</pre></body></html>";
                await WriteResponse(response, responseString, "text/html");
            }
        }
        else
        {
            SendErrorResponse(response, 404, "Page Not Found");
        }
    }

    private async Task<string> GenerateAdminDashboard()
    {
        string logs = File.ReadAllText(logFilePath);
        return $@"
            <html>
            <body>
                <h1>Admin Dashboard</h1>
                <h2>Server Control</h2>
                <form method='post' action='/admin/action'>
                    <input type='hidden' name='action' value='stop_server'/>
                    <button type='submit'>Stop Server</button>
                </form>
                <h2>Logs</h2>
                <pre>{logs}</pre>
                <form method='post' action='/admin/action'>
                    <input type='hidden' name='action' value='clear_logs'/>
                    <button type='submit'>Clear Logs</button>
                </form>
            </body>
            </html>";
    }

    private async Task WriteResponse(HttpListenerResponse response, string responseString, string contentType)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.ContentType = contentType;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        Log($"Sent POST response for {response.Request.Url.AbsolutePath}");
    }

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
        string boundary = contentType.Split("boundary=")[1];
        return "--" + boundary;
    }

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

    private void LogRequestDetails(HttpListenerRequest request)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"--- Request Details ---");
        sb.AppendLine($"URL: {request.Url}");
        sb.AppendLine($"Method: {request.HttpMethod}");
        sb.AppendLine
