namespace CORR.Communications.Sms;

/// <summary>
/// Defines the contract for an SMS service that supports sending text messages 
/// through various delivery methods including direct phone numbers and messaging services.
/// </summary>
public interface ISmsService {
	/// <summary>
	/// Sends a single SMS message from a specific phone number to a recipient.
	/// </summary>
	/// <param name="from">The sender's phone number in E.164 format (e.g., "+15551234567").</param>
	/// <param name="to">The recipient's phone number in E.164 format (e.g., "+15551234567").</param>
	/// <param name="message">The text message content to send. Provider-specific character limits may apply.</param>
	/// <param name="options">Optional settings for message delivery, media attachments, scheduling, etc.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>
	/// A <see cref="MessageResult"/> containing the delivery status, message ID (if successful), 
	/// and error details (if failed).
	/// </returns>
	/// <exception cref="ArgumentException">Thrown when phone numbers are in invalid format.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the SMS service is not properly configured.</exception>
	Task<MessageResult> SendFromAsync(
		string from,
		string to,
		string message,
		SmsOptions? options = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends a single SMS message using a messaging service instead of a specific phone number.
	/// Messaging services typically provide features like load balancing across multiple sender numbers,
	/// automatic fallback, and enhanced delivery rates.
	/// </summary>
	/// <param name="serviceId">The messaging service identifier provided by the SMS provider.</param>
	/// <param name="to">The recipient's phone number in E.164 format (e.g., "+15551234567").</param>
	/// <param name="message">The text message content to send. Provider-specific character limits may apply.</param>
	/// <param name="options">Optional settings for message delivery, media attachments, scheduling, etc.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>
	/// A <see cref="MessageResult"/> containing the delivery status, message ID (if successful), 
	/// and error details (if failed).
	/// </returns>
	/// <exception cref="ArgumentException">Thrown when the phone number is in invalid format or serviceId is invalid.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the SMS service is not properly configured.</exception>
	Task<MessageResult> SendViaServiceAsync(
		string serviceId,
		string to,
		string message,
		SmsOptions? options = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends the same message to multiple recipients efficiently in a single operation.
	/// Supports both validation-only mode for testing and actual message delivery.
	/// </summary>
	/// <param name="message">The text message content to send to all recipients. Provider-specific character limits may apply.</param>
	/// <param name="phoneNumbers">
	/// Collection of recipient phone numbers. Numbers will be parsed and validated according to the specified country code.
	/// Invalid numbers will be reported in the response but won't prevent delivery to valid numbers.
	/// </param>
	/// <param name="from">
	/// Optional sender phone number in E.164 format. Used when not sending via a messaging service.
	/// Ignored if <paramref name="serviceId"/> is provided.
	/// </param>
	/// <param name="serviceId">
	/// Optional messaging service identifier. When provided, takes precedence over the <paramref name="from"/> parameter.
	/// Messaging services typically offer better delivery rates and automatic number selection.
	/// </param>
	/// <param name="countryCode">
	/// ISO 3166-1 alpha-2 country code used for parsing phone numbers that are not in international format.
	/// Default is "US". Examples: "US", "CA", "GB", "AU".
	/// </param>
	/// <param name="validateOnly">
	/// When true, performs phone number validation and parsing without actually sending messages.
	/// Useful for testing phone number formats and configuration without incurring SMS costs.
	/// Default is false.
	/// </param>
	/// <param name="options">Optional settings for message delivery, media attachments, scheduling, etc. Applied to all messages in the batch.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>
	/// A <see cref="MessageResponse"/> containing aggregate statistics (sent/failed counts) 
	/// and individual <see cref="MessageResult"/> details for each phone number attempted.
	/// </returns>
	/// <exception cref="ArgumentException">Thrown when message is empty or no phone numbers are provided.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the SMS service is not properly configured or both from and serviceId are provided.</exception>
	/// <remarks>
	/// <para>
	/// Either <paramref name="from"/> or <paramref name="serviceId"/> must be provided, but not both.
	/// If both are provided, <paramref name="serviceId"/> takes precedence and <paramref name="from"/> is ignored.
	/// </para>
	/// <para>
	/// Phone numbers can be provided in various formats (national, international) and will be normalized
	/// according to the specified <paramref name="countryCode"/>. Invalid numbers will be reported
	/// in the failed results but won't prevent processing of valid numbers.
	/// </para>
	/// <para>
	/// When <paramref name="validateOnly"/> is true, no actual SMS messages are sent, but all
	/// validation, parsing, and provider connectivity checks are performed. This is useful for
	/// testing configurations and phone number formats without incurring costs.
	/// </para>
	/// <para>
	/// When <paramref name="options"/> is provided, the same options (scheduling, media, etc.) 
	/// are applied to all messages in the batch. For different options per recipient, 
	/// use individual <see cref="SendFromAsync"/> or <see cref="SendViaServiceAsync"/> calls.
	/// </para>
	/// </remarks>
	Task<MessageResponse> SendBulkAsync(
		string message,
		IEnumerable<string> phoneNumbers,
		string? from = null,
		string? serviceId = null,
		string countryCode = "US",
		bool validateOnly = false,
		SmsOptions? options = null,
		CancellationToken cancellationToken = default);
}