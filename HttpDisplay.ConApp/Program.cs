using System.Net;

namespace HttpDisplay.ConApp
{
    internal partial class Program
    {
        static DateTime lastRequestTime = DateTime.Now;

        static async Task Main(string[] args)
        {
            var listener = new HttpListener();

            Console.Clear();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Listening on http://localhost:8080/ ...");

            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;

                // Check if more than one minute has passed since last request
                DateTime currentRequestTime = DateTime.Now;
                if ((currentRequestTime - lastRequestTime).TotalMinutes > 1)
                {
                    Console.Clear();
                    Console.WriteLine("Listening on http://localhost:8080/ ...");
                    Console.WriteLine("(Console cleared - more than 1 minute since last request)");
                    Console.WriteLine();
                }
                lastRequestTime = currentRequestTime;

                Console.WriteLine($"Received {request.HttpMethod} request for {request.Url}");

                // Check if PRINT: OFF is in headers
                bool shouldPrint = true;

                if (request?.Headers?.AllKeys != null)
                {
                    foreach (string key in request.Headers.AllKeys!)
                    {
                        string headerValue = request.Headers[key] ?? "";

                        if (key.Equals("PRINT", StringComparison.OrdinalIgnoreCase) &&
                            headerValue.Equals("OFF", StringComparison.OrdinalIgnoreCase))
                        {
                            shouldPrint = false;
                            break;
                        }
                    }
                }

                if (shouldPrint)
                {
                    Console.WriteLine($"Headers:");
                    if (request?.Headers?.AllKeys != null)
                    {
                        foreach (string key in request.Headers.AllKeys!)
                        {
                            Console.WriteLine($"  {key}: {request.Headers[key]}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Headers: (suppressed - PRINT: OFF found)");
                }
                Console.WriteLine(new string('-', 80));

                // Read and display request body
                if (request != null && request.HasEntityBody && request.InputStream != null)
                {
                    using var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding ?? System.Text.Encoding.UTF8);
                    string body = await reader.ReadToEndAsync();
                    Console.WriteLine($"Body:");
                    if (!string.IsNullOrEmpty(body))
                    {
                        Console.WriteLine($"  {body}");
                    }
                    else
                    {
                        Console.WriteLine("  (empty)");
                    }
                }
                else
                {
                    Console.WriteLine("Body: (no body)");
                }
                Console.WriteLine();

                // Respond to the client
                var responseString = "<html><body>Request received</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
        }
    }
}
