using Microsoft.AspNetCore.Mvc;
using System.IO;
using TestProject.Models;

namespace TestProject.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class FilesController : ControllerBase {

        private readonly ILogger<FilesController> _logger;
        private readonly string _baseFolder;

        public FilesController(ILogger<FilesController> logger, IConfiguration config) {
            _logger = logger;
            _baseFolder = config.GetValue<string>("BaseFolder")!;
        }

        [HttpGet("GetFolders")]
        public ActionResult<IEnumerable<Folder>> GetFolders(string node = "") {
            var folderList = new List<Folder>();

            try
            {
                var dirList = Directory.EnumerateDirectories(Path.Combine(this._baseFolder, node));
                foreach (var item in dirList)
                {
                    var newFolder = new Folder(Path.GetRelativePath(this._baseFolder, item));
                    try
                    {
                        if (Directory.GetDirectories(item).Length > 0)
                        {
                            newFolder.LoadOnDemand = true;
                        }
                        folderList.Add(newFolder);
                    }
                    catch(UnauthorizedAccessException)
                    {
                        //  If an error, the folder can't be accessed.  Don't add to the tree.
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when accessing folder {}.  Error: {}", node, ex.Message);
                throw;
            }

            return Ok(folderList);
        }

        [HttpGet("GetFiles")]
        public ActionResult<IEnumerable<FileInternal>> GetFiles(string path)
        {
            var fileList = new List<FileInternal>();
            try
            {
                var di = new DirectoryInfo(Path.Combine(this._baseFolder, path));
                var fileInfoList = di.EnumerateFiles();
                foreach (var item in fileInfoList)
                {
                    var newFile = new FileInternal
                    {
                        Name = item.Name,
                        Size = item.Length,
                        Path = Path.Combine(path, item.Name)
                    };
                    fileList.Add(newFile);
                }
            }
            catch (Exception)
            {

                throw;
            }

            return Ok(fileList);
        }

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFile(string path)
        {
            var fullPath = Path.Combine(this._baseFolder, path);
            if (System.IO.File.Exists(fullPath)) {
                return File(await System.IO.File.ReadAllBytesAsync(fullPath), "application/octet-stream", Path.GetFileName(fullPath));
            }

            return NotFound();
        }

        [HttpPost("UploadFile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] UploadRequest request)
        {
            var fullPath = Path.Combine(this._baseFolder, request.UploadPath);

            if (Path.Exists(fullPath))
            {
                if (request.File.Length > 0)
                {
                    string filePath = Path.Combine(fullPath, request.File.FileName);
                    using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await request.File.CopyToAsync(fileStream);
                    }
                }
            }
            else
            {
                return NotFound();
            }

            return Accepted();
        }

        [HttpGet("FindFiles")]
        public ActionResult<IEnumerable<FileInternal>> FindFiles(string search)
        {
            return Ok(SearchAccessibleFiles(this._baseFolder, search));
        }


        static IEnumerable<FileInternal> SearchAccessibleFiles(string root, string searchTerm)
        {
            var files = new List<FileInternal>();

            foreach (var file in Directory.EnumerateFiles(root).Where(m => Path.GetFileName(m).IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) > 0))
            {
                var fileInfo = new FileInfo(file);
                var newFile = new FileInternal
                {
                    Name = fileInfo.Name,
                    Size = fileInfo.Length,
                    Path = Path.Combine(root, fileInfo.Name)
                };
                files.Add(newFile);

            }
            foreach (var subDir in Directory.EnumerateDirectories(root))
            {
                try
                {
                    files.AddRange(SearchAccessibleFiles(subDir, searchTerm));
                }
                catch (UnauthorizedAccessException ex)
                {
                    // ...
                }
            }

            return files;
        }
    }
}