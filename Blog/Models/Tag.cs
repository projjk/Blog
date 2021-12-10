namespace Blog.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Count { get; set; }
}