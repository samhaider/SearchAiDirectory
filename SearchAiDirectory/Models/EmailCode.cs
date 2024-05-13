namespace SearchAiDirectory.Models;

public class EmailCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required, MaxLength(99)]
    public string Email { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; }

    public DateTime DateCreated { get; set; }
}
