from pydantic import BaseModel, Field
from typing import Optional
from datetime import date
from enum import Enum


class ExpenseCategory(str, Enum):
    RENT = "Rent"
    UTILITIES = "Utilities"
    SALARIES = "Salaries"
    INVENTORY = "Inventory"
    TRANSPORT = "Transport"
    MARKETING = "Marketing"
    EQUIPMENT = "Equipment"
    MAINTENANCE = "Maintenance"
    INSURANCE = "Insurance"
    TAXES = "Taxes"
    COMMUNICATION = "Communication"
    SUPPLIES = "Supplies"
    OTHER = "Other"


class TransactionItem(BaseModel):
    amount: float
    date: date
    label: str
    category: Optional[str] = None


class CashflowPredictionRequest(BaseModel):
    business_id: str
    incomes: list[TransactionItem]
    expenses: list[TransactionItem]
    prediction_days: int = Field(default=30, ge=7, le=90)


class DailyPrediction(BaseModel):
    date: date
    predicted_income: float
    predicted_expense: float
    predicted_net: float


class CashflowPredictionResponse(BaseModel):
    business_id: str
    predictions: list[DailyPrediction]
    confidence: float = Field(ge=0, le=1)
    recommendation: str
    predicted_status: str  # Healthy, Warning, Critical


class AnomalyDetectionRequest(BaseModel):
    business_id: str
    incomes: list[TransactionItem]
    expenses: list[TransactionItem]


class Anomaly(BaseModel):
    date: date
    type: str  # "income" or "expense"
    amount: float
    label: str
    severity: str  # "low", "medium", "high"
    reason: str


class AnomalyDetectionResponse(BaseModel):
    business_id: str
    anomalies: list[Anomaly]
    total_anomalies: int
    recommendation: str


class HealthResponse(BaseModel):
    status: str
    version: str
