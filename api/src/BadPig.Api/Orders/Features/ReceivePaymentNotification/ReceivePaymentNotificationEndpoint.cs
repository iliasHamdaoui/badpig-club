using System.Text;
using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Orders.Features.ReceivePaymentNotification;

[ApiController]
[Tags("Orders")]
[Route(ApiRoutes.V1 + "/shop/payments/notifications")]
[AllowAnonymous]
public sealed class ReceivePaymentNotificationEndpoint(IHandler<ReceivePaymentNotificationRequest> handler) : ControllerBase
{
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The notification is taken into account (or does not concern an order: nothing to do).</response>
    /// <response code="400">Empty notification, or the signature is wrong.</response>
    [HttpPost]
    [EndpointSummary("Receives a notification of the payment provider")]
    [EndpointDescription("Called by the payment provider (not by the site) when a payment page is paid or expires. There is no sign-in: the notification is trusted only if its signature (header Stripe-Signature, computed on the raw body with the secret of the webhook) is right. A paid notification sells the held stock and e-mails the buyer; an expired one releases the stock. Notifications can arrive twice or late: each order is settled once.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceivePaymentNotification(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();
        return (await handler.HandleAsync(new ReceivePaymentNotificationRequest(payload, signature), ct)).ToNoContent();
    }
}
