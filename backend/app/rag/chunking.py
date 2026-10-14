from app.core.config import RAG_CHUNK_OVERLAP, RAG_CHUNK_SIZE


def chunk_text(
    text: str,
    chunk_size: int = RAG_CHUNK_SIZE,
    overlap: int = RAG_CHUNK_OVERLAP,
) -> list[str]:
    cleaned = " ".join((text or "").split())
    if not cleaned:
        return []
    if len(cleaned) <= chunk_size:
        return [cleaned]

    chunks: list[str] = []
    start = 0
    while start < len(cleaned):
        end = min(len(cleaned), start + chunk_size)
        piece = cleaned[start:end]
        # Prefer breaking on a sentence boundary near the end of the window.
        if end < len(cleaned):
            cut = max(piece.rfind(". "), piece.rfind("? "), piece.rfind("! "))
            if cut > chunk_size // 3:
                piece = piece[: cut + 1]
                end = start + cut + 1
        chunks.append(piece.strip())
        if end >= len(cleaned):
            break
        start = max(0, end - overlap)
    return [c for c in chunks if c]
