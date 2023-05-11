using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime.Caching;

namespace Projekat_1
{
    enum FileType{
        Binary,
        Text
    }
  
    public class HttpServer
    {
        public HttpListener listener;
        private FileLogic logic;
        private MemoryCache cache;
        private CacheItemPolicy policy;
        public HttpServer(string directory)
        {
            listener = new HttpListener();
            logic = new FileLogic();
            
            string[] files = Directory.GetFiles(directory);
            foreach(string file in files)
            {
                logic.add(GetFileFromPath(file));
            }
            cache = new MemoryCache("file cache");
            policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(120.0);
        }

        public static string GetFileFromUrl(string url)
        {
            int indexOfLastDividor = url.LastIndexOf('/');
            return url.Substring(indexOfLastDividor + 1);
        }

        public static string GetFileFromPath(string path)
        {
            int indexOfLastDividor = path.LastIndexOf('\\');
            return path.Substring(indexOfLastDividor + 1);
        }
        public static string GetExtension(string file)
        {
            int pointIndex = file.LastIndexOf('.');
            return file.Substring(pointIndex + 1);
        }
        public static string ConvertFileNameExtension(string file, string extension)
        {
            if (!file.Contains('.'))
            {
                return file;
            }
            int pointIndex = file.IndexOf('.');
            return file.Substring(0, pointIndex + 1) + extension;
        }

      

        public void returnResponse(HttpListenerContext context, HttpStatusCode statusCode, string description, string body = null)
        {
            HttpListenerResponse response = context.Response;
            response.StatusCode = (int)statusCode;
            response.StatusDescription = description;
            byte[] buffer;
            if (body != null)
            {
                buffer = Encoding.UTF8.GetBytes("<HTML><BODY>" + body + "</BODY></HTML>");
                response.OutputStream.Write(buffer);
            }       
            response.OutputStream.Close();
        }

        public void returnResponseWithFile(HttpListenerContext context, HttpStatusCode statusCode,
                 string description, string body = null, byte[] content = null)
        {
            HttpListenerResponse response = context.Response;
            response.StatusCode = (int)statusCode;
            response.StatusDescription = description;
            byte[] buffer;
            if (body != null)
            {
                buffer = Encoding.UTF8.GetBytes("<HTML><BODY>" + body);
                response.OutputStream.Write(buffer);
                response.OutputStream.Write(content);
                response.OutputStream.Write(Encoding.UTF8.GetBytes("</BODY></HTML>"));
            }       
            
            
        }

        public static string createParagraph(string text)
        {
            return "<p>" + text + "</p>";
        }
        public static string createHeading(string text)
        {
            return "<h2>" + text + "</h2>";
        }
        public static string unorderedListOfAllFiles(string directory)
        {
            string returnHtmlString = "<ul>";
            foreach (string fileName in Directory.GetFiles(directory))
            {
                returnHtmlString += $"<li>{HttpServer.GetFileFromPath(fileName)}</li>";
            }
            returnHtmlString += "</ul>";

            return returnHtmlString;
        }

        public void openStream(HttpListenerContext context, string fileName, out FileStream streamToOpen)   //throws exception
        {
                try
                {
                    streamToOpen = new FileStream($".\\Resources\\{fileName}", FileMode.Open, FileAccess.Read);
                }
                catch(FileNotFoundException ex){
                    Console.Write("\nStatus: File not found with specified name\n");
                    string paragraph = HttpServer.createHeading("There isn't a file with specified name");
                    paragraph += HttpServer.createParagraph("List of available files is ") + HttpServer.unorderedListOfAllFiles(".\\Resources\\");
                    this.returnResponse(context, HttpStatusCode.OK, "File is not found", paragraph);
                    throw new Exception();
                }
                catch(DirectoryNotFoundException ex)
                {
                    Console.Write("\nStatus: Directory not found with specified name\n");
                    this.returnResponse(context, HttpStatusCode.InternalServerError, "Directory not found");
                    throw new Exception();
                }
                catch(Exception ex)
                {
                    Console.Write("\nStatus: Some internal error happened\n");
                    string paragraph = HttpServer.createHeading("Something's wrong");
                    this.returnResponse(context, HttpStatusCode.InternalServerError, paragraph);
                    throw new Exception();
                }
        }

        public void performConverting(HttpListenerContext context, FileStream stream, string fileName)
        {
            HttpListenerResponse response = context.Response;
            if (fileName.EndsWith(".txt"))
            {
                string binFileName = HttpServer.ConvertFileNameExtension(fileName, "bin");
                
                byte[] buffer;


                FileStream streamCreate;

                if (stream != null)     //stream ce biti null kad postoji sadrzaj fajla u kesu, metoda handleRequest
                {
                    lock(logic.getElement(fileName))
                    {
                        StreamReader reader = new StreamReader(stream);
                        long length = stream.Length;
                        buffer = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                        CacheItem item = new CacheItem(fileName, buffer);
                        cache.Add(item, policy);
                        reader.Close();
                    }
                    stream.Close();
                }
                else{
                    buffer = (byte[])cache[fileName];
                }

                logic.add(binFileName);
                lock(logic.getElement(binFileName))
                {
                    streamCreate = new FileStream
                    ($".\\Resources\\{binFileName}", FileMode.OpenOrCreate, FileAccess.Write);
                    BinaryWriter writer = new BinaryWriter(streamCreate);
                    writer.Write(buffer);
                    writer.Close();

                    streamCreate.Close();
                }
                
                
                
                string returnString = HttpServer.createHeading($"File is converted, created file name is {binFileName}");
                returnString += HttpServer.createParagraph("Content:");
                this.returnResponseWithFile(context, HttpStatusCode.OK, "Converted successfully", returnString, buffer);
                response.OutputStream.Close();  //salje response
                
                streamCreate.Close();
                Console.Write("\nStatus: File is converted, new file name is " + binFileName + "\n");
            }
            else if (fileName.EndsWith(".bin"))
            {
                string txtFileName = HttpServer.ConvertFileNameExtension(fileName, "txt");

                char[] buffer;
                FileStream streamCreate;


                if (stream != null)     //stream ce biti null kad postoji sadrzaj fajla u kesu, metoda handleRequest
                {
                    lock(logic.getElement(fileName))
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        long lengthChars = stream.Length / sizeof(char);
                        buffer = reader.ReadChars((int)lengthChars);
                        CacheItem item = new CacheItem(fileName, buffer);
                        cache.Add(item, policy);
                        reader.Close();

                    }
                    stream.Close();
                }
                else{
                    buffer = (char[])cache[fileName];
                }

                logic.add(txtFileName);
                lock(logic.getElement(txtFileName))
                {
                    streamCreate = new FileStream
                    ($".\\Resources\\{txtFileName}", FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(streamCreate);
                    writer.Write(buffer);
                    streamCreate.Close();
                }
                
                byte[] bufferBytes = Encoding.UTF8.GetBytes(buffer);
                
                string returnString = HttpServer.createHeading($"File is converted, created file name is {txtFileName}");
                returnString += HttpServer.createParagraph("Content:");
                this.returnResponseWithFile(context, HttpStatusCode.OK, "Converted successfully", returnString, bufferBytes);
                response.OutputStream.Close();  //salje response
                Console.Write("\nStatus: File is converted, new file name is " + txtFileName + "\n");

                
            }
            
        }

        public void handleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            Console.Write($"\n\nRequest made from client {request.RemoteEndPoint.ToString()}: ");
            
            string url = request.RawUrl;
            string fileName = HttpServer.GetFileFromUrl(url);
            HttpListenerResponse response = context.Response;
            response.ContentType = "text/html";

            if (HttpServer.GetExtension(fileName) != "txt" && HttpServer.GetExtension(fileName) != "bin")
            {
                Console.Write("\nStatus: bad extension");
                this.returnResponse(context, HttpStatusCode.NotFound, "File not found");
                return;
            }

            FileStream stream = null;
            if (!cache.Contains(fileName))
            {
                
                    try
                    {
                        this.openStream(context, fileName, out stream);
                    }
                    catch(Exception ex)
                    {
                        return;
                    }
            }

            performConverting(context, stream, fileName);
        }



        public void StartServer(string[] prefixes)
        {
            if (listener.IsListening)
            {
                return; //already listening;
            }
            foreach (string p in prefixes)
            {
                listener.Prefixes.Add(p);
            }

            listener.Start();
            Console.WriteLine("Ready for converting...");
            
            

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ThreadPool.QueueUserWorkItem((state) => handleRequest(context));
            }

        }

        public void StopServer()
        {
            if (listener.IsListening)
            {
                listener.Stop();
            }
        }
    }
}