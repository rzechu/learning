using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CheckoutService;

/// <summary>
/// Performs HTTP-based calls to the Payments API
/// </summary>
public class PaymentsClient : IPaymentsClient
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IHttpClientFactory _factory;

    /// <summary>
    /// Initialises a new instance of the <see cref="PaymentsClient"/> class.
    /// </summary>
    /// <param name="factory">HTTP client factory</param>
    public PaymentsClient(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Get an payment by ID
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Payment</returns>
    public async Task<PaymentDto> GetPaymentAsync(int paymentId)
    {
        using HttpClient client = _factory.CreateClient("Payments");

        PaymentDto payment = await client.GetFromJsonAsync<PaymentDto>($"/api/payments/{paymentId}", Options);
        return payment;
    }

    /// <summary>
    /// Get an payment by ID
    /// </summary>
    /// <param name="paymentDto">Payment ID</param>
    /// <returns>Payment</returns>
    public async Task<PaymentDto> CreatePaymentAsync(PaymentDto paymentDto)
    {
        using HttpClient client = _factory.CreateClient("Payments");

        var response = await client.PostAsJsonAsync<PaymentDto>($"/api/payments", paymentDto, Options);
        response.EnsureSuccessStatusCode();

        var payment = await JsonSerializer.DeserializeAsync<PaymentDto>(await response.Content.ReadAsStreamAsync(), Options);
        return payment;
    }
}
