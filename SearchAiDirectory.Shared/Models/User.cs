namespace SearchAiDirectory.Shared.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    [Required, MaxLength(99)]
    public string Name { get; set; }

    [Required, MaxLength(99)]
    public string Email { get; set; }

    [MaxLength(99)]
    public string SaltCode { get; set; }

    [Required, MaxLength(99)]
    public string Password { get; set; }

    public string Avatar { get; set; }

    [MaxLength(50)]
    public string TimeZone { get; set; }

    public bool IsActive { get; set; }

    public DateTime Registration { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool EmailConfirmed { get; set; }

    public long? ToolID { get; set; }


    [ForeignKey(nameof(ToolID))]
    public virtual Tool Tool { get; set; }

    [InverseProperty(nameof(Like.User))]
    public virtual ICollection<Like> Likes { get; set; }

    [InverseProperty(nameof(Comment.User))]
    public virtual ICollection<Comment> Comments { get; set; }
}