namespace Portfolio.Models.APIs;

public class EditorJs
{
    public long time { get; set; } = default!;
    public Block[] blocks { get; set; } = default!;
    public string version { get; set; } = default!;

    public class Block
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public Data? data { get; set; }
    }

    public class Data
    {
        public string? text { get; set; }
        public int? level { get; set; }
        public string? caption { get; set; }
        public string? alignment { get; set; }
        public File? file { get; set; }
        public bool? withBorder { get; set; }
        public bool? withBackground { get; set; }
        public bool? stretched { get; set; }
        public string? code { get; set; }
    }

    public class File
    {
        public string? url { get; set; }
    }
}