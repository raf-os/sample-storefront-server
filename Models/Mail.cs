// using System.Text.Json.Serialization;

namespace SampleStorefront.Models;

public class Mail
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string? Title { get; set; }
  public required string Content { get; set; }
  public DateTimeOffset SendDate { get; set; } = DateTimeOffset.UtcNow;
  public bool IsRead { get; set; } = false;

  public Guid SenderId { get; set; }
  public User Sender { get; set; } = default!;

  public Guid RecipientId { get; set; }
  public User Recipient { get; set; } = default!;

  public UserMail UserMail { get; set; } = default!;

  // public List<Guid>? ExtraRecipients { get; set; }
}

public class MailDTO
{
  public Guid Id { get; set; }
  public string? Title { get; set; }
  public string Content { get; set; } = null!;
  public DateTimeOffset SendDate { get; set; }
  public bool IsRead { get; set; }

  public Guid SenderId { get; set; }
  public UserPublicDTO? Sender { get; set; } = default;

  public Guid RecipientId { get; set; }
  public UserPublicDTO? Recipient { get; set; } = default;

  // public List<Guid>? ExtraRecipients { get; set; }

  public MailDTO() { }
  public MailDTO(Mail mail)
  {
    Id = mail.Id;
    Title = mail.Title;
    Content = mail.Content;
    SendDate = mail.SendDate;
    IsRead = mail.IsRead;

    SenderId = mail.SenderId;
    RecipientId = mail.RecipientId;
    // ExtraRecipients = mail.ExtraRecipients;

    if (mail.Sender != null)
      Sender = new UserPublicDTO(mail.Sender);

    if (mail.Recipient != null)
      Recipient = new UserPublicDTO(mail.Recipient);
  }
}

public class MailPreviewDTO
{
  public Guid Id { get; set; }
  public string? Title { get; set; }
  public DateTimeOffset SendDate { get; set; }
  public Guid SenderId { get; set; }
  public string SenderName { get; set; } = "";
  public string? SenderAvatarUrl { get; set; }

  public MailPreviewDTO() { }
  public MailPreviewDTO(Mail mail)
  {
    Id = mail.Id;
    Title = mail.Title;
    SendDate = mail.SendDate;
    SenderId = mail.SenderId;

    if (mail.Sender != null)
    {
      SenderName = mail.Sender.Name;
      if (mail.Sender.Avatar != null)
        SenderAvatarUrl = mail.Sender.Avatar.Url;
    }
  }
}
