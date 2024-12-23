namespace SearchAiDirectory.Shared.Models;

public class Like
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    public long ToolID { get; set; }

    public long UserID { get; set; }


    [ForeignKey(nameof(ToolID))]
    public virtual Tool Tool { get; set; }

    [ForeignKey(nameof(UserID))]
    public virtual User User { get; set; }
}
