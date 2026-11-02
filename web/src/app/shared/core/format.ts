import { HttpErrorResponse } from '@angular/common/http';

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fr-FR');
}

/** A day written for the people: "samedi 14 novembre 2026" (the API gives dates as yyyy-MM-dd). */
export function formatLongDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
}

export function formatEuro(amount: number): string {
  return new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' }).format(amount);
}

/** An amount in euro cents (what the API gives for the prices of the shop), written for the people: "20,00 €". */
export function formatCents(cents: number): string {
  return formatEuro(cents / 100);
}

/** Uses the API's message when it gives one (409, 502, or the first validation error of a 400), otherwise the general message. */
export function apiErrorMessage(e: unknown, fallback: string): string {
  if (e instanceof HttpErrorResponse && (e.status === 409 || e.status === 502)) {
    return (e.error?.title as string | undefined) ?? fallback;
  }
  if (e instanceof HttpErrorResponse && e.status === 400) {
    // Validation errors: { errors: { field: [message, ...] } }, the messages are written for the person.
    const first = Object.values((e.error?.errors ?? {}) as Record<string, string[]>)[0]?.[0];
    return first ?? fallback;
  }
  return fallback;
}
