namespace SearchAiDirectory.Models;

public class Tool
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required]
    public long CategoryID { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(100)]
    public string Website { get; set; }

    [MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(100)]
    public string Phone { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public bool IsFree { get; set; }

    [Column(TypeName = "money")]
    public decimal PricePerMonth { get; set; }

    public bool IsConfirmed { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Modified { get; set; }


    [ForeignKey(nameof(CategoryID))]
    public virtual Category Category { get; set; }
}