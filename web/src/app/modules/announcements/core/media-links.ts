/** A video link, with the address of its player (privacy-friendly variants). */
export interface VideoLink {
  url: string;
  provider: 'YouTube' | 'Vimeo';
  embedUrl: string;
}

/** A piece of an announcement: some HTML text, or a video that replaces a link written alone on its line. */
export type BodyPart = { kind: 'html'; html: string } | { kind: 'video'; video: VideoLink };

const YOUTUBE_ID = /^[\w-]{6,20}$/;
const VIMEO_ID = /^\d{4,15}$/;

/** Reads a YouTube or Vimeo link; null for any other address (or a video that cannot be identified). */
export function parseVideoLink(url: string): VideoLink | null {
  let uri: URL;
  try {
    uri = new URL(url);
  } catch {
    return null;
  }
  if (uri.protocol !== 'https:') {
    return null;
  }
  const host = uri.hostname.toLowerCase().replace(/^(www|m)\./, '');
  const parts = uri.pathname.split('/').filter(Boolean);
  if (host === 'vimeo.com' || host === 'player.vimeo.com') {
    const id = parts.find((part) => VIMEO_ID.test(part));
    return id ? { url, provider: 'Vimeo', embedUrl: `https://player.vimeo.com/video/${id}?dnt=1` } : null;
  }
  if (host === 'youtube.com' || host === 'youtu.be' || host === 'youtube-nocookie.com') {
    const id = host === 'youtu.be' ? parts[0] : uri.searchParams.get('v') ?? (['embed', 'shorts', 'live'].includes(parts[0]) ? parts[1] : undefined);
    return id && YOUTUBE_ID.test(id) ? { url, provider: 'YouTube', embedUrl: `https://www.youtube-nocookie.com/embed/${id}` } : null;
  }
  return null;
}

/**
 * Cuts the (already cleaned) HTML of an announcement into parts: a paragraph that only holds a YouTube or Vimeo link
 * (as a link or as plain text) becomes a video; everything else stays text. A link inside a sentence stays a normal link.
 */
export function splitBody(html: string): BodyPart[] {
  const parts: BodyPart[] = [];
  let text = '';
  const flush = (): void => {
    if (text) {
      parts.push({ kind: 'html', html: text });
      text = '';
    }
  };
  const body = new DOMParser().parseFromString(html, 'text/html').body;
  for (const node of Array.from(body.childNodes)) {
    const video = node instanceof HTMLElement && (node.tagName === 'P' || node.tagName === 'DIV') ? videoOfParagraph(node) : null;
    if (video) {
      flush();
      parts.push({ kind: 'video', video });
    } else {
      text += node instanceof HTMLElement ? node.outerHTML : escapeText(node.textContent ?? '');
    }
  }
  flush();
  return parts;
}

function videoOfParagraph(paragraph: HTMLElement): VideoLink | null {
  const content = paragraph.textContent?.trim() ?? '';
  const links = paragraph.querySelectorAll('a');
  const isOnlyTheLink = links.length === 0 || (links.length === 1 && (links[0].textContent?.trim() === content || links[0].getAttribute('href') === content));
  const url = links.length === 1 ? links[0].getAttribute('href') ?? content : content;
  return isOnlyTheLink && !/\s/.test(content) ? parseVideoLink(url) : null;
}

function escapeText(text: string): string {
  return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

/** "1,2 Mo", "340 Ko"… */
export function formatSize(bytes: number): string {
  if (bytes >= 1024 * 1024) {
    return `${(bytes / (1024 * 1024)).toLocaleString('fr-FR', { maximumFractionDigits: 1 })} Mo`;
  }
  return `${Math.max(1, Math.round(bytes / 1024))} Ko`;
}
