# Cirreum.Communications.Sms

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Communications.Sms.svg?style=flat-square)](https://www.nuget.org/packages/Cirreum.Communications.Sms/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Communications.Sms.svg?style=flat-square)](https://www.nuget.org/packages/Cirreum.Communications.Sms/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Communications.Sms?style=flat-square)](https://github.com/cirreum/Cirreum.Communications.Sms/releases)

Core abstractions and models for SMS communication services within the Cirreum ecosystem.

## Overview

This package provides the fundamental interfaces and data models for SMS messaging functionality. It defines contracts that SMS provider implementations must follow, enabling consistent SMS operations across different provider backends with support for advanced features like scheduled delivery, MMS attachments, delivery tracking, and message expiration control.

## Installation

```bash
dotnet add package Cirreum.Communications.Sms
```

## Interfaces

### ISmsService

The primary interface for SMS operations supporting both individual and bulk messaging scenarios with advanced delivery options.

```csharp
public interface ISmsService
{
    /// <summary>
    /// Sends a single SMS from a specific phone number
    /// </summary>
    Task<MessageResult> SendFromAsync(
        string from, 
        string to, 
        string message, 
        SmsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a single SMS from a messaging service
    /// </summary>
    Task<MessageResult> SendViaServiceAsync(
        string serviceId, 
        string to, 
        string message, 
        SmsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple messages to different phone numbers
    /// </summary>
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
```

## Data Models

### MessageResult

Represents the result of a single SMS operation.

```csharp
public record MessageResult(
    string PhoneNumber,
    bool Success,
    string? MessageId = null,
    string? ErrorMessage = null);
```

### MessageResponse

Represents the aggregate result of bulk SMS operations.

```csharp
public record MessageResponse(int Sent, int Failed, IReadOnlyList<MessageResult> Results);
```

### SmsOptions

Configures advanced SMS delivery features including scheduling, media attachments, delivery tracking, and message expiration.

```csharp
public class SmsOptions
{
    /// <summary>
    /// Schedule message for future delivery. Must be at least 5 minutes in the future.
    /// </summary>
    public DateTime? ScheduledSendTime { get; set; }

    /// <summary>
    /// URLs of media files to include (creates MMS). Supports images, videos, documents.
    /// </summary>
    public IEnumerable<Uri>? MediaUrls { get; set; }

    /// <summary>
    /// HTTPS webhook URL for delivery status notifications.
    /// </summary>
    public Uri? StatusCallbackUrl { get; set; }

    /// <summary>
    /// Maximum time to attempt delivery. Message fails if not delivered within this period.
    /// </summary>
    public TimeSpan? ValidityPeriod { get; set; }
}
```

## Usage

This package contains only abstractions and models. To send SMS messages, you'll need a concrete implementation package such as:

- `Cirreum.Communications.Sms.Twilio` - Twilio SMS provider implementation

### Dependency Injection

Register your chosen SMS provider in your application startup:

```csharp
// Example with Twilio provider
builder.Services.AddTwilioSms(configuration);
```

### Basic Usage

```csharp
public class NotificationService
{
    private readonly ISmsService _smsService;

    public NotificationService(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task SendWelcomeMessage(string phoneNumber)
    {
        var result = await _smsService.SendViaServiceAsync(
            serviceId: "your-messaging-service-id",
            to: phoneNumber,
            message: "Welcome to our service!");

        if (result.Success)
        {
            Console.WriteLine($"Message sent with ID: {result.MessageId}");
        }
        else
        {
            Console.WriteLine($"Failed to send message: {result.ErrorMessage}");
        }
    }

    public async Task SendBulkNotifications(List<string> phoneNumbers, string message)
    {
        var response = await _smsService.SendBulkAsync(
            message: message,
            phoneNumbers: phoneNumbers,
            serviceId: "your-messaging-service-id");

        Console.WriteLine($"Sent: {response.Sent}, Failed: {response.Failed}");
        
        // Review individual results
        foreach (var result in response.Results.Where(r => !r.Success))
        {
            Console.WriteLine($"Failed to send to {result.PhoneNumber}: {result.ErrorMessage}");
        }
    }
}
```

### Advanced Features

#### Scheduled Messages

Send messages at a specific future time:

```csharp
var options = new SmsOptions
{
    ScheduledSendTime = DateTime.UtcNow.AddHours(2),
    StatusCallbackUrl = new Uri("https://myapp.com/webhooks/sms-status")
};

var result = await _smsService.SendFromAsync(
    from: "+1234567890",
    to: "+0987654321", 
    message: "Your appointment reminder",
    options: options);
```

#### MMS with Media Attachments

Send images, videos, or documents:

```csharp
var options = new SmsOptions
{
    MediaUrls = new[]
    {
        new Uri("https://myapp.com/receipt.pdf"),
        new Uri("https://myapp.com/qr-code.png")
    },
    StatusCallbackUrl = new Uri("https://myapp.com/webhooks/mms-status")
};

var result = await _smsService.SendViaServiceAsync(
    serviceId: "your-service-id",
    to: "+1234567890",
    message: "Your order receipt and QR code",
    options: options);
```

#### Time-Sensitive Messages

Control message expiration for urgent notifications:

```csharp
var options = new SmsOptions
{
    ValidityPeriod = TimeSpan.FromMinutes(5), // 2FA codes
    StatusCallbackUrl = new Uri("https://myapp.com/webhooks/auth-status")
};

var result = await _smsService.SendFromAsync(
    from: "+1234567890",
    to: "+0987654321",
    message: "Your verification code: 123456",
    options: options);
```

#### Delivery Tracking

Monitor message delivery status with webhooks:

```csharp
var options = new SmsOptions
{
    StatusCallbackUrl = new Uri("https://myapp.com/webhooks/delivery-status")
};

// Your webhook endpoint will receive POST requests with delivery updates:
// - "queued" - Message accepted for delivery
// - "sent" - Handed off to carrier
// - "delivered" - Confirmed delivery to recipient
// - "failed" - Delivery failed with error details
```

### Validation Mode

Test phone number parsing and validation without sending messages:

```csharp
var response = await _smsService.SendBulkAsync(
    message: "Test message",
    phoneNumbers: phoneNumbers,
    validateOnly: true);

// Check which phone numbers are valid without sending
var validNumbers = response.Results.Where(r => r.Success).Select(r => r.PhoneNumber);
```

## Features

- **Multiple sending methods**: Send from specific phone numbers or messaging services
- **Scheduled delivery**: Send messages at future dates and times
- **MMS support**: Include images, videos, and documents with messages
- **Delivery tracking**: Real-time status updates via webhooks
- **Message expiration**: Control how long to attempt delivery
- **Bulk operations**: Efficient batch sending with individual result tracking
- **Validation mode**: Test phone number formatting without sending messages
- **Country code support**: Configurable country code for phone number parsing
- **Comprehensive error handling**: Detailed error information for failed operations
- **Provider agnostic**: Works with any SMS provider implementation
- **Cancellation support**: Cancellation tokens for operation control

## Validation and Constraints

### SmsOptions Validation

- **ScheduledSendTime**: Must be at least 5 minutes in the future and within provider limits
- **MediaUrls**: Maximum 10 URLs, must be publicly accessible HTTPS endpoints
- **StatusCallbackUrl**: Must be HTTPS for security
- **ValidityPeriod**: Between 10 seconds and provider maximum (typically 10 hours)

### Phone Number Format

- Phone numbers should be in E.164 format (+1234567890)
- Country codes are used for parsing non-international numbers
- Invalid numbers are reported in results but don't prevent bulk operations

## Provider Implementations

This package provides the abstractions only. Choose from available provider implementations:

- **Twilio**: `Cirreum.Communications.Sms.Twilio`
- Additional providers can be added by implementing the `ISmsService` interface

## Contributing

This package is part of the Cirreum ecosystem. Follow the established patterns when contributing new features or provider implementations.

## Webhook Integration

When using `StatusCallbackUrl`, your webhook endpoint should:

- Accept POST requests with form-encoded data
- Respond with HTTP 200 status code within 10 seconds
- Handle multiple status updates per message (queued → sent → delivered)
- Process delivery failures with error codes and messages

Example webhook payload:
```json
{
  "MessageSid": "SM1234567890abcdef",
  "MessageStatus": "delivered",
  "To": "+1234567890",
  "From": "+0987654321",
  "ErrorCode": null,
  "ErrorMessage": null
}
```






