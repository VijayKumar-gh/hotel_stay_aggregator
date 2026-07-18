# Reflection

## What went well

- The provider abstraction was simple to extend and made the aggregation flow easy to reason about.
- The core search and reservation flows were fast to scaffold and validate with xUnit.
- The React UI was lightweight enough for a quick end-to-end demo.

## What could be improved with more time

- Add a richer domain model for `RoomType` and cancellation policy so the API and UI can share a stricter contract.
- Introduce a true provider-specific pricing and availability policy instead of the current in-memory mock data.
- Improve the frontend into a more complete multi-step experience with a shared state model and stronger error handling.
- Add integration tests against the actual HTTP endpoints so the API contract is verified end-to-end as well as in unit tests.
