using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace BasicWebServer
{
    class Mymethods
    {
        private static void Main(string[] args)
        {

            //if HttpListener is not supported by the Framework
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("A more recent Windows version is required to use the HttpListener class.");
                return;
            }


            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            if (args.Length != 0)
            {
                foreach (string s in args)
                {
                    listener.Prefixes.Add(s);
                }
            }
            else
            {
                Console.WriteLine("Syntax error: the call must contain at least one web server url as argument");
            }
            listener.Start();

            // get args 
            foreach (string s in args)
            {
                Console.WriteLine("Listening for connections on " + s);
            }

            // Trap Ctrl-C on console to exit 
            Console.CancelKeyPress += delegate {
                // call methods to close socket and exit
                listener.Stop();
                listener.Close();
                Environment.Exit(0);
            };


            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string documentContents;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                    }
                }
                string refPath = request.Url.Segments[request.Url.Segments.Length - 2];
                if (refPath != "td02/" && refPath != "m2m/")
                {
                    byte[] errorBuffer = Encoding.UTF8.GetBytes("<html><body>Unsupported url</body></html>");
                    // Get a response stream and write the response to it.
                    HttpListenerResponse errorResponse = context.Response;
                    errorResponse.ContentLength64 = errorBuffer.Length;
                    System.IO.Stream errorOutput = errorResponse.OutputStream;
                    errorOutput.Write(errorBuffer, 0, errorBuffer.Length);
                    // You must close the output stream.
                    errorOutput.Close();
                    continue;
                }

                Console.WriteLine($"Received request for {request.RawUrl}");


                //parse params in url
                object[] parameters;
                if (refPath == "td02/")
                {
                    parameters = new object[2] { HttpUtility.ParseQueryString(request.Url.Query).Get("param1"), HttpUtility.ParseQueryString(request.Url.Query).Get("param2") };
                } else
                {
                    parameters = new object[1] { Convert.ToInt32(HttpUtility.ParseQueryString(request.Url.Query).Get("param1")) };
                }

                HttpListenerResponse response = context.Response;

                Type type = typeof(Mymethods);
                MethodInfo method = type.GetMethod(request.Url.Segments[request.Url.Segments.Length - 1]);
                Mymethods c = new Mymethods();

                string responseString = method.Invoke(c, parameters).ToString();
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
        }

        // http://localhost:8080/td02/add?param1=asterix&param2=obelix
        public string add(string username, string newFriend)
        {
            return $"<html><body>{username} added new friend : {newFriend}</body></html>";
        }

        // http://localhost:8080/td02/send?param1=obelix&param2=hello
        public string send(string username, string message)
        {
            return $"<html><body>[{username}] {message}</body></html>";
        }

        // http://localhost:8080/td02/call?param1=asterix&param2=3637
        public string call(string source, string phoneNumber)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"..\..\..\call\bin\Debug\net6.0\call.exe";
            start.Arguments = $"{source} {phoneNumber}";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    return reader.ReadToEnd();
                }
            }
        }

        // http://localhost:8080/m2m/incr?param1=1
        public int incr(int number)
        {
            return number + 1;
        }
    }
}
