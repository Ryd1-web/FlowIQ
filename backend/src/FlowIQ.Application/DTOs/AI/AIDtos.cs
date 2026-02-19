namespace FlowIQ.Application.DTOs.AI;

public record CashflowPredictionRequest(
    Guid BusinessId,
    List<DailyDataPoint> HistoricalData,
    int PredictionDays = 30);

public record DailyDataPoint(DateTime Date, decimal Income, decimal Expense);

public record CashflowPredictionResponse(
    List<PredictionPoint> Predictions,
    double ConfidenceScore,
    string Recommendation,
    string PredictedStatus);

public record PredictionPoint(DateTime Date, decimal PredictedIncome, decimal PredictedExpense, decimal PredictedNet);

public record AnomalyDetectionRequest(
    Guid BusinessId,
    List<DailyDataPoint> HistoricalData);

public record AnomalyDetectionResponse(
    List<AnomalyItem> Anomalies,
    double ConfidenceScore,
    int TotalAnomalies,
    string Recommendation);

public record AnomalyItem(DateTime Date, string Type, decimal Amount, string Description, string Severity, string Label);

public record ReceiptCategorizationRequest(string? ReceiptText, string? ImageBase64);

public record ReceiptCategorizationResponse(string Category, double ConfidenceScore);
