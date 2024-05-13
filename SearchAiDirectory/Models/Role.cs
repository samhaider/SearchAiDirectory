namespace SearchAiDirectory.Models;

public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short ID { get; set; }

    [Required, MaxLength(99)]
    public string Name { get; set; }
}