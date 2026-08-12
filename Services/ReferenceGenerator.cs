namespace GiftOfTheGivers.Services;

/// <summary>Creates human-readable references for donations and tax certificates.</summary>
public interface IReferenceGenerator
{
    string NewDonationReference();
    string NewCertificateNumber();
}

public class ReferenceGenerator : IReferenceGenerator
{
    public string NewDonationReference() =>
        $"GG-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";

    public string NewCertificateNumber() =>
        $"18A-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}";
}
