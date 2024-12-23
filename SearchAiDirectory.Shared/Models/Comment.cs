namespace SearchAiDirectory.Shared.Models;

public class Comment
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required]
    public long ToolID { get; set; }

    [Required]
    public long UserID { get; set; }

    [Required, MaxLength(500)]
    public string Content { get; set; }

    public bool Approve { get; set; }

    public DateTime CreatedOn { get; set; }


    [ForeignKey(nameof(ToolID))]
    public virtual Tool Tool { get; set; }

    [ForeignKey(nameof(UserID))]
    public virtual User User { get; set; }
}
