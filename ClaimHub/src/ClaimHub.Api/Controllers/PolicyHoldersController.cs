using ClaimHub.Contracts.Common;
using ClaimHub.Contracts.Policies;
using ClaimHub.Contracts.PolicyHolders;
using ClaimHub.Api.Policies;
using ClaimHub.Api.Security;
using ClaimHub.Domain.Common;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimHub.Api.Controllers;

[ApiController]
[Route("api/PolicyHolders")]
[Authorize]
public class PolicyHoldersController : ControllerBase
{
    private readonly PolicyService _policyService;

    public PolicyHoldersController(PolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<PolicyHolderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PolicyHolderQuery query, CancellationToken cancellationToken)
    {
        if (User.GetRole() == UserRole.PolicyHolder)
        {
            return Forbid();
        }

        var holders = await _policyService.ListHoldersAsync(query, cancellationToken);
        return Ok(holders);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PolicyHolderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var holder = await _policyService.GetHolderAsync(id, cancellationToken);
        if (holder == null)
        {
            return NotFound();
        }

        return Ok(holder);
    }

    [HttpPost]
    [ProducesResponseType<PolicyHolderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePolicyHolderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _policyService.CreateHolderAsync(request, User.GetEmail(), cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (DomainRuleException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<PolicyHolderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PolicyHolder holder, CancellationToken cancellationToken)
    {
        var updated = await _policyService.UpdateHolderAsync(id, holder, User.GetEmail(), cancellationToken);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpGet("{id:int}/policies")]
    [ProducesResponseType<List<PolicyListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPolicies(int id, CancellationToken cancellationToken)
    {
        var policies = await _policyService.GetPoliciesForHolderAsync(id, cancellationToken);
        if (policies == null)
        {
            return NotFound();
        }

        return Ok(policies);
    }
}
