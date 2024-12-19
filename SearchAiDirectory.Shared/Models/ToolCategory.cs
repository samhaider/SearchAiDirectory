namespace SearchAiDirectory.Shared.Models;

public class ToolCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [InverseProperty(nameof(Tool.Category))]
    public virtual ICollection<Tool> Tools { get; set; }
}