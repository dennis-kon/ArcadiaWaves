### **Project Overview: Simple Web Server in C#**

This project demonstrates the implementation of a simple web server using the `HttpListener` class in C#. The web server can handle basic HTTP requests (`GET`, `POST`), route requests to different paths, handle various content types like JSON and form-data, and log requests and errors for debugging and monitoring. The project is ideal for learning about building web servers, handling HTTP requests, and adding essential features such as routing, logging, and error handling.

### **Key Features:**

1. **Basic HTTP Server Using `HttpListener`:**
   - A lightweight HTTP server that listens on specified URLs (`http://localhost:8080/` by default).
   - Can handle both `GET` and `POST` HTTP methods.

2. **Routing Support:**
   - Different URL paths are handled differently based on `request.Url.AbsolutePath`.
   - Paths include:
     - `/`: Home page.
     - `/about`: About page.
     - `/contact`: Contact page with form submission support.
   - Routes to custom 404 error pages if a path is not found.

3. **HTTP Method Handling (`GET`, `POST`):**
   - Handles `GET` requests to serve static HTML content.
   - Handles `POST` requests to process form submissions and JSON API requests.
   - Custom handling of HTTP methods with specific responses (e.g., 405 for unsupported methods).

4. **JSON Parsing for API Requests:**
   - If a `POST` request is sent with `Content-Type: application/json`, the server will parse the JSON body using `System.Text.Json`.
   - The server responds by displaying the parsed JSON back in the response.

5. **Robust Form Data Parsing:**
   - Supports `application/x-www-form-urlencoded` for traditional form submissions.
   - Supports `multipart/form-data` for handling file uploads and complex forms.

6. **Custom Error Pages for Status Codes:**
   - Custom HTML error pages for specific status codes, including:
     - **404**: Not Found – Returned when a path doesn't exist.
     - **405**: Method Not Allowed – Returned for unsupported HTTP methods.
     - **415**: Unsupported Media Type – Returned for unsupported content types.
     - **500**: Internal Server Error – Returned when an unhandled exception occurs.

7. **Logging for Debugging and Monitoring:**
   - Logs server events, including:
     - Server start/stop events.
     - Incoming requests (method, URL, headers).
     - JSON and form data submitted in POST requests.
     - Errors and exceptions.
   - Logs are written both to the console and a file (`webserver.log`).
   - Logs include timestamps to track when events occur.

### **Detailed Feature Breakdown:**

1. **`HttpListener` Web Server:**
   - The core of the project, `HttpListener`, allows the server to listen on specified URLs and handle incoming HTTP requests.
   - The server runs asynchronously, using tasks to handle multiple requests without blocking.

2. **Routing and Path Handling:**
   - The server inspects the `request.Url.AbsolutePath` and uses a switch-case statement to route requests to appropriate handlers.
   - For each route, different content is served:
     - `/`: Returns the home page.
     - `/about`: Returns the about page.
     - `/contact`: Serves a form and processes form submissions.

3. **Handling Different HTTP Methods:**
   - **GET Requests**: Used to serve static pages like the home, about, and contact pages.
   - **POST Requests**: Used to handle form submissions and API requests.
   - For `POST` requests, the server checks the `Content-Type` header to decide how to process the request (e.g., JSON parsing or form-data handling).

4. **Handling JSON Data:**
   - When the `Content-Type` is `application/json`, the server reads the body, parses the JSON, and responds with the parsed data.
   - Uses `System.Text.Json` for efficient and type-safe JSON parsing.

5. **Handling Form Submissions:**
   - For traditional form submissions (`application/x-www-form-urlencoded`), the server parses form data using `HttpUtility.ParseQueryString`.
   - For file uploads (`multipart/form-data`), the server handles form parts using the boundary defined in the content-type header. This feature is simplified in the current version but can be extended to handle actual file uploads.

6. **Custom Error Handling:**
   - Custom HTML error pages are sent when a particular status code is encountered:
     - **404 Not Found**: Returned if the requested URL path doesn't exist.
     - **405 Method Not Allowed**: Returned for unsupported HTTP methods.
     - **415 Unsupported Media Type**: Returned for unrecognized or unsupported content types in `POST` requests.
     - **500 Internal Server Error**: Returned when an unhandled exception occurs, such as server crashes.

7. **Logging for Monitoring and Debugging:**
   - **Request Logging**: Logs details of each incoming request, including the URL, HTTP method, and headers.
   - **Data Logging**: Logs JSON or form data received in `POST` requests for easier debugging.
   - **Error Logging**: Catches and logs exceptions to help with debugging server crashes or unexpected errors.
   - Logs are saved to both the console and a log file (`webserver.log`).

### **How to Run the Project:**

1. **Build and Run the Server:**
   - Compile the C# code using an IDE like Visual Studio or the .NET CLI.
   - Run the program. The server will start listening on `http://localhost:8080/`.

2. **Test the Web Server:**
   - Open a web browser and navigate to:
     - `http://localhost:8080/` to see the home page.
     - `http://localhost:8080/about` to see the about page.
     - `http://localhost:8080/contact` to submit a form.
   - Send `POST` requests using a tool like **Postman** or **cURL**:
     - Submit JSON data to `http://localhost:8080/contact` with the `Content-Type: application/json` header.
     - Submit form-data (e.g., file uploads) to `http://localhost:8080/contact` with the `Content-Type: multipart/form-data` header.

3. **Monitor Logs:**
   - View the logs in the console or open the `webserver.log` file to see the recorded events and errors.

### **Future Enhancements:**
1. **Support for File Uploads**: Extend the multipart form-data handler to properly save uploaded files to the server.
2. **Middleware Support**: Introduce middleware-like functionality for pre-processing requests (e.g., authentication, rate limiting).
3. **SSL Support**: Add support for HTTPS to serve content securely.
4. **Asynchronous Request Processing**: Further optimize handling of multiple concurrent requests using advanced async/await patterns.

---

### **Conclusion:**
This project provides a functional, simple web server with essential features such as routing, HTTP method handling, JSON parsing, form-data handling, custom error handling, and logging. It is a solid starting point for anyone looking to understand the basics of web servers in C# and can be extended with additional features as needed.
