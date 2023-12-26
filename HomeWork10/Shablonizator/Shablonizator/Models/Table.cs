namespace Shablonizator.Models;

public class Table
{
    public string Group { get; set; } = "11-208";
    public List<StudentForTable> Students { get; set; } = new();
}