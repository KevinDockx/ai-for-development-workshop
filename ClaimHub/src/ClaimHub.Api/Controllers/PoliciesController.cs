using ClaimHub.Contracts.Common;
using ClaimHub.Contracts.Policies;
using ClaimHub.Api.Policies;
using ClaimHub.Api.Security;
using ClaimHub.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimHub.Api.Controllers;

[ApiController]
[Route("api/Policies")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly PolicyService _policyService;

    public PoliciesController(PolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost("quote")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Quote([FromBody] QuotePolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var quote = await _policyService.QuoteAsync(request, cancellationToken);
            return Ok(quote);
        }
        catch (DomainRuleException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPost("issue")]
    [Authorize(Roles = "Admin,Underwriter,Broker")]
    [ProducesResponseType<PolicyDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Issue([FromBody] IssuePolicyRequest request, CancellationToken cancellationToken)
    {
        var policy = await _policyService.IssueAsync(request, User.GetEmail(), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = policy.Id }, policy);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PolicyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var policy = await _policyService.GetPolicyAsync(id, cancellationToken);
        return policy == null ? NotFound() : Ok(policy);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<PolicyListItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PolicySearchQuery query, CancellationToken cancellationToken)
    {
        var page = await _policyService.SearchPoliciesAsync(query, cancellationToken);
        return Ok(page);
    }

    [HttpGet("expiring")]
    [ProducesResponseType<List<PolicyListItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Expiring([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var policies = await _policyService.GetExpiringAsync(days, cancellationToken);
        return Ok(policies);
    }

    [HttpGet("{id:int}/balance")]
    public async Task<IActionResult> Balance(int id, CancellationToken cancellationToken)
    {
        var policy = await _policyService.GetPolicyAsync(id, cancellationToken);
        if (policy == null)
        {
            return NotFound();
        }

        decimal outstanding = await _policyService.GetOutstandingBalanceAsync(id, cancellationToken);
        return Ok(new PolicyBalanceResponse(id, outstanding));
    }

    [HttpPost("{id:int}/endorse")]
    [Authorize(Roles = "Admin,Underwriter")]
    [ProducesResponseType<EndorsementResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Endorse(int id, [FromBody] EndorsePolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _policyService.EndorseAsync(id, request, User.GetEmail(), cancellationToken);
            return result == null ? NotFound() : Ok(result);
        }
        catch (DomainRuleException ex)
        {
            return UnprocessableEntity(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,Underwriter")]
    [ProducesResponseType<CancellationResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelPolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await _policyService.CancelAsync(id, request, User.GetEmail(), cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:int}/renew")]
    [Authorize(Roles = "Admin,Underwriter")]
    [ProducesResponseType<PolicyDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Renew(int id, [FromBody] RenewPolicyRequest request, CancellationToken cancellationToken)
    {
        var renewed = await _policyService.RenewAsync(id, request, User.GetEmail(), cancellationToken);
        if (renewed == null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(Get), new { id = renewed.Id }, renewed);
    }
}
