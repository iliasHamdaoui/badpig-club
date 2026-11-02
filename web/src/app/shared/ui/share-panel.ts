import { Component, computed, input, signal } from '@angular/core';
import qrcode from 'qrcode-generator';

/**
 * "Share" block for any page of the site (an announcement today, an event later): WhatsApp, the phone's own share sheet,
 * copy the link, and a QR code (drawn in the browser, nothing is sent to an outside service) that can be downloaded as a PNG.
 */
@Component({
  selector: 'app-share-panel',
  template: `
    <div class="share">
      <span class="small-text">Partager :</span>
      <a class="btn btn-ghost btn-icon" [href]="whatsappUrl()" target="_blank" rel="noopener noreferrer" aria-label="Partager sur WhatsApp" title="WhatsApp">
        <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M20 12a8 8 0 0 1-11.9 7L4 20l1.1-4A8 8 0 1 1 20 12z"></path><path d="M9 9.5c0 3 2.5 5.5 5.5 5.5.6 0 1-.5 1-1l-1.5-.8-1 .8a4.6 4.6 0 0 1-2.3-2.3l.8-1L10.5 9c-.5 0-1.5 0-1.5.5z"></path></svg>
      </a>
      @if (canNativeShare) {
        <button type="button" class="btn btn-ghost btn-icon" (click)="nativeShare()" aria-label="Partager…" title="Partager…">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 15V3M8 7l4-4 4 4M5 13v6a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2v-6"></path></svg>
        </button>
      }
      <button type="button" class="btn btn-ghost btn-icon" (click)="copy()" [attr.aria-label]="copied() ? 'Lien copié' : 'Copier le lien'" [title]="copied() ? 'Lien copié' : 'Copier le lien'">
        @if (copied()) {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg>
        } @else {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="9" y="9" width="12" height="12" rx="2"></rect><path d="M5 15V5a2 2 0 0 1 2-2h10"></path></svg>
        }
      </button>
      <button type="button" class="btn btn-ghost btn-icon" [attr.aria-expanded]="showQr()" (click)="showQr.set(!showQr())" aria-label="QR code" title="QR code">
        <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="3" width="7" height="7"></rect><rect x="14" y="3" width="7" height="7"></rect><rect x="3" y="14" width="7" height="7"></rect><path d="M14 14h3v3h-3zM19 19h2v2h-2"></path></svg>
      </button>
      @if (copied()) { <span class="small-text" role="status">Lien copié</span> }
      @if (copyFailed()) { <span class="small-text" role="status">Copie impossible : le lien est dans le QR code.</span> }
    </div>

    @if (showQr()) {
      <figure class="card qr stack stack-s">
        <svg [attr.viewBox]="'0 0 ' + qr().size + ' ' + qr().size" width="200" height="200" role="img" [attr.aria-label]="'QR code : ' + title()" shape-rendering="crispEdges">
          <rect width="100%" height="100%" fill="#fff" />
          <path [attr.d]="qr().path" fill="#000" />
        </svg>
        <figcaption class="small-text">{{ note() || "Scannez avec l'appareil photo d'un téléphone pour ouvrir la page." }}</figcaption>
        <p class="form-actions">
          <button type="button" class="btn btn-ghost btn-sm" (click)="downloadQr()">Télécharger le QR code</button>
          <button type="button" class="btn btn-ghost btn-sm" (click)="showQr.set(false)">Fermer</button>
        </p>
        <input class="input" readonly [value]="url()" aria-label="Lien de la page" (focus)="$any($event.target).select()" />
      </figure>
    }
  `,
  styles: `
    :host { display: block; }
    .share { display: flex; flex-wrap: wrap; align-items: center; gap: var(--space-5); }
    .qr { width: fit-content; align-items: center; margin: var(--space-15) 0 0; }
    .qr svg { border-radius: var(--radius-s); }
  `,
})
export class SharePanel {
  /** Address to share (absolute). */
  readonly url = input.required<string>();
  /** Title of the page: the first line of the WhatsApp message and the name of the QR code file. */
  readonly title = input.required<string>();
  /** Optional line shown above the buttons (for instance a reminder that the page is for members). */
  readonly note = input('');

  protected readonly showQr = signal(false);
  protected readonly copied = signal(false);
  protected readonly copyFailed = signal(false);
  protected readonly canNativeShare = typeof navigator !== 'undefined' && typeof navigator.share === 'function';

  protected readonly whatsappUrl = computed(() => `https://wa.me/?text=${encodeURIComponent(`${this.title()}\n${this.url()}`)}`);

  /** The QR code as the matrix of dark squares, drawn as one SVG path (4 squares of quiet zone around it). */
  protected readonly qr = computed(() => {
    const code = qrcode(0, 'M');
    code.addData(this.url());
    code.make();
    const count = code.getModuleCount();
    const margin = 4;
    let path = '';
    for (let row = 0; row < count; row++) {
      for (let col = 0; col < count; col++) {
        if (code.isDark(row, col)) {
          path += `M${col + margin} ${row + margin}h1v1h-1z`;
        }
      }
    }
    return { size: count + 2 * margin, count, margin, path, isDark: (r: number, c: number) => code.isDark(r, c) };
  });

  protected async nativeShare(): Promise<void> {
    try {
      await navigator.share({ title: this.title(), url: this.url() });
    } catch {
      // Cancelled by the person: nothing to do.
    }
  }

  protected async copy(): Promise<void> {
    this.copyFailed.set(false);
    try {
      await navigator.clipboard.writeText(this.url());
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {
      this.copyFailed.set(true);
    }
  }

  /** Draws the QR code on a canvas (10 pixels per square) and saves it as a PNG file. */
  protected downloadQr(): void {
    const { size, count, margin, isDark } = this.qr();
    const scale = 10;
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = size * scale;
    const context = canvas.getContext('2d');
    if (!context) {
      return;
    }
    context.fillStyle = '#fff';
    context.fillRect(0, 0, canvas.width, canvas.height);
    context.fillStyle = '#000';
    for (let row = 0; row < count; row++) {
      for (let col = 0; col < count; col++) {
        if (isDark(row, col)) {
          context.fillRect((col + margin) * scale, (row + margin) * scale, scale, scale);
        }
      }
    }
    canvas.toBlob((blob) => {
      if (!blob) {
        return;
      }
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `qr-${this.fileName(this.title())}.png`;
      link.click();
      URL.revokeObjectURL(link.href);
    }, 'image/png');
  }

  private fileName(title: string): string {
    const name = title.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
    return name.slice(0, 40) || 'page';
  }
}
