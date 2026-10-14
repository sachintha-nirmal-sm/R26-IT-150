from io import BytesIO
from pathlib import Path


def extract_text(file_bytes: bytes, file_name: str | None) -> str:
    """Pull plain text from a PDF or text-like upload."""
    suffix = Path(file_name or "").suffix.lower()
    if suffix == ".pdf" or (file_bytes[:5] == b"%PDF-"):
        return _from_pdf(file_bytes)
    try:
        return file_bytes.decode("utf-8")
    except UnicodeDecodeError:
        return file_bytes.decode("latin-1", errors="ignore")


def _from_pdf(file_bytes: bytes) -> str:
    from pypdf import PdfReader

    reader = PdfReader(BytesIO(file_bytes))
    pages = []
    for page in reader.pages:
        pages.append(page.extract_text() or "")
    return "\n\n".join(pages)
