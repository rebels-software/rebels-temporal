# Rebels.Temporal — Komendy LLM

Ten dokument opisuje system komend dostępnych podczas pracy z asystentami AI (Claude, ChatGPT, Gemini, itp.).

---

## Dostępne komendy

| Komenda | Opis | Kiedy używać |
|---------|------|--------------|
| `/init` | Inicjalizuje kontekst LLM | Na początku sesji, przed rozpoczęciem pracy |
| `/why`  | Wyjaśnia decyzje projektowe | Gdy chcesz zrozumieć "dlaczego" coś zostało zaprojektowane w określony sposób |

---

## /init

### Opis

Komenda `/init` ładuje pełny kontekst projektu Rebels.Temporal do pamięci LLM. Powinna być użyta na początku każdej nowej sesji pracy z asystentem AI.

### Użycie

Wpisz `/init` w konwersacji z LLM.

### Co robi

Po wykonaniu komendy, LLM:

1. **Załaduje strukturę repozytorium:**
   - `README.md` — przegląd biblioteki i API
   - `/docs` — pełna dokumentacja
   - `/docs/adr` — Architecture Decision Records
   - `/docs/invariants` — nienaruszalne reguły systemu
   - `/src/Rebels.Temporal` — kod źródłowy

2. **Zrozumie model domeny:**
   - Temporal Events (zdarzenia punktowe)
   - Temporal Periods vs Temporal Intervals
   - Time Windows (okna czasowe)
   - Temporal Relations (Allen's Interval Algebra)

3. **Pozna zasady projektowe:**
   - Performance-first design
   - Zero alokacji w hot path
   - Tylko `DateTimeOffset`
   - Brak zewnętrznych zależności

4. **Potwierdzi gotowość:**
   ```
   Rebels.Temporal context loaded and understood. Ready to contribute.
   ```

### Prompt inicjalizacyjny

```text
You are assisting as a contributor to the open-source library Rebels.Temporal.

Load and study the following repository structure, including its documentation and architecture decision records:
- README.md
- /docs (all files)
- /docs/adr (all Architecture Decision Records)
- /docs/invariants (all non-negotiable rules of the system)
- /src/Rebels.Temporal — the source code with domain model and matching engine

Your goals:
1. Understand the temporal domain model used by the library, including:
   - Temporal Events
   - Temporal Periods vs Temporal Intervals
   - Time Windows
   - Temporal Relations
2. Understand the design philosophy, performance principles, and boundaries of the project.
3. Respect all decisions declared in ADRs.
4. Provide answers and code suggestions consistent with the existing architecture.
5. When asked about new features, propose solutions aligned with the project's domain model and design constraints.

After loading all documents, acknowledge with:
"Rebels.Temporal context loaded and understood. Ready to contribute."
```

---

## /why

### Opis

Komenda `/why` służy do wyjaśniania decyzji projektowych w kodzie Rebels.Temporal. Pomaga zrozumieć, dlaczego coś zostało zaimplementowane w określony sposób.

### Użycie

```
/why <pytanie lub kontekst>
```

### Przykłady

```
/why dlaczego używamy DateTimeOffset zamiast DateTime?
/why jaki jest powód user-provided buffers?
/why dlaczego Allen's Interval Algebra?
/why wyjaśnij decyzję o single namespace
```

### Co robi

Komenda `/why`:

1. Przeszukuje ADRs (Architecture Decision Records)
2. Sprawdza invarianty systemu
3. Analizuje kontekst kodu
4. Zwraca wyjaśnienie z odniesieniami do odpowiednich dokumentów

### Powiązane dokumenty

- [ADRs](/docs/adr) — wszystkie decyzje architektoniczne
- [Invariants](/docs/invariants) — nienaruszalne reguły
- [GLOSSARY.md](/docs/GLOSSARY.md) — definicje terminów
- [DECISION-TREE.md](/docs/DECISION-TREE.md) — drzewo decyzji

---

## Dodawanie nowych komend

Aby dodać nową komendę:

1. Dodaj wpis do tabeli "Dostępne komendy" powyżej
2. Utwórz sekcję z opisem komendy zawierającą:
   - Opis
   - Użycie
   - Przykłady
   - Co robi
3. Zaktualizuj README.md jeśli komenda jest kluczowa

### Format sekcji komendy

```markdown
## /nazwa-komendy

### Opis
[Krótki opis co robi komenda]

### Użycie
[Jak wywołać komendę]

### Przykłady
[Konkretne przykłady użycia]

### Co robi
[Szczegółowy opis działania]
```

---

## Zobacz także

- [README.md](/README.md) — główna dokumentacja
- [GLOSSARY.md](GLOSSARY.md) — słownik terminów
- [DECISION-TREE.md](DECISION-TREE.md) — przewodnik wyboru API
