namespace SearchAiDirectory.Shared.Models;

public class NewsEmbedding
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required]
    public long NewsID { get; set; }

    public DateTime CreatedOn { get; set; }

    public string EmbeddingCode { get; set; }


    [ForeignKey(nameof(NewsID))]
    public virtual News News { get; set; }
}