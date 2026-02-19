using System.Net.Http.Json;
using System.Text.Json;
using FlowIQ.Application.DTOs.AI;
using FlowIQ.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FlowIQ.Infrastructure.Services;

public class AIServiceClient : IAIServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIServiceClient> _logger;
    private readonly double _confidenceThreshold;

    public AIServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<AIServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _confidenceThreshold = double.Parse(configuration["AIService:ConfidenceThreshold"] ?? "0.6");

        var baseUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:8000";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<CashflowPredictionResponse?> PredictCashflowAsync(CashflowPredictionRequest request)
    {
        try
        {
            // Translate internal DTO to AI service expected JSON schema
            var incomes = request.HistoricalData
                .Where(d => d.Income > 0)
                .Select(d => new { amount = (double)d.Income, date = d.Date.ToString("yyyy-MM-dd"), label = "", category = (string?)null })
                .ToList();

            var expenses = request.HistoricalData
                .Where(d => d.Expense > 0)
                .Select(d => new { amount = (double)d.Expense, date = d.Date.ToString("yyyy-MM-dd"), label = "", category = (string?)null })
                .ToList();

            var payload = new
            {
                business_id = request.BusinessId.ToString(),
                incomes,
                expenses,
                prediction_days = request.PredictionDays
            };

            var response = await _httpClient.PostAsJsonAsync("/predict/cashflow", payload);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("AI prediction service returned {StatusCode}. Body: {Body}", response.StatusCode, body);
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            double confidence = 0.0;
            if (doc.RootElement.TryGetProperty("confidence", out var confEl) && confEl.ValueKind == JsonValueKind.Number)
                confidence = confEl.GetDouble();

            var preds = new List<PredictionPoint>();
            if (doc.RootElement.TryGetProperty("predictions", out var predEl) && predEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in predEl.EnumerateArray())
                {
                    var dateStr = item.GetProperty("date").GetString();
                    DateTime date = DateTime.Parse(dateStr!);
                    var income = item.GetProperty("predicted_income").GetDecimal();
                    var expense = item.GetProperty("predicted_expense").GetDecimal();
                    var net = item.GetProperty("predicted_net").GetDecimal();
                    preds.Add(new PredictionPoint(date, income, expense, net));
                }
            }

            var recommendation = doc.RootElement.TryGetProperty("recommendation", out var recEl)
                ? recEl.GetString() ?? string.Empty
                : string.Empty;
            var predictedStatus = doc.RootElement.TryGetProperty("predicted_status", out var statusEl)
                ? statusEl.GetString() ?? "Warning"
                : "Warning";

            var mapped = new CashflowPredictionResponse(preds, confidence, recommendation, predictedStatus);
            if (mapped != null && mapped.ConfidenceScore < _confidenceThreshold)
            {
                _logger.LogInformation("AI prediction confidence {Score} below threshold {Threshold}", mapped.ConfidenceScore, _confidenceThreshold);
            }

            return mapped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AI prediction service");
            return null; // AI failures must not break core app flow
        }
    }

    public async Task<AnomalyDetectionResponse?> DetectAnomaliesAsync(AnomalyDetectionRequest request)
    {
        try
        {
            var incomes = request.HistoricalData
                .Where(d => d.Income > 0)
                .Select(d => new { amount = (double)d.Income, date = d.Date.ToString("yyyy-MM-dd"), label = "", category = (string?)null })
                .ToList();

            var expenses = request.HistoricalData
                .Where(d => d.Expense > 0)
                .Select(d => new { amount = (double)d.Expense, date = d.Date.ToString("yyyy-MM-dd"), label = "", category = (string?)null })
                .ToList();

            var payload = new
            {
                business_id = request.BusinessId.ToString(),
                incomes,
                expenses
            };

            var response = await _httpClient.PostAsJsonAsync("/detect/anomaly", payload);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("AI anomaly service returned {StatusCode}. Body: {Body}", response.StatusCode, body);
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var anomalies = new List<AnomalyItem>();
            if (doc.RootElement.TryGetProperty("anomalies", out var anomaliesEl) && anomaliesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in anomaliesEl.EnumerateArray())
                {
                    var date = DateTime.Parse(item.GetProperty("date").GetString()!);
                    var type = item.TryGetProperty("type", out var typeEl) ? (typeEl.GetString() ?? "unknown") : "unknown";
                    var amount = item.TryGetProperty("amount", out var amountEl) && amountEl.ValueKind == JsonValueKind.Number
                        ? amountEl.GetDecimal()
                        : 0m;
                    var description = item.TryGetProperty("reason", out var reasonEl)
                        ? (reasonEl.GetString() ?? string.Empty)
                        : string.Empty;
                    var severity = item.TryGetProperty("severity", out var severityEl)
                        ? (severityEl.GetString() ?? "low")
                        : "low";
                    var label = item.TryGetProperty("label", out var labelEl)
                        ? (labelEl.GetString() ?? string.Empty)
                        : string.Empty;

                    anomalies.Add(new AnomalyItem(date, type, amount, description, severity, label));
                }
            }

            var total = doc.RootElement.TryGetProperty("total_anomalies", out var totalEl) && totalEl.ValueKind == JsonValueKind.Number
                ? totalEl.GetInt32()
                : anomalies.Count;
            var recommendation = doc.RootElement.TryGetProperty("recommendation", out var recEl)
                ? recEl.GetString() ?? string.Empty
                : string.Empty;

            // AI anomaly service currently does not emit confidence; infer a simple score.
            var inferredConfidence = anomalies.Count == 0 ? 0.9 : 0.75;
            var result = new AnomalyDetectionResponse(anomalies, inferredConfidence, total, recommendation);

            if (result.ConfidenceScore < _confidenceThreshold)
            {
                _logger.LogInformation("AI anomaly confidence {Score} below threshold {Threshold}",
                    result.ConfidenceScore, _confidenceThreshold);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AI anomaly detection service");
            return null; // AI failures must not break core app flow
        }
    }

    public async Task<ReceiptCategorizationResponse?> CategorizeReceiptAsync(ReceiptCategorizationRequest request)
    {
        try
        {
            var payload = new Dictionary<string, object?>();
            if (!string.IsNullOrEmpty(request.ReceiptText)) payload["text"] = request.ReceiptText;
            if (!string.IsNullOrEmpty(request.ImageBase64)) payload["image_base64"] = request.ImageBase64;
            var response = await _httpClient.PostAsJsonAsync("/categorize/receipt", payload);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("AI categorize service returned {StatusCode}. Body: {Body}", response.StatusCode, body);
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var category = doc.RootElement.TryGetProperty("category", out var categoryEl)
                ? categoryEl.GetString() ?? "Other"
                : "Other";
            var confidence = doc.RootElement.TryGetProperty("confidence", out var confidenceEl) && confidenceEl.ValueKind == JsonValueKind.Number
                ? confidenceEl.GetDouble()
                : 0.0;

            return new ReceiptCategorizationResponse(category, confidence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AI categorize service");
            return null;
        }
    }
}
