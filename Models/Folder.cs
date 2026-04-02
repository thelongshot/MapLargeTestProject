using System.Text.Json.Serialization;

namespace TestProject.Models
{
    public class Folder
    {
        public Folder(string name)
        {
            
            Id = name;
            Path = name;
            Name = Path[(Path.LastIndexOf('\\') + 1)..];
            LoadOnDemand = false;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        [JsonPropertyName("load_on_demand")]
        public bool LoadOnDemand { get; set; }
    }
}
