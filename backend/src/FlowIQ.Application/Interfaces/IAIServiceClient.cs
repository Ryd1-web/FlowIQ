using FlowIQ.Application.DTOs.AI;

namespace FlowIQ.Application.Interfaces;

public interface IAIServiceClient
{
    Task<CashflowPredictionResponse?> PredictCashflowAsync(CashflowPredictionRequest request);
    Task<AnomalyDetectionResponse?> DetectAnomaliesAsync(AnomalyDetectionRequest request);
    Task<ReceiptCategorizationResponse?> CategorizeReceiptAsync(ReceiptCategorizationRequest request);
}
