# Bulk SMS

Demonstrates sending SMS to multiple numbers with automatic batching.

## Key Points

- The client auto-splits batches exceeding 200 numbers.
- 0.5s delay between batches to stay under the 2 req/s rate limit.
- ERR013 (queue full) is retried automatically with exponential backoff (30s, 60s, 120s).
- Invalid numbers are filtered locally and reported in `result.Invalid` without blocking valid sends.
- Duplicate numbers (same number in different formats) are deduplicated before sending.
- Use a Promotional Sender ID for marketing/offers. Use Transactional only for OTP/alerts.
