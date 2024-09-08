### Features:

- **HTTP Server Initialization**:
  - **`HttpListener`**: Uses `HttpListener` to listen for HTTP requests on specified prefixes.
  - **Prefix Configuration**: Allows configuration of URL prefixes that the server will listen to.
  - **Logging Setup**: Configures `Trace` listeners to log to both the console and a file (`webserver.log`).

- **Server Lifecycle Management**:
  - **Start**: Starts the HTTP listener and begins handling incoming connections asynchronously.
  - **Stop**: Stops and closes the HTTP listener, and releases resources.

- **Request Handling**:
  - **Asynchronous Processing**: Handles incoming requests asynchronously using `Task.Run` and `await`.
  - **Request Methods**:
    - **GET**: Handles `GET` requests and serves different pages based on the URL path (`/`, `/about`, `/contact`).
    - **POST**: Handles `POST` requests for form submissions, JSON data, and multipart form-data.

- **Response Handling**:
  - **Send Response**: Sends HTTP responses with appropriate content and status codes.
  - **Error Handling**: Sends error responses with status codes for different types of errors (405, 404, 415, 500).

- **Form Data Handling**:
  - **JSON Data**: Reads and processes JSON data from `POST` requests, deserializes it, and sends back a formatted response.
  - **Multipart Form-Data**: Handles `multipart/form-data` for file uploads and form submissions, though parsing is simplified.
  - **URL-Encoded Form Data**: Processes `application/x-www-form-urlencoded` data using `HttpUtility.ParseQueryString` to parse form data.

- **Logging**:
  - **Request Logging**: Logs detailed information about each request, including URL, method, and headers.
  - **Response Logging**: Logs details about the responses sent by the server.

- **Resource Management**:
  - **`IDisposable` Implementation**: Implements `IDisposable` to properly release resources (e.g., stopping and closing the `HttpListener`).
  - **Dispose Pattern**: Uses the Dispose pattern to handle resource cleanup and prevent resource leaks.

- **Boundary Extraction**:
  - **Multipart Boundary**: Extracts boundary information from the `Content-Type` header for multipart form-data processing.

- **Error Handling**:
  - **Exception Logging**: Logs exceptions and sends a 500 Internal Server Error response for unexpected errors.

- **Main Method**:
  - **Entry Point**: Provides a `Main` method that initializes and runs the `SimpleWebServer` on `http://localhost:8080/`, and stops it when Enter is pressed.

This summary encapsulates the key functionalities and design features of the `SimpleWebServer` implementation, providing a comprehensive overview of its capabilities and operations.
