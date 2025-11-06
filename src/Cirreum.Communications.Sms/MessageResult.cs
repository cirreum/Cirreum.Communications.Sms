namespace CORR.Communications.Sms;
/// <summary>
/// Result for individual message
/// </summary>
public record MessageResult(
	string PhoneNumber,
	bool Success,
	string? MessageId = null,
	string? ErrorMessage = null);