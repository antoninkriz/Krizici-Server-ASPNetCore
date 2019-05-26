using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using HtmlAgilityPack;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using ImageMagick;
using KriziciServer.Common.Exceptions;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace KriziciServer.Services.Data.Services
{
    public class DataService : IDataService
    {
        private static readonly string DirDivider = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\" : @"/";
        private static readonly string TempPath = Path.GetTempPath() + DirDivider + "KriziciServer";
        private static readonly string ContentPath = Directory.GetCurrentDirectory() + DirDivider + "StaticContent" + DirDivider;
        private static readonly string[] RemotePdfNames = {"ucitele", "tridy", "ucebny"};
        private readonly DriveService _drive;

        public DataService(IClientService googleDrive)
        {
            _drive = googleDrive as DriveService;

            if (Directory.Exists(TempPath))
                return;

            Directory.CreateDirectory(TempPath);
        }

        public async Task<string> GetDataAsync(Types type)
        {
            string fileName;

            switch (type)
            {
                case Types.Data:
                    fileName = "data";
                    break;
                case Types.Contacts:
                    fileName = "contacts";
                    break;
                default:
                    throw new DataException("type_not_implemented");
            }

            var path = ContentPath + fileName + ".json";

            try
            {
                if (File.Exists(path)) return await File.ReadAllTextAsync(path);

                throw new DataException("file_not_exists");
            }
            catch (Exception e)
            {
                throw new DataException("cannot_read_file", e);
            }
        }

        public async Task<byte[]> GetContentAsync(string type, int id)
        {
            var path = new FileInfo(ContentPath + type + "-" + id + ".png");

            try
            {
                if (!File.Exists(path.ToString()))
                    throw new DataException("file_not_exists");

                return await File.ReadAllBytesAsync(path.ToString());
            }
            catch (Exception e)
            {
                throw new DataException("cannot_read_file", e);
            }
        }

        public async Task UpdateDataAsync()
        {
            // Prepare directories
            var dir = new DirectoryInfo(TempPath);
            if (!dir.Exists) dir.Create();

            var newDir = new DirectoryInfo(Directory.GetCurrentDirectory() + DirDivider + "StaticContent");
            if (!newDir.Exists)
                newDir.Create();

            // Get files from the TeamDrive
            var filesRequest = _drive.Files.List();
            filesRequest.IncludeTeamDriveItems = true;
            filesRequest.SupportsTeamDrives = true;
            filesRequest.TeamDriveId = "0ACwv3_2hEMByUk9PVA";
            filesRequest.Corpora = "teamDrive";
            filesRequest.OrderBy = "modifiedTime desc";
            filesRequest.Q = string.Format("mimeType='application/pdf' and ({0})", "name='rozvrh " + string.Join(".pdf' or name='rozvrh ", RemotePdfNames) + ".pdf'");
            filesRequest.Spaces = "drive";
            var files = await filesRequest.ExecuteAsync();

            var filesList = RemotePdfNames.Select(
                name =>
                    files.Files.FirstOrDefault(f => f.Name == $"rozvrh {name}.pdf") ?? throw new DataException("drive_files_not_ok")
            ).ToList();

            if (filesList.Count != 3)
                throw new DataException("drive_files_not_ok");

            // Parse contacts from the web async
            var parseContacts = ParseContacts();

            // Process those files async
            var tasks = new Dictionary<string, Task<string[]>>();
            foreach (var f in filesList)
            {
                var type = f.Name.Remove(0, 7).Split('.')[0];
                tasks.Add(type, ProcessFileAsync(f.Id, type));
            }

            var finishedTasks = new Dictionary<string, string[]>();
            foreach (var (key, value) in tasks)
            {
                finishedTasks.Add(key, await value);
            }

            // Move converted PDFs and JSONs to the StaticContent directory
            // Clean StaticContent directory first
            newDir.GetFiles().ToList().ForEach(f => f.Delete());

            var writeFile = File.WriteAllTextAsync(newDir + DirDivider + "data.json", JsonConvert.SerializeObject(finishedTasks));

            var transformedFiles = dir.GetFiles();
            foreach (var file in transformedFiles)
            {
                file.CopyTo(newDir + DirDivider + file.Name, true);
                file.Delete();
            }

            await writeFile;
            await File.WriteAllTextAsync(newDir + DirDivider + "contacts.json", JsonConvert.SerializeObject(await parseContacts));
        }

        private static async Task<List<Dictionary<string, string>>> ParseContacts()
        {
            var request = (HttpWebRequest) WebRequest.Create("http://vosaspsekrizik.cz/cs/kontakty/vyhledavani.ep/?name=&subject_id=0&action=search");
            request.Proxy = null;

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var stream = response.GetResponseStream();
                var data = string.Empty;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                
                if (stream != null)
                    using (var reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-2")))
                        data = await reader.ReadToEndAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(data);
                var table = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@id, 'content-right')]/table[contains(@class, 'common')]");

                var td = table.SelectNodes("//td");
                var teachers = new List<Dictionary<string, string>>();
                var teacher = new Dictionary<string, string>();

                if (data == string.Empty) return teachers;

                for (var i = 0; i < td.Count; i++)
                {
                    var text = string.Empty;
                    switch (i % 5)
                    {
                        case 0:
                            teacher = new Dictionary<string, string>();
                            text = "Jmeno";
                            break;
                        case 1:
                            text = "Zkratka";
                            break;
                        case 2:
                            text = "Telefon";
                            break;
                        case 3:
                            text = "Email";
                            break;
                        case 4:
                            text = "Předměty"; 
                            break;
                    }

                    teacher.Add(text, td[i].InnerText);

                    if (i % 5 == 4)
                        teachers.Add(teacher);
                }

                return teachers;
            }
        }

        private async Task<string[]> ProcessFileAsync(string fileId, string type)
        {
            var downloadUrl = $"https://www.googleapis.com/drive/v3/files/{fileId}?alt=media";
            var fileBytes = _drive.HttpClient.GetByteArrayAsync(downloadUrl);

            var convert = PdfConvertToPngAsync(fileBytes, type);
            var read = PdfReadAsync(fileBytes, type);

            await convert;

            return await read;
        }

        private static async Task<string[]> PdfReadAsync(Task<byte[]> fileBytes, string type)
        {
            var reader = new PdfReader(new MemoryStream(await fileBytes));
            var document = new PdfDocument(reader);
            
            var output = new StringWriter();

            for (var i = 1; i <= document.GetNumberOfPages(); i++)
                await output.WriteLineAsync(PdfTextExtractor.GetTextFromPage(document.GetPage(i), new SimpleTextExtractionStrategy()));

            var classes = new List<string>();
            var lines = output.ToString().Split('\n');

            for (var i = 0; i < lines.Length - 1; i++)
            {
                if (!lines[i].StartsWith("Vyšší odborná škola a Střední průmyslová škola elektrotechnická Františka Křižíka")) continue;

                switch (type)
                {
                    case "ucitele":
                    {
                        var tituly = new List<string>();
                        var jmena = new List<string>();

                        lines[++i].Split(' ').ToList().ForEach(l =>
                        {
                            var testL = l.ToLowerInvariant();
                            if (testL.Contains('.'))
                                tituly.Add(l);
                            else if (testL == "ing" || testL == "mgr" || testL == "rndr" || testL == "bc" || testL == "dis" || testL == "csc")
                                tituly.Add(l + '.');
                            else
                                jmena.Add(l);
                        });

                        var prijemni = jmena.Last();
                        jmena.Remove(prijemni);
                        var jmeno = string.Join(" ", jmena);
                        var titul = string.Join(" ", tituly);

                        var format = titul != string.Empty ? "{0} {1} ({2})" : "{0} {1}";

                        classes.Add(string.Format(format, prijemni, jmeno, titul));
                        break;
                    }
                    case "tridy":
                    case "ucebny":
                        classes.Add(lines[++i]);
                        break;
                }
            }

            return classes.ToArray();
        }

        private static async Task PdfConvertToPngAsync(Task<byte[]> fileBytes, string type)
        {
            var settings = new MagickReadSettings
            {
                ColorSpace = ColorSpace.Gray,
                BackgroundColor = MagickColor.FromRgb(0xff, 0xff, 0xff),
                Density = new Density(150)
            };
            var pathAndName = TempPath + DirDivider + type + "-";

            using (var images = new MagickImageCollection())
            {
                // Add all the pages of the pdf file to the collection
                images.Read(await fileBytes, settings);

                var page = 1;
                images.ToList().ForEach(
                    magickImage =>
                    {
                        magickImage.Trim();
                        magickImage.Density = new Density(150);
                        magickImage.Quality = 100;
                        magickImage.ColorType = ColorType.Grayscale;
                        magickImage.BackgroundColor = MagickColor.FromRgb(0xff, 0xff, 0xff);
                        magickImage.Alpha(AlphaOption.Remove);

                        // Write page to file that contains the page number
                        magickImage.Write(pathAndName + (page - 1) + ".png");

                        page++;
                    });
            }
        }
    }
}