public class ReviewDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOwner { get; set; }

    public string CreatedAtFormatted { get; set; }
}
