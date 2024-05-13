namespace SearchAiDirectory.Models;

public class Log
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }


    [MaxLength(100)]
    public string User { get; set; }

    [MaxLength(100)]
    public string UserIp { get; set; }

    [MaxLength(100)]
    public string Host { get; set; }

    [MaxLength(300)]
    public string Url { get; set; }

    public int Code { get; set; }

    [MaxLength(500)]
    public string Message { get; set; }

    [MaxLength(1000)]
    public string StackTrace { get; set; }

    public DateTime Created { get; set; }
}
