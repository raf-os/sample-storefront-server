namespace SampleStorefront.Models;

public class UserMail
{
  public Guid SenderId { get; set; }
  public Guid RecipientId { get; set; }
  public Guid MailId { get; set; }

  public User Sender { get; set; } = default!;
  public User Recipient { get; set; } = default!;
  public Mail MailObj { get; set; } = default!;
}
