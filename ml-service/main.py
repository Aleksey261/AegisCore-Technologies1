from fastapi import FastAPI
from pydantic import BaseModel
from typing import Any, Dict

app = FastAPI(title="AegisCore ML Service")

class Payload(BaseModel):
    data: Dict[str, Any] | None = None

@app.get("/health")
def health():
    return {"status": "ok"}

@app.post("/api/v1/predict")
def predict(payload: Payload):
    return {"risk_score": 0.42, "label": "medium", "source": "stub-model"}

@app.post("/api/v1/analyze")
def analyze(payload: Dict[str, Any]):
    return {"status": "ok", "summary": "ML service stub processed payload", "items": len(str(payload))}
