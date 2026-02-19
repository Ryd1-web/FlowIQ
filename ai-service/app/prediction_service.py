"""
FlowIQ AI Service - Cashflow Prediction Engine
Uses linear regression for time-series cashflow prediction tailored to Nigerian MSMEs.
"""

import numpy as np
import pandas as pd
from datetime import date, timedelta
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import PolynomialFeatures

from app.models import (
    CashflowPredictionRequest,
    CashflowPredictionResponse,
    DailyPrediction,
    TransactionItem,
)


def _build_daily_series(transactions: list[TransactionItem], start: date, end: date) -> pd.Series:
    """Aggregate transactions into a daily time series."""
    date_range = pd.date_range(start=start, end=end, freq="D")
    daily = pd.Series(0.0, index=date_range)

    for txn in transactions:
        txn_date = pd.Timestamp(txn.date)
        if txn_date in daily.index:
            daily[txn_date] += txn.amount

    return daily


def _fit_and_predict(series: pd.Series, future_days: int) -> tuple[np.ndarray, float]:
    """Fit a polynomial regression model and predict future values."""
    if len(series) < 3:
        mean_val = series.mean() if len(series) > 0 else 0
        return np.full(future_days, mean_val), 0.3

    X = np.arange(len(series)).reshape(-1, 1)
    y = series.values

    # Use degree 2 polynomial for capturing trends without overfitting
    poly = PolynomialFeatures(degree=2, include_bias=False)
    X_poly = poly.fit_transform(X)

    model = LinearRegression()
    model.fit(X_poly, y)

    # R² score as confidence proxy (clamped to 0-1)
    r2 = model.score(X_poly, y)
    confidence = max(0.0, min(1.0, r2))

    # Predict future
    future_X = np.arange(len(series), len(series) + future_days).reshape(-1, 1)
    future_X_poly = poly.transform(future_X)
    predictions = model.predict(future_X_poly)

    # Clamp predictions to non-negative values
    predictions = np.maximum(predictions, 0)

    return predictions, confidence


def _determine_status(total_income: float, total_expense: float) -> str:
    """Determine predicted cashflow health status."""
    if total_income == 0:
        return "Critical"

    net = total_income - total_expense
    ratio = total_expense / total_income if total_income > 0 else 1.0

    if net < 0:
        return "Critical"
    elif ratio > 0.8:
        return "Warning"
    else:
        return "Healthy"


def _generate_recommendation(status: str, avg_net: float, trend_direction: str) -> str:
    """Generate a human-friendly recommendation in simple language for Nigerian MSMEs."""
    recommendations = {
        "Critical": (
            f"⚠️ Your business is spending more than it earns. "
            f"Your predicted daily balance is ₦{avg_net:,.0f}. "
            f"Consider reducing expenses or finding new income sources urgently. "
            f"Check your biggest expenses and see which ones you can cut."
        ),
        "Warning": (
            f"🟡 Your expenses are getting close to your income. "
            f"Your predicted daily balance is ₦{avg_net:,.0f}. "
            f"Try to increase sales or reduce costs before things get tight. "
            f"Build a small emergency fund if you can."
        ),
        "Healthy": (
            f"✅ Your business cashflow looks good! "
            f"Your predicted daily balance is ₦{avg_net:,.0f}. "
            f"Keep it up! Consider saving some profit for slow periods. "
            f"This might be a good time to invest in growing your business."
        ),
    }

    base = recommendations.get(status, recommendations["Warning"])

    if trend_direction == "declining":
        base += " 📉 Note: Your income trend is going down - watch this closely."
    elif trend_direction == "growing":
        base += " 📈 Your income trend is growing - well done!"

    return base


def predict_cashflow(request: CashflowPredictionRequest) -> CashflowPredictionResponse:
    """Main prediction pipeline."""
    # Determine date range from transaction data
    all_dates = [t.date for t in request.incomes + request.expenses]
    if not all_dates:
        # No data - return default response
        today = date.today()
        return CashflowPredictionResponse(
            business_id=request.business_id,
            predictions=[
                DailyPrediction(
                    date=today + timedelta(days=i),
                    predicted_income=0,
                    predicted_expense=0,
                    predicted_net=0,
                )
                for i in range(request.prediction_days)
            ],
            confidence=0.0,
            recommendation="We don't have enough data yet. Keep recording your income and expenses daily!",
            predicted_status="Warning",
        )

    start_date = min(all_dates)
    end_date = max(all_dates)

    # Build daily series
    income_series = _build_daily_series(request.incomes, start_date, end_date)
    expense_series = _build_daily_series(request.expenses, start_date, end_date)

    # Use 7-day rolling averages for smoother predictions
    if len(income_series) >= 7:
        income_smoothed = income_series.rolling(window=7, min_periods=1).mean()
        expense_smoothed = expense_series.rolling(window=7, min_periods=1).mean()
    else:
        income_smoothed = income_series
        expense_smoothed = expense_series

    # Predict
    income_pred, income_conf = _fit_and_predict(income_smoothed, request.prediction_days)
    expense_pred, expense_conf = _fit_and_predict(expense_smoothed, request.prediction_days)

    confidence = (income_conf + expense_conf) / 2

    # Build predictions list
    prediction_start = end_date + timedelta(days=1)
    daily_predictions = []
    for i in range(request.prediction_days):
        pred_date = prediction_start + timedelta(days=i)
        pred_income = round(income_pred[i], 2)
        pred_expense = round(expense_pred[i], 2)
        daily_predictions.append(
            DailyPrediction(
                date=pred_date,
                predicted_income=pred_income,
                predicted_expense=pred_expense,
                predicted_net=round(pred_income - pred_expense, 2),
            )
        )

    # Determine overall predicted status
    total_pred_income = sum(p.predicted_income for p in daily_predictions)
    total_pred_expense = sum(p.predicted_expense for p in daily_predictions)
    status = _determine_status(total_pred_income, total_pred_expense)

    # Determine trend direction
    if len(income_pred) >= 7:
        first_week_avg = np.mean(income_pred[:7])
        last_week_avg = np.mean(income_pred[-7:])
        if last_week_avg < first_week_avg * 0.9:
            trend = "declining"
        elif last_week_avg > first_week_avg * 1.1:
            trend = "growing"
        else:
            trend = "stable"
    else:
        trend = "stable"

    avg_net = np.mean([p.predicted_net for p in daily_predictions])
    recommendation = _generate_recommendation(status, avg_net, trend)

    return CashflowPredictionResponse(
        business_id=request.business_id,
        predictions=daily_predictions,
        confidence=round(confidence, 2),
        recommendation=recommendation,
        predicted_status=status,
    )
