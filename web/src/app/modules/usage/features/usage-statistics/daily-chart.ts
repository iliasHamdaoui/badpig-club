import { Component, computed, input } from '@angular/core';
import { UsageDay } from '../../core/usage-item';

/** The bars of the chart: one per day, or one per week over a long period so that they stay readable. */
interface Bar {
  label: string;
  title: string;
  value: number;
}

const MONTHS = ['janv.', 'févr.', 'mars', 'avr.', 'mai', 'juin', 'juil.', 'août', 'sept.', 'oct.', 'nov.', 'déc.'];
/** Beyond this many days, the days are grouped by week. */
const MAX_DAILY_BARS = 62;

// Geometry of the drawing, in its own units (the SVG scales to the width of the page).
const WIDTH = 600;
const HEIGHT = 240;
const LEFT = 44;
const RIGHT = 8;
const TOP = 12;
const BOTTOM = 30;

/** Pages opened per day, as bars that start at zero. Without a library: a small SVG that takes its colours from the design tokens. */
@Component({
  selector: 'app-daily-chart',
  template: `
    <svg [attr.viewBox]="'0 0 ' + width + ' ' + height" role="img" [attr.aria-label]="summary()" preserveAspectRatio="xMidYMid meet">
      @for (tick of ticks(); track tick.value) {
        <line [attr.x1]="left" [attr.x2]="width - right" [attr.y1]="tick.y" [attr.y2]="tick.y" class="grid" />
        <text [attr.x]="left - 8" [attr.y]="tick.y + 6" text-anchor="end" class="axis">{{ tick.value }}</text>
      }
      @for (bar of bars(); track $index) {
        <rect [attr.x]="bar.x" [attr.y]="bar.y" [attr.width]="bar.width" [attr.height]="bar.height" rx="2" class="bar"><title>{{ bar.title }}</title></rect>
      }
      @for (mark of marks(); track mark.x) {
        <text [attr.x]="mark.x" [attr.y]="height - 8" text-anchor="middle" class="axis">{{ mark.label }}</text>
      }
    </svg>
  `,
  styles: `
    :host { display: block; }
    svg { width: 100%; height: auto; display: block; }
    .grid { stroke: var(--color-border); stroke-width: 1; }
    .axis { fill: var(--color-text-muted); font-size: 16px; }
    .bar { fill: var(--color-brand); }
    .bar:hover { fill: var(--color-link-hover); }
  `,
})
export class DailyChart {
  readonly days = input.required<UsageDay[]>();
  protected readonly width = WIDTH;
  protected readonly height = HEIGHT;
  protected readonly left = LEFT;
  protected readonly right = RIGHT;

  private readonly data = computed<Bar[]>(() => {
    const days = this.days();
    if (days.length <= MAX_DAILY_BARS) {
      return days.map((d) => ({ label: shortDate(d.date), title: `${longDate(d.date)} : ${plural(d.views)}`, value: d.views }));
    }
    const weeks: Bar[] = [];
    for (let i = 0; i < days.length; i += 7) {
      const week = days.slice(i, i + 7);
      const views = week.reduce((sum, d) => sum + d.views, 0);
      weeks.push({ label: shortDate(week[0].date), title: `Semaine du ${longDate(week[0].date)} : ${plural(views)}`, value: views });
    }
    return weeks;
  });

  /** The top of the scale: a round number just above the highest bar (at least 4, so that an empty chart still has a scale). */
  private readonly top = computed(() => niceMax(Math.max(4, ...this.data().map((b) => b.value))));

  protected readonly bars = computed(() => {
    const data = this.data();
    const slot = (WIDTH - LEFT - RIGHT) / data.length;
    const barWidth = Math.max(2, slot * 0.7);
    const plot = HEIGHT - TOP - BOTTOM;
    return data.map((bar, index) => {
      const height = (bar.value / this.top()) * plot;
      return { x: LEFT + index * slot + (slot - barWidth) / 2, y: TOP + plot - height, width: barWidth, height, title: bar.title };
    });
  });

  protected readonly ticks = computed(() => {
    const plot = HEIGHT - TOP - BOTTOM;
    return [0, 1, 2, 3, 4].map((i) => ({ value: (this.top() / 4) * i, y: TOP + plot - (plot / 4) * i }));
  });

  /** About six dates under the bars, evenly spread. */
  protected readonly marks = computed(() => {
    const data = this.data();
    const slot = (WIDTH - LEFT - RIGHT) / data.length;
    const step = Math.max(1, Math.ceil(data.length / 6));
    return data.flatMap((bar, index) => (index % step === 0 ? [{ x: LEFT + index * slot + slot / 2, label: bar.label }] : []));
  });

  protected readonly summary = computed(() => {
    const total = this.data().reduce((sum, b) => sum + b.value, 0);
    return `Pages ouvertes par ${this.days().length > MAX_DAILY_BARS ? 'semaine' : 'jour'} : ${plural(total)} au total.`;
  });
}

function plural(views: number): string {
  return `${views} consultation${views > 1 ? 's' : ''}`;
}

function shortDate(date: string): string {
  const [, month, day] = date.split('-').map(Number);
  return `${day} ${MONTHS[month - 1]}`;
}

function longDate(date: string): string {
  const [year, month, day] = date.split('-').map(Number);
  return `${day} ${MONTHS[month - 1]} ${year}`;
}

/** A multiple of 4 (4, 8, 20, 40, 80…) that gives whole steps on the scale. */
function niceMax(value: number): number {
  const step = value / 4;
  const magnitude = Math.pow(10, Math.floor(Math.log10(step)));
  const rounded = [1, 2, 5, 10].map((m) => m * magnitude).find((candidate) => candidate >= step) ?? 10 * magnitude;
  return rounded * 4;
}
