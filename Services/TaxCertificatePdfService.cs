using GiftOfTheGivers.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GiftOfTheGivers.Services;

public interface ITaxCertificatePdfService
{
    byte[] Render(Donation donation, TaxCertificate certificate);
}

/// <summary>
/// Produces the placeholder Section 18A style tax certificate PDF for a donation.
/// Values are symbolic at prototype stage.
/// </summary>
public class TaxCertificatePdfService : ITaxCertificatePdfService
{
    public byte[] Render(Donation donation, TaxCertificate certificate)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(11).FontColor("#22303C"));

                page.Header().Column(col =>
                {
                    col.Item().Text("Gift of the Givers Foundation")
                        .FontSize(20).Bold().FontColor("#0B6B4F");
                    col.Item().Text("Section 18A Donation Tax Certificate (Prototype)")
                        .FontSize(12).FontColor("#6B7B8C");
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#0B6B4F");
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Certificate number: {certificate.CertificateNumber}").Bold();
                    col.Item().Text($"Issued: {certificate.IssuedAt:dd MMMM yyyy}");
                    col.Item().PaddingTop(10).Text("Donor details").Bold().FontSize(13);
                    col.Item().Text($"Name: {(donation.IsAnonymous ? "Anonymous donor" : donation.DonorName)}");
                    if (!donation.IsAnonymous && !string.IsNullOrWhiteSpace(donation.DonorEmail))
                        col.Item().Text($"Email: {donation.DonorEmail}");

                    col.Item().PaddingTop(10).Text("Donation details").Bold().FontSize(13);
                    col.Item().Text($"Reference: {donation.Reference}");
                    col.Item().Text($"Amount: {donation.Currency} {donation.Amount:N2}");
                    col.Item().Text($"Type: {(donation.Frequency == DonationFrequency.Monthly ? "Recurring (monthly)" : "One-time")}");
                    col.Item().Text($"Date received: {donation.DonatedAt:dd MMMM yyyy}");
                    col.Item().Text($"Designated to: {donation.ReliefProject?.Name ?? "Where the need is greatest"}");

                    col.Item().PaddingTop(20).Text(
                        "This certificate confirms a donation received in support of disaster relief operations. " +
                        "It is a prototype placeholder document generated for demonstration purposes and does not " +
                        "constitute a valid receipt for tax purposes.")
                        .Italic().FontColor("#6B7B8C");
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Gift of the Givers Foundation  •  giftofthegivers.org  •  Page ");
                    t.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}
