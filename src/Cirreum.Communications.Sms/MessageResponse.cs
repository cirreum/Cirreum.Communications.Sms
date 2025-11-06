namespace CORR.Communications.Sms;
/// <summary>
/// Response object for bulk message operations
/// </summary>
public record MessageResponse(int Sent, int Failed, IReadOnlyList<MessageResult> Results);