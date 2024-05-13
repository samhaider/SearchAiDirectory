namespace SearchAiDirectory.Models;

public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short ID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [InverseProperty(nameof(Tool.Category))]
    public virtual ICollection<Tool> Tools { get; set; }
}