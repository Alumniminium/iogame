/**
 * Returns a random color
 * @param random - The random function to be used (defaults to Math.random)
 */
export function randomColor(random = Math.random): number {
  const r = Math.floor(0xff * random());
  const g = Math.floor(0xff * random());
  const b = Math.floor(0xff * random());
  return (r << 16) | (g << 8) | b;
}
