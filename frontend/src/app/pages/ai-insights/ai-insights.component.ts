import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { forkJoin, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BusinessService } from '../../core/services/business.service';
import { ApiResponse, unwrapApiData } from '../../core/models/api-response.model';

interface AiInsight {
  recommendation: string;
  confidence: number;
  riskLevel: 'Low' | 'Medium' | 'High';
}

interface CashflowPredictionResponseDto {
  confidenceScore?: number;
  ConfidenceScore?: number;
  recommendation?: string;
  Recommendation?: string;
  predictedStatus?: string;
  PredictedStatus?: string;
}

interface AnomalyItemDto {
  severity?: string;
  Severity?: string;
}

interface AnomalyDetectionResponseDto {
  anomalies?: AnomalyItemDto[];
  Anomalies?: AnomalyItemDto[];
  confidenceScore?: number;
  ConfidenceScore?: number;
  totalAnomalies?: number;
  TotalAnomalies?: number;
  recommendation?: string;
  Recommendation?: string;
}

interface ReceiptCategorizationResponseDto {
  category?: string;
  Category?: string;
  confidenceScore?: number;
  ConfidenceScore?: number;
}

@Component({
  selector: 'app-ai-insights',
  templateUrl: './ai-insights.component.html',
  styleUrls: ['./ai-insights.component.scss']
})
export class AiInsightsComponent implements OnInit {
  insights: AiInsight[] = [];
  loading = true;
  error = '';
  businessId = '';

  receiptText = '';
  categorizing = false;
  receiptCategory = '';
  receiptConfidence: number | null = null;
  receiptError = '';

  constructor(
    private http: HttpClient,
    private businessService: BusinessService
  ) {}

  ngOnInit() {
    this.businessService.getAllForCurrentUser().subscribe({
      next: businesses => {
        if (!businesses.length || !businesses[0].id) {
          this.error = 'No business found for this user.';
          this.loading = false;
          return;
        }

        this.businessId = businesses[0].id;
        this.getInsights(this.businessId).subscribe({
          next: insights => {
            this.insights = insights;
            this.loading = false;
          },
          error: (err) => {
            this.error = err?.message || 'Failed to load AI insights.';
            this.loading = false;
          }
        });
      },
      error: () => {
        this.error = 'Failed to load businesses.';
        this.loading = false;
      }
    });
  }

  getInsights(businessId: string): Observable<AiInsight[]> {
    return forkJoin({
      prediction: this.http.get<ApiResponse<CashflowPredictionResponseDto> | CashflowPredictionResponseDto>(
        `${environment.apiBaseUrl}/ai/predict/${businessId}?lookbackDays=60&predictionDays=30`
      ),
      anomaly: this.http.get<ApiResponse<AnomalyDetectionResponseDto> | AnomalyDetectionResponseDto>(
        `${environment.apiBaseUrl}/ai/anomaly/${businessId}?lookbackDays=60`
      )
    }).pipe(
      map(({ prediction, anomaly }) => {
        const predictionData = this.unwrapOrThrow<CashflowPredictionResponseDto>(prediction, 'AI prediction service unavailable.');
        const anomalyData = this.unwrapOrThrow<AnomalyDetectionResponseDto>(anomaly, 'AI anomaly service unavailable.');
        return this.toInsights(predictionData, anomalyData);
      })
    );
  }

  categorizeReceipt(): void {
    if (!this.receiptText.trim()) {
      this.receiptError = 'Please enter receipt text.';
      return;
    }

    this.categorizing = true;
    this.receiptError = '';
    this.receiptCategory = '';
    this.receiptConfidence = null;

    this.http
      .post<ApiResponse<ReceiptCategorizationResponseDto> | ReceiptCategorizationResponseDto>(
        `${environment.apiBaseUrl}/ai/categorize/receipt`,
        { receiptText: this.receiptText }
      )
      .pipe(
        map(res => this.unwrapOrThrow<ReceiptCategorizationResponseDto>(res, 'AI receipt categorization service unavailable.'))
      )
      .subscribe({
        next: res => {
          this.categorizing = false;
          this.receiptCategory = res.category ?? res.Category ?? 'Other';
          this.receiptConfidence = Number(res.confidenceScore ?? res.ConfidenceScore ?? 0);
        },
        error: (err) => {
          this.categorizing = false;
          this.receiptError = err?.message || 'Failed to categorize receipt.';
        }
      });
  }

  getRiskClass(riskLevel: AiInsight['riskLevel']): string {
    if (riskLevel === 'High') {
      return 'risk-high';
    }
    if (riskLevel === 'Medium') {
      return 'risk-medium';
    }
    return 'risk-low';
  }

  private toInsights(
    prediction: CashflowPredictionResponseDto,
    anomaly: AnomalyDetectionResponseDto
  ): AiInsight[] {
    const predictionRecommendation = prediction.recommendation ?? prediction.Recommendation ?? 'No prediction recommendation.';
    const predictionConfidence = Number(prediction.confidenceScore ?? prediction.ConfidenceScore ?? 0);
    const predictedStatus = (prediction.predictedStatus ?? prediction.PredictedStatus ?? 'Warning').toLowerCase();

    const anomalies = anomaly.anomalies ?? anomaly.Anomalies ?? [];
    const totalAnomalies = Number(anomaly.totalAnomalies ?? anomaly.TotalAnomalies ?? anomalies.length);
    const anomalyRecommendation = anomaly.recommendation ?? anomaly.Recommendation ?? 'No anomaly recommendation.';
    const anomalyConfidence = Number(anomaly.confidenceScore ?? anomaly.ConfidenceScore ?? 0.75);

    const predictionRisk: AiInsight['riskLevel'] =
      predictedStatus.includes('healthy') ? 'Low' : predictedStatus.includes('critical') ? 'High' : 'Medium';

    let anomalyRisk: AiInsight['riskLevel'] = 'Low';
    if (totalAnomalies > 2 || anomalies.some(a => (a.severity ?? a.Severity ?? '').toLowerCase() === 'high')) {
      anomalyRisk = 'High';
    } else if (totalAnomalies > 0) {
      anomalyRisk = 'Medium';
    }

    return [
      {
        recommendation: predictionRecommendation,
        confidence: predictionConfidence,
        riskLevel: predictionRisk
      },
      {
        recommendation: anomalyRecommendation,
        confidence: anomalyConfidence,
        riskLevel: anomalyRisk
      }
    ];
  }

  private unwrapOrThrow<T>(response: ApiResponse<T> | T, unavailableMessage: string): T {
    const apiResponse = response as ApiResponse<T>;
    if (apiResponse && typeof apiResponse === 'object' && 'success' in apiResponse) {
      if (!apiResponse.success || apiResponse.data === null || apiResponse.data === undefined) {
        throw new Error(apiResponse.message || unavailableMessage);
      }
      return apiResponse.data;
    }

    return unwrapApiData<T>(response, {} as T);
  }
}
