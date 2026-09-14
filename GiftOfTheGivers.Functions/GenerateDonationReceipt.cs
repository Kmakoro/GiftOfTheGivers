using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Functions
{
    public class GenerateDonationReceipt
    {
        private readonly ILogger<GenerateDonationReceipt> _logger;

        public GenerateDonationReceipt(
            ILogger<GenerateDonationReceipt> logger)
        {
            _logger = logger;
        }

        [Function("GenerateDonationReceipt")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                "post",
                Route = "donation/receipt")]
            HttpRequestData request)
        {
            _logger.LogInformation(
                "GenerateDonationReceipt function received a {Method} request.",
                request.Method);

            try
            {
                // ---------------------------------------------------------
                // GET REQUEST
                // Used to confirm in the browser that the function is alive.
                // ---------------------------------------------------------
                if (request.Method.Equals(
                    "GET",
                    StringComparison.OrdinalIgnoreCase))
                {
                    HttpResponseData browserResponse =
                        request.CreateResponse(HttpStatusCode.OK);

                    await browserResponse.WriteAsJsonAsync(new
                    {
                        success = true,
                        function = "GenerateDonationReceipt",
                        status = "Running",
                        message =
                            "Gift of the Givers donation function is running. " +
                            "Use a POST request to generate a donation acknowledgement.",
                        supportedCurrencies = new[]
                        {
                            "ZAR",
                            "USD",
                            "EUR"
                        },
                        supportedDonationTypes = new[]
                        {
                            "One-time",
                            "Recurring"
                        }
                    });

                    _logger.LogInformation(
                        "Function status returned successfully through GET.");

                    return browserResponse;
                }

                // ---------------------------------------------------------
                // POST REQUEST
                // Read donation information sent in the JSON body.
                // ---------------------------------------------------------
                string requestBody =
                    await new StreamReader(request.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    _logger.LogWarning(
                        "Donation request was received without a request body.");

                    return await CreateErrorResponse(
                        request,
                        HttpStatusCode.BadRequest,
                        "Donation information is required.");
                }

                DonationRequest? donation =
                    JsonSerializer.Deserialize<DonationRequest>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (donation == null)
                {
                    _logger.LogWarning(
                        "Donation request could not be converted into donation information.");

                    return await CreateErrorResponse(
                        request,
                        HttpStatusCode.BadRequest,
                        "Donation information is required.");
                }

                // ---------------------------------------------------------
                // DONOR VALIDATION
                // Anonymous donations do not require the donor's real name.
                // ---------------------------------------------------------
                if (!donation.IsAnonymous &&
                    string.IsNullOrWhiteSpace(donation.DonorName))
                {
                    _logger.LogWarning(
                        "Donation request failed because donor name was missing.");

                    return await CreateErrorResponse(
                        request,
                        HttpStatusCode.BadRequest,
                        "Donor name is required unless the donation is anonymous.");
                }

                // ---------------------------------------------------------
                // AMOUNT VALIDATION
                // ---------------------------------------------------------
                if (donation.Amount <= 0)
                {
                    _logger.LogWarning(
                        "Donation request failed because amount {Amount} is invalid.",
                        donation.Amount);

                    return await CreateErrorResponse(
                        request,
                        HttpStatusCode.BadRequest,
                        "Donation amount must be greater than zero.");
                }

                // ---------------------------------------------------------
                // CURRENCY VALIDATION
                // ---------------------------------------------------------
                string[] supportedCurrencies =
                {
                    "ZAR",
                    "USD",
                    "EUR"
                };

                string currency =
                    donation.Currency?.Trim().ToUpperInvariant()
                    ?? string.Empty;

                if (!supportedCurrencies.Contains(currency))
                {
                    _logger.LogWarning(
                        "Unsupported donation currency received: {Currency}",
                        currency);

                    return await CreateErrorResponse(
                        request,
                        HttpStatusCode.BadRequest,
                        "Currency must be ZAR, USD or EUR.");
                }

                // ---------------------------------------------------------
                // DONATION TYPE VALIDATION
                // ---------------------------------------------------------
                string donationType =
                    string.IsNullOrWhiteSpace(donation.DonationType)
                        ? "One-time"
                        : donation.DonationType.Trim();

                string[] supportedDonationTypes =
                {
                    "One-time",
                    "Recurring"
                };

                string? validDonationType =
                    supportedDonationTypes.FirstOrDefault(
                        type => type.Equals(
                            donationType,
                            StringComparison.OrdinalIgnoreCase));

                if (validDonationType == null)
                {
                    _logger.LogWarning(
                        "Unsupported donation type received: {DonationType}",
                        donationType);

                    return await CreateErrorResponse(
                        request,
                        HttpStatusCode.BadRequest,
                        "Donation type must be One-time or Recurring.");
                }

                donationType = validDonationType;

                // ---------------------------------------------------------
                // DETERMINE DISPLAY NAME
                // ---------------------------------------------------------
                string donorName =
                    donation.IsAnonymous
                        ? "Anonymous"
                        : donation.DonorName.Trim();

                // ---------------------------------------------------------
                // GENERATE UNIQUE CERTIFICATE / ACKNOWLEDGEMENT NUMBER
                // ---------------------------------------------------------
                string certificateNumber =
                    $"GOTG-{DateTime.UtcNow:yyyyMMdd}-" +
                    $"{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

                // ---------------------------------------------------------
                // CREATE SUCCESSFUL RESPONSE
                // ---------------------------------------------------------
                var result = new
                {
                    success = true,
                    certificateNumber,
                    donorName,
                    amount = donation.Amount,
                    currency,
                    donationType,
                    isAnonymous = donation.IsAnonymous,
                    generatedAt = DateTime.UtcNow,
                    message =
                        "Donation acknowledgement generated successfully."
                };

                _logger.LogInformation(
                    "Donation acknowledgement generated successfully. " +
                    "Certificate: {CertificateNumber}, " +
                    "Donor: {DonorName}, " +
                    "Amount: {Amount}, " +
                    "Currency: {Currency}, " +
                    "Type: {DonationType}, " +
                    "Anonymous: {IsAnonymous}",
                    certificateNumber,
                    donorName,
                    donation.Amount,
                    currency,
                    donationType,
                    donation.IsAnonymous);

                HttpResponseData response =
                    request.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(result);

                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid JSON received by the donation function.");

                return await CreateErrorResponse(
                    request,
                    HttpStatusCode.BadRequest,
                    "The request body contains invalid JSON.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while processing donation request.");

                return await CreateErrorResponse(
                    request,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred while processing the donation.");
            }
        }

        // -------------------------------------------------------------
        // HELPER METHOD FOR ERROR RESPONSES
        // -------------------------------------------------------------
        private static async Task<HttpResponseData> CreateErrorResponse(
            HttpRequestData request,
            HttpStatusCode statusCode,
            string message)
        {
            HttpResponseData response =
                request.CreateResponse(statusCode);

            await response.WriteAsJsonAsync(new
            {
                success = false,
                statusCode = (int)statusCode,
                message
            });

            return response;
        }
    }

    // -----------------------------------------------------------------
    // MODEL USED FOR THE JSON REQUEST
    // -----------------------------------------------------------------
    public class DonationRequest
    {
        public string DonorName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string DonationType { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; }
    }
}