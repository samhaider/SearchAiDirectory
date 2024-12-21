namespace SearchAiDirectory.Shared.Models;

public class ToolEmbedding
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required]
    public long ToolID { get; set; }

    public DateTime CreatedOn { get; set; }

    public string EmbeddingCode { get; set; }


    [ForeignKey(nameof(ToolID))]
    public virtual Tool Tool { get; set; }
}