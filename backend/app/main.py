import sys
import io
import traceback
from fastapi import FastAPI, Request
from fastapi.responses import Response, JSONResponse

# Force UTF-8 on Windows so binary data in tracebacks never crashes the process
if hasattr(sys.stdout, 'buffer'):
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
if hasattr(sys.stderr, 'buffer'):
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

from app.api.auth import router as auth_router
from app.api.admin_lessons import router as admin_lessons_router
from app.api.generate_questions import router as generate_router
from app.api.ml_analytics import router as ml_analytics_router
from app.api.recommendations import router as recommendations_router

app = FastAPI(title="PhysicsLab API")

CORS_HEADERS = {
    "Access-Control-Allow-Origin": "*",
    "Access-Control-Allow-Methods": "GET, POST, PUT, PATCH, DELETE, OPTIONS",
    "Access-Control-Allow-Headers": "Authorization, Content-Type, Accept",
}


@app.middleware("http")
async def cors_middleware(request: Request, call_next):
    if request.method == "OPTIONS":
        return Response(status_code=204, headers=CORS_HEADERS)
    try:
        response = await call_next(request)
    except Exception as exc:
        try:
            traceback.print_exc()
        except Exception:
            print(f"[error] {type(exc).__name__}: {repr(exc)[:200]}", file=sys.__stderr__)
        response = JSONResponse(status_code=500, content={"detail": "Internal server error"})
    for key, value in CORS_HEADERS.items():
        response.headers[key] = value
    return response


app.include_router(auth_router)
app.include_router(admin_lessons_router)
app.include_router(generate_router)
app.include_router(ml_analytics_router)
app.include_router(recommendations_router)


@app.get("/")
def health():
    return {"status": "ok"}
