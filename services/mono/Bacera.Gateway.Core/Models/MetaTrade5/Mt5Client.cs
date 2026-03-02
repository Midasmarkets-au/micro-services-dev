using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Client
{
    public ulong ClientId { get; set; }

    public long Timestamp { get; set; }

    public uint ClientType { get; set; }

    public uint ClientStatus { get; set; }

    public string ClientExternalId { get; set; } = null!;

    public ulong AssignedManager { get; set; }

    public string Comment { get; set; } = null!;

    public ulong ComplianceApprovedBy { get; set; }

    public string ComplianceClientCategory { get; set; } = null!;

    public DateTime ComplianceDateApproval { get; set; }

    public DateTime ComplianceDateTermination { get; set; }

    public string LeadCampaign { get; set; } = null!;

    public string LeadSource { get; set; } = null!;

    public string Introducer { get; set; } = null!;

    public string PersonTitle { get; set; } = null!;

    public string PersonName { get; set; } = null!;

    public string PersonMiddleName { get; set; } = null!;

    public string PersonLastName { get; set; } = null!;

    public DateTime PersonBirthDate { get; set; }

    public string PersonCitizenship { get; set; } = null!;

    public uint PersonGender { get; set; }

    public string PersonTaxId { get; set; } = null!;

    public string PersonDocumentType { get; set; } = null!;

    public string PersonDocumentNumber { get; set; } = null!;

    public DateTime PersonDocumentDate { get; set; }

    public string PersonDocumentExtra { get; set; } = null!;

    public uint PersonEmployment { get; set; }

    public uint PersonIndustry { get; set; }

    public uint PersonEducation { get; set; }

    public uint PersonWealthSource { get; set; }

    public double PersonAnnualIncome { get; set; }

    public double PersonNetWorth { get; set; }

    public double PersonAnnualDeposit { get; set; }

    public string CompanyName { get; set; } = null!;

    public string CompanyRegNumber { get; set; } = null!;

    public DateTime CompanyRegDate { get; set; }

    public string CompanyRegAuthority { get; set; } = null!;

    public string CompanyVat { get; set; } = null!;

    public string CompanyLei { get; set; } = null!;

    public string CompanyLicenseNumber { get; set; } = null!;

    public string CompanyLicenseAuthority { get; set; } = null!;

    public string CompanyCountry { get; set; } = null!;

    public string CompanyAddress { get; set; } = null!;

    public string CompanyWebsite { get; set; } = null!;

    public uint ContactPreferred { get; set; }

    public string ContactLanguage { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string ContactPhone { get; set; } = null!;

    public string ContactMessengers { get; set; } = null!;

    public string ContactSocialNetworks { get; set; } = null!;

    public DateTime ContactLastDate { get; set; }

    public string AddressCountry { get; set; } = null!;

    public string AddressPostcode { get; set; } = null!;

    public string AddressStreet { get; set; } = null!;

    public string AddressState { get; set; } = null!;

    public string AddressCity { get; set; } = null!;

    public uint ExperienceFx { get; set; }

    public uint ExperienceCfd { get; set; }

    public uint ExperienceFutures { get; set; }

    public uint ExperienceStocks { get; set; }

    public uint ClientOrigin { get; set; }

    public ulong ClientOriginLogin { get; set; }
}
