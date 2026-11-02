using BadPig.Shared.Core;
using BadPig.Orders.Core;
using FluentValidation;

namespace BadPig.Orders.Features.ReceivePaymentNotification;

internal sealed class ReceivePaymentNotificationValidator : AbstractValidator<ReceivePaymentNotificationRequest>
{
    public ReceivePaymentNotificationValidator()
    {
        RuleFor(x => x.Payload).NotEmpty().WithMessage("Notification vide.");
    }
}
