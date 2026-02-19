"""
Tests for FlowIQ AI Service
"""

import pytest
from datetime import date, timedelta
from app.models import (
    CashflowPredictionRequest,
    AnomalyDetectionRequest,
    TransactionItem,
)
from app.prediction_service import predict_cashflow
from app.anomaly_service import detect_anomalies


def _generate_transactions(
    days: int = 30, base_amount: float = 10000, variance: float = 2000, label: str = "Sales"
) -> list[TransactionItem]:
    """Generate test transaction data."""
    import random
    random.seed(42)
    today = date.today()
    return [
        TransactionItem(
            amount=max(0, base_amount + random.uniform(-variance, variance)),
            date=today - timedelta(days=i),
            label=label,
        )
        for i in range(days)
    ]


class TestCashflowPrediction:
    def test_predict_with_sufficient_data(self):
        incomes = _generate_transactions(30, 15000, 3000, "Sales")
        expenses = _generate_transactions(30, 8000, 2000, "Supplies")

        request = CashflowPredictionRequest(
            business_id="test-biz-1",
            incomes=incomes,
            expenses=expenses,
            prediction_days=14,
        )

        result = predict_cashflow(request)

        assert result.business_id == "test-biz-1"
        assert len(result.predictions) == 14
        assert 0 <= result.confidence <= 1
        assert result.predicted_status in ["Healthy", "Warning", "Critical"]
        assert len(result.recommendation) > 0

    def test_predict_with_no_data(self):
        request = CashflowPredictionRequest(
            business_id="test-biz-2",
            incomes=[],
            expenses=[],
            prediction_days=7,
        )

        result = predict_cashflow(request)

        assert result.business_id == "test-biz-2"
        assert len(result.predictions) == 7
        assert result.confidence == 0.0
        assert "don't have enough data" in result.recommendation

    def test_predict_with_minimal_data(self):
        today = date.today()
        incomes = [
            TransactionItem(amount=5000, date=today - timedelta(days=1), label="Sales"),
            TransactionItem(amount=6000, date=today, label="Sales"),
        ]
        expenses = [
            TransactionItem(amount=3000, date=today - timedelta(days=1), label="Rent"),
        ]

        request = CashflowPredictionRequest(
            business_id="test-biz-3",
            incomes=incomes,
            expenses=expenses,
            prediction_days=7,
        )

        result = predict_cashflow(request)

        assert len(result.predictions) == 7
        assert all(p.predicted_income >= 0 for p in result.predictions)
        assert all(p.predicted_expense >= 0 for p in result.predictions)

    def test_critical_status_when_expenses_exceed_income(self):
        incomes = _generate_transactions(30, 5000, 1000, "Sales")
        expenses = _generate_transactions(30, 15000, 2000, "Costs")

        request = CashflowPredictionRequest(
            business_id="test-biz-4",
            incomes=incomes,
            expenses=expenses,
            prediction_days=14,
        )

        result = predict_cashflow(request)

        assert result.predicted_status == "Critical"


class TestAnomalyDetection:
    def test_detect_anomalies_with_outliers(self):
        today = date.today()
        # Normal transactions + 1 obvious outlier
        incomes = [
            TransactionItem(amount=10000, date=today - timedelta(days=i), label="Sales")
            for i in range(10)
        ]
        # Add an outlier
        incomes.append(
            TransactionItem(amount=100000, date=today, label="Big Sale")
        )

        expenses = _generate_transactions(10, 5000, 500, "Supplies")

        request = AnomalyDetectionRequest(
            business_id="test-biz-5",
            incomes=incomes,
            expenses=expenses,
        )

        result = detect_anomalies(request)

        assert result.business_id == "test-biz-5"
        assert result.total_anomalies > 0
        assert any(a.amount == 100000 for a in result.anomalies)

    def test_no_anomalies_with_consistent_data(self):
        today = date.today()
        incomes = [
            TransactionItem(amount=10000, date=today - timedelta(days=i), label="Sales")
            for i in range(20)
        ]
        expenses = [
            TransactionItem(amount=5000, date=today - timedelta(days=i), label="Supplies")
            for i in range(20)
        ]

        request = AnomalyDetectionRequest(
            business_id="test-biz-6",
            incomes=incomes,
            expenses=expenses,
        )

        result = detect_anomalies(request)

        assert result.total_anomalies == 0
        assert "No unusual transactions" in result.recommendation

    def test_insufficient_data_returns_no_anomalies(self):
        today = date.today()
        incomes = [
            TransactionItem(amount=10000, date=today, label="Sales"),
        ]

        request = AnomalyDetectionRequest(
            business_id="test-biz-7",
            incomes=incomes,
            expenses=[],
        )

        result = detect_anomalies(request)

        assert result.total_anomalies == 0

    def test_anomaly_severity_ranking(self):
        today = date.today()
        incomes = [
            TransactionItem(amount=10000, date=today - timedelta(days=i), label="Sales")
            for i in range(15)
        ]
        # Add high and medium anomalies
        incomes.append(
            TransactionItem(amount=500000, date=today, label="Mega Sale")
        )
        incomes.append(
            TransactionItem(amount=50000, date=today - timedelta(days=1), label="Big Sale")
        )

        request = AnomalyDetectionRequest(
            business_id="test-biz-8",
            incomes=incomes,
            expenses=[],
        )

        result = detect_anomalies(request)

        # High severity should come first
        if len(result.anomalies) > 1:
            severity_order = {"high": 0, "medium": 1, "low": 2}
            for i in range(len(result.anomalies) - 1):
                assert severity_order[result.anomalies[i].severity] <= severity_order[result.anomalies[i + 1].severity]
