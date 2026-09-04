namespace WebApplication2.Models;
public class Problem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
