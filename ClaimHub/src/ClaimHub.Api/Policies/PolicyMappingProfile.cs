using AutoMapper;
using ClaimHub.Contracts.Policies;
using ClaimHub.Contracts.PolicyHolders;
using ClaimHub.Domain.Entities;
using ClaimHub.Domain.Enums;
using ClaimHub.Domain.Models;

namespace ClaimHub.Api.Policies;

public class PolicyMappingProfile : Profile
{
    public PolicyMappingProfile()
    {
        CreateMap<Coverage, CoverageDto>();
        CreateMap<PolicyEndorsement, EndorsementDto>();

        CreateMap<Policy, PolicyListItemDto>()
            .ForMember(d => d.PolicyHolderName, o => o.MapFrom(s => s.PolicyHolder != null ? s.PolicyHolder.FullName : null));

        CreateMap<Policy, PolicyDto>()
            .IncludeBase<Policy, PolicyListItemDto>()
            .ForMember(d => d.BrokerName, o => o.MapFrom(s => s.Broker != null ? s.Broker.Name : null))
            .ForMember(d => d.RiskSummary, o => o.MapFrom(s => DescribeRisk(s)))
            .ForMember(d => d.Coverages, o => o.MapFrom(s => s.Coverages.Where(c => c.IsActive)))
            .ForMember(d => d.Endorsements, o => o.MapFrom(s => s.Endorsements.OrderBy(e => e.EffectiveDate)));

        CreateMap<Installment, InstallmentDto>();
        CreateMap<DiscountLine, DiscountLineDto>();
        CreateMap<RatingFactor, RatingFactorDto>();
        CreateMap<PremiumQuote, QuoteResponse>();

        CreateMap<EndorsementPremiumResult, EndorsementResultDto>()
            .ForMember(d => d.PolicyId, o => o.Ignore())
            .ForMember(d => d.EndorsementId, o => o.Ignore());

        CreateMap<CancellationRefundResult, CancellationResultDto>()
            .ForMember(d => d.PolicyId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreditNoteNumber, o => o.Ignore());

        CreateMap<PolicyHolder, PolicyHolderSummaryDto>();
        CreateMap<PolicyHolder, PolicyHolderDto>()
            .ForMember(d => d.PolicyCount, o => o.MapFrom(s => s.Policies.Count(p => !p.IsDeleted)));

        CreateMap<CreatePolicyHolderRequest, PolicyHolder>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CustomerSince, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.IsVip, o => o.Ignore())
            .ForMember(d => d.CreditLimit, o => o.Ignore())
            .ForMember(d => d.Broker, o => o.Ignore())
            .ForMember(d => d.Policies, o => o.Ignore())
            .ForMember(d => d.InsuredPersons, o => o.Ignore());
    }

    private static string DescribeRisk(Policy policy) => policy.ProductType switch
    {
        ProductType.Home when policy.HomeRisk is { } home =>
            $"{home.Street}, {home.PostalCode} {home.City} (zone {home.PostcodeZone}, built {home.ConstructionYear})",
        ProductType.Motor when policy.MotorRisk is { } motor =>
            $"{motor.Make} {motor.Model}, {motor.PowerKw} kW, plate {motor.LicensePlate}",
        ProductType.Travel when policy.TravelRisk is { } travel =>
            $"{travel.PartyType} travel cover, {travel.Scope}",
        _ => "No risk details"
    };
}
