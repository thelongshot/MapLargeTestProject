using System.ComponentModel.DataAnnotations;

namespace TestProject.Models
{
    public class UploadRequest
    {
        [Required]
        public string? UploadPath { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}
