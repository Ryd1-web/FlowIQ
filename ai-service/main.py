"""
FlowIQ AI Service
=================
FastAPI-based AI microservice for Nigerian MSME cashflow prediction and anomaly detection.
Integrates with the FlowIQ .NET backend API.

Endpoints:
  POST /predict/cashflow  - Predict future cashflow based on historical data
  POST /detect/anomaly    - Detect unusual transactions in income/expense data
  GET  /health            - Service health check
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
import logging

from app.models import (
    CashflowPredictionRequest,
    CashflowPredictionResponse,
    AnomalyDetectionRequest,
    AnomalyDetectionResponse,
    HealthResponse,
)
from app.prediction_service import predict_cashflow
from app.anomaly_service import detect_anomalies
from typing import Dict

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
)
logger = logging.getLogger("flowiq-ai")

app = FastAPI(
    title="FlowIQ AI Service",
    description="AI-powered cashflow prediction and anomaly detection for Nigerian MSMEs",
    version="1.0.0",
    docs_url="/docs",
    redoc_url="/redoc",
)

# CORS - allow .NET backend
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5146", "http://localhost:4200"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health", response_model=HealthResponse, tags=["System"])
async def health_check():
    """Service health check endpoint."""
    return HealthResponse(status="healthy", version="1.0.0")


@app.post(
    "/predict/cashflow",
    response_model=CashflowPredictionResponse,
    tags=["Prediction"],
    summary="Predict future cashflow",
    description="Analyzes historical income and expense data to predict future cashflow trends using polynomial regression.",
)
async def cashflow_prediction(request: CashflowPredictionRequest):
    """
    Predict cashflow for a business based on historical transaction data.

    - **business_id**: The business identifier
    - **incomes**: List of historical income transactions
    - **expenses**: List of historical expense transactions
    - **prediction_days**: Number of days to predict (7-90, default 30)
    """
    try:
        logger.info(
            f"Prediction request for business {request.business_id}: "
            f"{len(request.incomes)} incomes, {len(request.expenses)} expenses, "
            f"{request.prediction_days} days"
        )
        result = predict_cashflow(request)
        logger.info(
            f"Prediction complete: status={result.predicted_status}, confidence={result.confidence}"
        )
        return result
    except Exception as e:
        logger.error(f"Prediction failed for business {request.business_id}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Prediction failed: {str(e)}")


@app.post(
    "/detect/anomaly",
    response_model=AnomalyDetectionResponse,
    tags=["Anomaly Detection"],
    summary="Detect unusual transactions",
    description="Uses IQR and Z-score statistical methods to identify unusual income and expense transactions.",
)
async def anomaly_detection(request: AnomalyDetectionRequest):
    """
    Detect anomalous transactions in business data.

    - **business_id**: The business identifier
    - **incomes**: List of income transactions to analyze
    - **expenses**: List of expense transactions to analyze
    """
    try:
        logger.info(
            f"Anomaly detection request for business {request.business_id}: "
            f"{len(request.incomes)} incomes, {len(request.expenses)} expenses"
        )
        result = detect_anomalies(request)
        logger.info(
            f"Anomaly detection complete: {result.total_anomalies} anomalies found"
        )
        return result
    except Exception as e:
        logger.error(f"Anomaly detection failed for business {request.business_id}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Anomaly detection failed: {str(e)}")



@app.post(
    "/categorize/receipt",
    response_model=Dict[str, object],
    tags=["Categorization"],
    summary="Categorize receipt text or image",
)
async def categorize_receipt(payload: Dict[str, object]):
    """Simple receipt categorization endpoint.

    Accepts JSON with `text` (string) or `image_base64` (string). Returns a category and confidence.
    """
    try:
        text = ''
        if 'text' in payload and isinstance(payload['text'], str):
            text = payload['text']
        elif 'image_base64' in payload and isinstance(payload['image_base64'], str):
            # For now, we don't run OCR on images in this lightweight service.
            # In production, run OCR (pytesseract or cloud OCR) here.
            text = ''

        t = (text or '').lower()
        # simple keyword-based categorization
        if any(k in t for k in ['food', 'restaurant', 'meal', 'cafe', 'drink']):
            cat = 'Food'
            conf = 0.95
        elif any(k in t for k in ['rent', 'landlord']):
            cat = 'Rent'
            conf = 0.98
        elif any(k in t for k in ['salary', 'payroll']):
            cat = 'Salary'
            conf = 0.9
        elif any(k in t for k in ['fuel', 'transport', 'uber', 'taxi']):
            cat = 'Transport'
            conf = 0.85
        elif any(k in t for k in ['office', 'stationery', 'supplies', 'supply']):
            cat = 'Supplies'
            conf = 0.85
        else:
            cat = 'Other'
            conf = 0.6

        return { 'category': cat, 'confidence': conf }
    except Exception as e:
        logger.error(f"Receipt categorization failed: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Categorization failed: {str(e)}")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
