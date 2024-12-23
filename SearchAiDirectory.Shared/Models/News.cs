namespace SearchAiDirectory.Shared.Models;

public class News
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [MaxLength(100)]
    public string Title { get; set; }

    [MaxLength(100)]
    public string Slug { get; set; }

    [MaxLength(1000)]
    public string Content { get; set; }

    [MaxLength(100)]
    public string ImageUrl { get; set; }

    [MaxLength(200)]
    public string MetaDescription { get; set; }

    [MaxLength(200)]
    public string MetaKeywords { get; set; }

    [MaxLength(300)]
    public string Website { get; set; }

    [MaxLength(7500)]
    public string WebsiteContent { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Modified { get; set; }


    [InverseProperty(nameof(NewsEmbedding.News))]
    public virtual NewsEmbedding Embedding { get; set; }
}
