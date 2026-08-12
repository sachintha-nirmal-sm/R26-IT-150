import traceback
from fastapi import FastAPI, Request
from fastapi.responses import Response, JSONResponse

from app.api.auth import router as auth_router
from app.api.admin_lessons import router as admin_lessons_router
from app.api.quiz_generation import router as quiz_gen_router

app = FastAPI(title="PhysicsLab API")

ALLOWED_ORIGINS = [
    "http://localhost",
    "http://localhost:8080",
    "http://localhost:3000",
    "http://127.0.0.1:8080",
    "http://10.0.2.2:8000",
]

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
    except Exception:
        traceback.print_exc()
        response = JSONResponse(status_code=500, content={"detail": "Internal server error"})
    for key, value in CORS_HEADERS.items():
        response.headers[key] = value
    return response


app.include_router(auth_router)
app.include_router(admin_lessons_router)
app.include_router(quiz_gen_router)


@app.get("/")
def health():
    return {"status": "ok"}
