namespace SearchAiDirectory.Shared.Models;

public class UserCode
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
