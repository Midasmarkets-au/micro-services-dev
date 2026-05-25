/**
 * Compact display for `_Payment.Number`.
 *
 * Backend emits payment numbers with a per-vendor prefix:
 *   - default "pm-..."  (most vendors)
 *   - "mdm-..."         (Help2Pay merchant requirement)
 *
 * The tenant tables historically called `.substring(3)` to drop the "pm-" prefix.
 * That hard-coded 3-char strip cut into "mdm-" payments by one character (renders
 * "-2622a52f18b6" instead of "2622a52f18b6"). This helper strips ANY leading
 * "alpha-segment + dash" so future prefixes don't reintroduce the bug.
 *
 * Returns the original string unchanged when it has no matching prefix.
 */
export function formatPaymentNumber(value: string | null | undefined): string {
  if (!value) return "";
  return value.replace(/^[a-z]+-/i, "");
}

export default formatPaymentNumber;
