using Microsoft.AspNetCore.Mvc;

namespace PaymentIntegrationAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentIntegrationAPI.DTOs.Payment;
using PaymentIntegrationAPI.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/payment/esewa")]
public class PaymentController : ControllerBase
{
    private readonly IESewaPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IESewaPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("initiate")]
    [Authorize]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        var result = await _paymentService.InitiatePaymentAsync(request, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("callback/success")]
    public async Task<IActionResult> PaymentSuccess(
        [FromQuery] string oid,
        [FromQuery] string amt,
        [FromQuery] string refId)
    {
        var isVerified = await _paymentService.HandleSuccessCallbackAsync(oid, amt, refId);

        if (isVerified)
        {
            return Redirect($"https://yourfrontend.com/payment/success?ref={refId}");
        }

        return Redirect($"https://yourfrontend.com/payment/failure?reason=verification_failed");
    }

    [HttpGet("callback/failure")]
    public async Task<IActionResult> PaymentFailure(
        [FromQuery] string oid,
        [FromQuery] string? reason)
    {
        await _paymentService.HandleFailureCallbackAsync(oid, reason);
        return Redirect($"https://yourfrontend.com/payment/failure?reason={reason}");
    }

    [HttpGet("status/{transactionUuid}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentStatus(string transactionUuid)
    {
        var result = await _paymentService.GetPaymentStatusAsync(transactionUuid);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("verify")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentRequest request)
    {
        var isVerified = await _paymentService.VerifyPaymentAsync(request.TransactionUuid);

        return Ok(new
        {
            success = isVerified,
            message = isVerified ? "Payment verified successfully" : "Payment verification failed"
        });
    }
}