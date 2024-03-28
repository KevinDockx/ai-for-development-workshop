using ClaimHub.Api.Features.Claims;
using ClaimHub.Api.Security;
using ClaimHub.Contracts.Claims;
using ClaimHub.Contracts.Common;
using ClaimHub.Domain.Common;
using ClaimHub.Infrastructure.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace ClaimHub.Api.Endpoints;

public static class ClaimsEndpoints
{
    public static IEndpointRouteBuilder MapClaimsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/claims")
            .WithTags("Claims")
            .RequireAuthorization();

        group.MapPost("/", ReportAsync)
            .AddEndpointFilter<ValidationFilter<ReportClaimBody>>();
        group.MapGet("/search", SearchAsync);
        group.MapGet("/", ListAsync)
            .AddEndpointFilter<ValidationFilter<ListClaimsParameters>>();
        group.MapGet("/{id:int}", GetAsync);

        var staff = group.MapGroup("/").RequireAuthorization("ClaimsStaff");
        staff.MapPost("/{id:int}/start-assessment", StartAssessmentAsync);
        staff.MapPost("/{id:int}/assess", AssessAsync)
            .AddEndpointFilter<ValidationFilter<AssessClaimBody>>();
        staff.MapPost("/{id:int}/approve", ApproveAsync)
            .AddEndpointFilter<ValidationFilter<ApproveClaimBody>>();
        staff.MapPost("/{id:int}/reject", RejectAsync)
            .AddEndpointFilter<ValidationFilter<RejectClaimBody>>();
        staff.MapPost("/{id:int}/payout", PayOutAsync);
        group.MapPost("/{id:int}/withdraw", WithdrawAsync)
            .AddEndpointFilter<ValidationFilter<WithdrawClaimBody>>();
        staff.MapPost("/{id:int}/close", CloseAsync);

        return app;
    }

    private static ClaimActor ActorFor(HttpContext http) => new(
        http.User.GetUserId(),
        http.User.GetEmail(),
        http.User.GetRole(),
        http.User.GetPolicyHolderId(),
        http.User.GetBrokerId());

    private static IResult ToHttp<T>(Result<T> result, Func<T, IResult>? success = null)
    {
        if (result.IsSuccess)
        {
            return success != null ? success(result.Value) : Results.Ok(result.Value);
        }

        return result.Code switch
        {
            "claim.not_found" or "policy.not_found" => Results.NotFound(new ApiErrorResponse(result.Error)),
            "claim.invalid_transition" => Results.Conflict(new ApiErrorResponse(result.Error)),
            _ => Results.UnprocessableEntity(new ApiErrorWithCodeResponse(result.Error, result.Code))
        };
    }

    private static async Task<IResult> ReportAsync(
        ReportClaimBody body,
        IMiniMediator mediator,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            new ReportClaimCommand(ActorFor(http), body.PolicyId, body.CoverageType, body.DateOfLoss, body.Description),
            cancellationToken);
        return ToHttp(result, detail => Results.Created($"/api/claims/{detail.Claim.Id}", detail));
    }

    private static async Task<IResult> SearchAsync(
        [FromQuery] string? q,
        IMiniMediator mediator,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest(new ApiErrorResponse("A search term is required."));
        }

        var claims = await mediator.SendAsync(new SearchClaimsQuery(q), cancellationToken);
        return Results.Ok(claims);
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListClaimsParameters parameters,
        IMiniMediator mediator,
        CancellationToken cancellationToken)
    {
        var page = await mediator.SendAsync(
            new ListClaimsQuery(parameters.Page, parameters.PageSize, parameters.Status, parameters.PolicyId),
            cancellationToken);
        return Results.Ok(page);
    }

    private static async Task<IResult> GetAsync(int id, IMiniMediator mediator, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(new GetClaimQuery(id), cancellationToken));

    private static async Task<IResult> StartAssessmentAsync(
        int id, IMiniMediator mediator, HttpContext http, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(new StartAssessmentCommand(ActorFor(http), id), cancellationToken));

    private static async Task<IResult> AssessAsync(
        int id, AssessClaimBody body, IMiniMediator mediator, HttpContext http, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(
            new AssessClaimCommand(ActorFor(http), id, body.AssessedAmount, body.Note), cancellationToken));

    private static async Task<IResult> ApproveAsync(
        int id, ApproveClaimBody body, IMiniMediator mediator, HttpContext http, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(new ApproveClaimCommand(ActorFor(http), id, body.Note), cancellationToken));

    private static async Task<IResult> RejectAsync(
        int id, RejectClaimBody body, IMiniMediator mediator, HttpContext http, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(new RejectClaimCommand(ActorFor(http), id, body.Reason), cancellationToken));

    private static async Task<IResult> PayOutAsync(
        int id, IMiniMediator mediator, HttpContext http, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(new PayOutClaimCommand(ActorFor(http), id), cancellationToken));

    private static async Task<IResult> WithdrawAsync(
        int id, WithdrawClaimBody body, IMiniMediator mediator, HttpContext http, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(new WithdrawClaimCommand(ActorFor(http), id, body.Reason), cancellationToken));

    private static async Task<IResult> CloseAsync(
        int id, IMiniMediator mediator, HttpContext http, CancellationToken cancellationToken) =>
        ToHttp(await mediator.SendAsync(new CloseClaimCommand(ActorFor(http), id), cancellationToken));
}
