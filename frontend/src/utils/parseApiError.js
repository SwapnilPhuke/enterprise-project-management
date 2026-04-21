/**
 * Extracts a readable error string from an Axios error response.
 * Handles:
 *  - { message: "..." }               — service-level errors
 *  - { errors: { Field: ["msg"] } }   — ASP.NET ModelState / ProblemDetails
 *  - flat { Field: ["msg"] }          — raw ModelState fallback
 */
export function parseApiError(err, fallback = 'Something went wrong. Please try again.') {
  const data = err?.response?.data;
  if (!data) return fallback;

  if (typeof data === 'string') return data;

  if (data.message) return data.message;

  const errorBag = data.errors ?? data;
  if (typeof errorBag === 'object') {
    const messages = Object.values(errorBag)
      .flat()
      .filter(m => typeof m === 'string');
    if (messages.length > 0) return messages.join(' ');
  }

  return fallback;
}
