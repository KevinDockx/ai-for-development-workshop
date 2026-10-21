using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ClaimHub.Api.OpenApi;

/// <summary>Describes the API and adds the Bearer scheme so the Scalar UI can send a token.</summary>
internal sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "ClaimHub API",
            Version = "v1",
            Description = "Policy administration, claims and billing for Scheldt Assurance."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Token from POST /api/auth/token."
        };

        return Task.CompletedTask;
    }
}
