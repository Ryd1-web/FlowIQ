"""
FlowIQ AI Service - Anomaly Detection Engine
Uses statistical methods (IQR + Z-score) to detect unusual transactions for Nigerian MSMEs.
"""

import numpy as np
import pandas as pd

from app.models import (
    AnomalyDetectionRequest,
    AnomalyDetectionResponse,
    Anomaly,
    TransactionItem,
)


def _detect_anomalies_in_transactions(
    transactions: list[TransactionItem], txn_type: str
) -> list[Anomaly]:
    """Detect anomalies using IQR method with Z-score severity."""
    if len(transactions) < 5:
        return []

    amounts = np.array([t.amount for t in transactions])

    # IQR-based detection
    q1 = np.percentile(amounts, 25)
    q3 = np.percentile(amounts, 75)
    iqr = q3 - q1

    lower_bound = q1 - 1.5 * iqr
    upper_bound = q3 + 1.5 * iqr

    # Z-score for severity
    mean = np.mean(amounts)
    std = np.std(amounts) if np.std(amounts) > 0 else 1.0

    anomalies = []
    for txn in transactions:
        if txn.amount < lower_bound or txn.amount > upper_bound:
            z_score = abs((txn.amount - mean) / std)

            if z_score > 3:
                severity = "high"
            elif z_score > 2:
                severity = "medium"
            else:
                severity = "low"

            if txn.amount > upper_bound:
                reason = (
                    f"This {txn_type} of ₦{txn.amount:,.0f} is unusually high. "
                    f"Your normal range is ₦{lower_bound:,.0f} - ₦{upper_bound:,.0f}."
                )
            else:
                reason = (
                    f"This {txn_type} of ₦{txn.amount:,.0f} is unusually low. "
                    f"Your normal range is ₦{lower_bound:,.0f} - ₦{upper_bound:,.0f}."
                )

            anomalies.append(
                Anomaly(
                    date=txn.date,
                    type=txn_type,
                    amount=txn.amount,
                    label=txn.label,
                    severity=severity,
                    reason=reason,
                )
            )

    return anomalies


def _generate_anomaly_recommendation(anomalies: list[Anomaly]) -> str:
    """Generate a human-friendly anomaly summary."""
    if not anomalies:
        return (
            "✅ No unusual transactions detected! "
            "Your income and expenses look consistent. Keep up the good work!"
        )

    high_count = sum(1 for a in anomalies if a.severity == "high")
    expense_anomalies = [a for a in anomalies if a.type == "expense"]
    income_anomalies = [a for a in anomalies if a.type == "income"]

    parts = [f"🔍 We found {len(anomalies)} unusual transaction(s)."]

    if high_count > 0:
        parts.append(f"⚠️ {high_count} of them are very unusual and need your attention.")

    if expense_anomalies:
        total_unusual_expense = sum(a.amount for a in expense_anomalies)
        parts.append(
            f"💸 Unusual expenses total ₦{total_unusual_expense:,.0f}. "
            f"Check if these were one-time costs or mistakes."
        )

    if income_anomalies:
        total_unusual_income = sum(a.amount for a in income_anomalies)
        parts.append(
            f"💰 Unusual income totals ₦{total_unusual_income:,.0f}. "
            f"If this was a one-time payment, don't count on it repeating."
        )

    return " ".join(parts)


def detect_anomalies(request: AnomalyDetectionRequest) -> AnomalyDetectionResponse:
    """Main anomaly detection pipeline."""
    income_anomalies = _detect_anomalies_in_transactions(request.incomes, "income")
    expense_anomalies = _detect_anomalies_in_transactions(request.expenses, "expense")

    all_anomalies = income_anomalies + expense_anomalies
    # Sort by severity (high first) then by date
    severity_order = {"high": 0, "medium": 1, "low": 2}
    all_anomalies.sort(key=lambda a: (severity_order.get(a.severity, 3), a.date))

    recommendation = _generate_anomaly_recommendation(all_anomalies)

    return AnomalyDetectionResponse(
        business_id=request.business_id,
        anomalies=all_anomalies,
        total_anomalies=len(all_anomalies),
        recommendation=recommendation,
    )
