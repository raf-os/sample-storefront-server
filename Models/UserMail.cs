// using System.Text.Json.Serialization;

namespace SampleStorefront.Models;

public class UserMail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Title { get; set; }
    public required string Content { get; set; }
    public DateTime SendDate { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    public Guid SenderId { get; set; }
    // public User Sender { get; set; } = default!;

    public Guid RecipientId { get; set; }
    // public User Recipient { get; set; } = default!;

    public List<Guid>? ExtraRecipients { get; set; }
}

public class UserMailDTO
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public required string Content { get; set; }
    public DateTime SendDate { get; set; }
    public bool IsRead { get; set; }

    public Guid SenderId { get; set; }

    public Guid RecipientId { get; set; }

    public List<Guid>? ExtraRecipients { get; set; }

    public UserMailDTO() { }
    public UserMailDTO(UserMail userMail)
    {
        Id = userMail.Id;
        Title = userMail.Title;
        Content = userMail.Content;
        SendDate = userMail.SendDate;
        IsRead = userMail.IsRead;

        SenderId = userMail.SenderId;
        RecipientId = userMail.RecipientId;
        ExtraRecipients = userMail.ExtraRecipients;
    }
}
