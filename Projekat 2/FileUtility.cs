using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime;
using System.Net;
using System.Runtime.Caching;

namespace Projekat_2
{
    public class FileUtility
    {
        public HttpServer server;

        public FileUtility(HttpServer server)
        {
            this.server = server;
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
                    server.returnResponse(context, HttpStatusCode.OK, "File is not found", paragraph);
                    throw new Exception();
                }
                catch(DirectoryNotFoundException ex)
                {
                    Console.Write("\nStatus: Directory not found with specified name\n");
                    server.returnResponse(context, HttpStatusCode.InternalServerError, "Directory not found");
                    throw new Exception();
                }
                catch(Exception ex)
                {
                    Console.Write("\nStatus: Some internal error happened\n");
                    string paragraph = HttpServer.createHeading("Something's wrong");
                    server.returnResponse(context, HttpStatusCode.InternalServerError, paragraph);
                    throw new Exception();
                }
        }

        public static string GetFileFromUrl(string url)
        {
            int indexOfLastDividor = url.LastIndexOf('/');
            return url.Substring(indexOfLastDividor + 1);
        }

       
    }
}