namespace CORR.Communications.Sms;

/// <summary>
/// Options for customizing SMS message delivery behavior.
/// </summary>
public class SmsOptions {

	/// <summary>
	/// The time when the message should be sent. Must be in the future and within provider limits.
	/// If null, the message is sent immediately.
	/// </summary>
	public DateTime? ScheduledSendTime { get; set; }

	/// <summary>
	/// URLs of media files to include with the message (images, videos, documents, etc.).
	/// Creates an MMS message when provided. File size and format restrictions apply based on provider.
	/// </summary>
	public IEnumerable<Uri>? MediaUrls { get; set; }

	/// <summary>
	/// Webhook URL where the provider will send delivery status updates.
	/// Overrides any default status callback configured at the service level.
	/// </summary>
	public Uri? StatusCallbackUrl { get; set; }

	/// <summary>
	/// Maximum time in seconds the message should remain in the provider's queue for delivery.
	/// If the message cannot be delivered within this period, it will be marked as failed.
	/// Useful for time-sensitive messages like 2FA codes.
	/// </summary>
	public TimeSpan? ValidityPeriod { get; set; }

}