export async function parseJsonSafe(response: Response) {
  const text = await response.text();
  if (!text || text.trim().length === 0) return null;
  return JSON.parse(text);
}
