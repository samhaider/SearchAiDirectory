namespace SearchAiDirectory.Shared.Models;

public class ToolCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, MaxLength(100)]
    public string Slug { get; set; }

    [Required, MaxLength(200)]
    public string MetaDescription { get; set; }

    [Required, MaxLength(200)]
    public string MetaKeywords { get; set; }

    public DateTime Created { get; set; }


    [InverseProperty(nameof(Tool.Category))]
    public virtual ICollection<Tool> Tools { get; set; }
}