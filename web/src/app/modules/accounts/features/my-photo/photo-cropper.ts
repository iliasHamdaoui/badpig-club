import { Component, ElementRef, computed, effect, input, output, signal, viewChild } from '@angular/core';

/** Side (in pixels) of the square picture that is sent to the API. */
const OUTPUT_SIZE = 400;
/** Side of the drawing area; the preview is shown at this size scaled by CSS. */
const VIEW_SIZE = 400;
const MAX_ZOOM = 4;
const KEY_STEP = 12;

/**
 * Lets the person centre and zoom a picture inside a round frame before sending it: drag the picture (or use the arrow keys),
 * move the slider to zoom, « Centrer » puts it back in the middle. The result is a square JPEG (the avatar shows it round).
 * Everything is done in the browser (canvas): the original file never leaves the device.
 */
@Component({
  selector: 'app-photo-cropper',
  template: `
    <div class="frame">
      <canvas #canvas [width]="size" [height]="size" tabindex="0" role="img"
              aria-label="Aperçu de la photo. Faites-la glisser ou utilisez les flèches du clavier pour la centrer."
              (pointerdown)="start($event)" (pointermove)="move($event)" (pointerup)="end($event)" (pointercancel)="end($event)"
              (keydown)="key($event)"></canvas>
    </div>
    <div class="field">
      <label class="label" for="zoom">Zoom</label>
      <input id="zoom" type="range" min="1" [max]="maxZoom" step="0.01" [value]="zoom()" (input)="setZoom($any($event.target).valueAsNumber)" />
    </div>
    <p class="small-text">Faites glisser la photo pour la centrer sur votre visage (ou utilisez les flèches du clavier).</p>
    <p class="form-actions">
      <button type="button" class="btn btn-ghost" (click)="center()">Centrer</button>
      <button type="button" class="btn" [class.is-busy]="busy()" [disabled]="busy()" (click)="confirm()">{{ busy() ? 'Envoi…' : 'Enregistrer la photo' }}</button>
      <button type="button" class="btn btn-ghost" [disabled]="busy()" (click)="cancelled.emit()">Annuler</button>
    </p>
  `,
  styles: `
    :host { display: grid; gap: var(--space-15); justify-items: start; }
    .frame { width: min(100%, 28rem); aspect-ratio: 1; border-radius: 50%; overflow: hidden; border: 0.2rem solid var(--color-border-strong); background: var(--color-surface-subtle); touch-action: none; }
    canvas { width: 100%; height: 100%; display: block; cursor: grab; }
    canvas:active { cursor: grabbing; }
    canvas:focus-visible { outline: var(--focus-ring); outline-offset: calc(-1 * var(--focus-ring-offset)); }
    input[type='range'] { width: min(100%, 28rem); }
  `,
})
export class PhotoCropper {
  /** The picture chosen by the person. */
  readonly file = input.required<File>();
  /** True while the result is being sent. */
  readonly busy = input(false);
  /** The square picture, ready to be sent. */
  readonly confirmed = output<Blob>();
  readonly cancelled = output<void>();
  /** The file could not be read as a picture. */
  readonly unreadable = output<void>();

  protected readonly size = VIEW_SIZE;
  protected readonly maxZoom = MAX_ZOOM;
  protected readonly zoom = signal(1);
  /** Shift of the picture from the centre, in pixels of the drawing area. */
  private readonly offset = signal({ x: 0, y: 0 });
  private readonly image = signal<ImageBitmap | null>(null);
  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private drag: { pointer: number; x: number; y: number } | null = null;

  /** Size of the picture drawn in the area of side `side` at the current zoom (the picture always covers the area). */
  private readonly drawn = computed(() => {
    const image = this.image();
    if (!image) {
      return { width: 0, height: 0 };
    }
    const scale = (Math.max(VIEW_SIZE / image.width, VIEW_SIZE / image.height)) * this.zoom();
    return { width: image.width * scale, height: image.height * scale };
  });

  constructor() {
    effect(() => {
      const file = this.file();
      this.image.set(null);
      this.zoom.set(1);
      this.offset.set({ x: 0, y: 0 });
      createImageBitmap(file).then((bitmap) => this.image.set(bitmap), () => this.unreadable.emit());
    });
    effect(() => {
      this.zoom();
      this.offset();
      const image = this.image();
      if (image) {
        this.paint(this.canvas().nativeElement.getContext('2d')!, image, VIEW_SIZE);
      }
    });
  }

  protected setZoom(value: number): void {
    this.zoom.set(value);
    this.offset.set(this.clamp(this.offset().x, this.offset().y));
  }

  protected center(): void {
    this.zoom.set(1);
    this.offset.set({ x: 0, y: 0 });
  }

  protected start(event: PointerEvent): void {
    this.drag = { pointer: event.pointerId, x: event.clientX, y: event.clientY };
    (event.target as HTMLElement).setPointerCapture(event.pointerId);
  }

  protected move(event: PointerEvent): void {
    if (!this.drag || this.drag.pointer !== event.pointerId) {
      return;
    }
    // The area is shown smaller or bigger than its drawing size: the movement is converted back to drawing pixels.
    const ratio = VIEW_SIZE / (event.target as HTMLElement).getBoundingClientRect().width;
    const dx = (event.clientX - this.drag.x) * ratio;
    const dy = (event.clientY - this.drag.y) * ratio;
    this.drag = { ...this.drag, x: event.clientX, y: event.clientY };
    this.offset.set(this.clamp(this.offset().x + dx, this.offset().y + dy));
  }

  protected end(event: PointerEvent): void {
    if (this.drag?.pointer === event.pointerId) {
      this.drag = null;
    }
  }

  protected key(event: KeyboardEvent): void {
    const moves: Record<string, [number, number]> = { ArrowLeft: [KEY_STEP, 0], ArrowRight: [-KEY_STEP, 0], ArrowUp: [0, KEY_STEP], ArrowDown: [0, -KEY_STEP] };
    const move = moves[event.key];
    if (move) {
      event.preventDefault();
      this.offset.set(this.clamp(this.offset().x + move[0], this.offset().y + move[1]));
    }
  }

  protected confirm(): void {
    const image = this.image();
    if (!image) {
      return;
    }
    const output = document.createElement('canvas');
    output.width = OUTPUT_SIZE;
    output.height = OUTPUT_SIZE;
    this.paint(output.getContext('2d')!, image, OUTPUT_SIZE);
    output.toBlob((blob) => (blob ? this.confirmed.emit(blob) : this.unreadable.emit()), 'image/jpeg', 0.88);
  }

  /** Keeps the picture covering the whole area: it can be moved only as far as its edges allow. */
  private clamp(x: number, y: number): { x: number; y: number } {
    const { width, height } = this.drawn();
    const maxX = Math.max(0, (width - VIEW_SIZE) / 2);
    const maxY = Math.max(0, (height - VIEW_SIZE) / 2);
    return { x: Math.min(maxX, Math.max(-maxX, x)), y: Math.min(maxY, Math.max(-maxY, y)) };
  }

  /** Draws the picture, with the current zoom and shift, in a square area of side `side`. */
  private paint(context: CanvasRenderingContext2D, image: ImageBitmap, side: number): void {
    const factor = side / VIEW_SIZE;
    const { width, height } = this.drawn();
    const { x, y } = this.offset();
    context.fillStyle = '#ffffff';
    context.fillRect(0, 0, side, side);
    context.drawImage(image, (VIEW_SIZE - width) / 2 * factor + x * factor, (VIEW_SIZE - height) / 2 * factor + y * factor, width * factor, height * factor);
  }
}
