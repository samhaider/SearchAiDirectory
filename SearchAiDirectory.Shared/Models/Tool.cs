namespace SearchAiDirectory.Shared.Models;

public class Tool
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required]
    public long CategoryID { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(750)]
    public string Description { get; set; }

    [MaxLength(100)]
    public string Website { get; set; }

    [MaxLength(7500)]
    public string WebsiteContent { get; set; }

    [MaxLength(200)]
    public string MetaDescription { get; set; }

    [MaxLength(200)]
    public string MetaKeywords { get; set; }

    [MaxLength(300)]
    public string PriceModel { get; set; }

    public bool IsConfirmed { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Modified { get; set; }


    [ForeignKey(nameof(CategoryID))]
    public virtual ToolCategory Category { get; set; }
}