import { Component, ElementRef, afterNextRender, effect, model, viewChild } from '@angular/core';

/**
 * Small rich text editor (bold, italic, titles, lists, quote, link, emojis) built on a `contenteditable` element, with no library.
 * It produces HTML; the API cleans it again (only safe tags are kept), so this is a writing aid, not a protection.
 */
@Component({
  selector: 'app-rich-text-editor',
  template: `
    <div class="toolbar" role="toolbar" aria-label="Mise en forme">
      <button type="button" class="btn btn-ghost btn-sm" title="Gras" aria-label="Gras" (mousedown)="$event.preventDefault()" (click)="run('bold')"><b>G</b></button>
      <button type="button" class="btn btn-ghost btn-sm" title="Italique" aria-label="Italique" (mousedown)="$event.preventDefault()" (click)="run('italic')"><i>I</i></button>
      <button type="button" class="btn btn-ghost btn-sm" title="Souligné" aria-label="Souligné" (mousedown)="$event.preventDefault()" (click)="run('underline')"><u>S</u></button>
      <span class="sep"></span>
      <button type="button" class="btn btn-ghost btn-sm" title="Titre" (mousedown)="$event.preventDefault()" (click)="block('h2')">Titre</button>
      <button type="button" class="btn btn-ghost btn-sm" title="Sous-titre" (mousedown)="$event.preventDefault()" (click)="block('h3')">Sous-titre</button>
      <button type="button" class="btn btn-ghost btn-sm" title="Paragraphe" (mousedown)="$event.preventDefault()" (click)="block('p')">Texte</button>
      <span class="sep"></span>
      <button type="button" class="btn btn-ghost btn-sm" title="Liste à puces" (mousedown)="$event.preventDefault()" (click)="run('insertUnorderedList')">• Liste</button>
      <button type="button" class="btn btn-ghost btn-sm" title="Liste numérotée" (mousedown)="$event.preventDefault()" (click)="run('insertOrderedList')">1. Liste</button>
      <button type="button" class="btn btn-ghost btn-sm" title="Citation" (mousedown)="$event.preventDefault()" (click)="block('blockquote')">« Citation »</button>
      <span class="sep"></span>
      <button type="button" class="btn btn-ghost btn-sm" title="Ajouter un lien" (mousedown)="$event.preventDefault()" (click)="link()">Lien</button>
      <button type="button" class="btn btn-ghost btn-sm" title="Enlever le lien" (mousedown)="$event.preventDefault()" (click)="run('unlink')">Sans lien</button>
      <button type="button" class="btn btn-ghost btn-sm" title="Enlever la mise en forme" (mousedown)="$event.preventDefault()" (click)="run('removeFormat')">Effacer</button>
      <span class="sep"></span>
      @for (e of emojis; track e) {
        <button type="button" class="secondary small emoji" [attr.aria-label]="'Insérer ' + e" (mousedown)="$event.preventDefault()" (click)="insert(e)">{{ e }}</button>
      }
    </div>
    <div #editor class="editor" contenteditable="true" role="textbox" aria-multiline="true" aria-label="Contenu de l'annonce"
         data-placeholder="Écrivez votre annonce ici…" (input)="changed()" (blur)="changed()" (paste)="pastePlainText($event)"></div>
  `,
  styles: `
    .toolbar { display: flex; flex-wrap: wrap; gap: 0.48rem; align-items: center; padding: 0.64rem; border: 0.1rem solid var(--input-border); border-bottom: 0; border-radius: var(--radius-m) 8px 0 0; background: #faf6f5; }
    .sep { width: 1px; align-self: stretch; background: #d9cdcb; margin: 0 0.32rem; }
    .emoji { padding: 0.24rem 0.64rem; }
    .editor { min-height: 19.2rem; max-height: 44.8rem; overflow: auto; padding: 1.2rem; border: 0.1rem solid var(--input-border); border-radius: 0 0 8px 8px; background: var(--card-background); }
    .editor:focus { outline: 0.1rem solid var(--color-brand); outline-offset: -1px; }
    .editor:empty::before { content: attr(data-placeholder); color: var(--color-text-muted); }
    .editor :is(h2, h3) { margin: 0.96rem 0 0.48rem; }
    .editor blockquote { margin: 0.96rem 0; padding-left: 1.28rem; border-left: 0.1rem solid var(--color-brand); color: var(--color-text-muted); }
  `,
})
export class RichTextEditor {
  /** The HTML of the text (two-way binding). */
  readonly html = model('');
  protected readonly emojis = ['🏸', '🎉', '🏆', '📅', '📍', '⚠️', '✅', '👏', '😀', '❤️'];
  private readonly editor = viewChild.required<ElementRef<HTMLDivElement>>('editor');

  constructor() {
    // Enter makes a paragraph (<p>), not a <div> (Chrome's default): the text is stored with the tags the API keeps.
    afterNextRender(() => document.execCommand('defaultParagraphSeparator', false, 'p'));
    // Puts the text in the editor when it comes from outside (loading), without moving the cursor while typing.
    effect(() => {
      const html = this.html();
      const element = this.editor().nativeElement;
      if (element.innerHTML !== html) {
        element.innerHTML = html;
      }
    });
  }

  protected changed(): void {
    const html = this.editor().nativeElement.innerHTML;
    // An emptied editor leaves "<br>" behind: it counts as empty.
    this.html.set(html === '<br>' ? '' : html);
  }

  protected run(command: string, value?: string): void {
    this.editor().nativeElement.focus();
    document.execCommand(command, false, value);
    this.changed();
  }

  protected block(tag: 'h2' | 'h3' | 'p' | 'blockquote'): void {
    this.run('formatBlock', tag);
  }

  protected insert(text: string): void {
    this.run('insertText', text);
  }

  /**
   * Adds a link on the selected text; with nothing selected, the address itself is inserted as a link
   * (the browser's own "createLink" does nothing without a selection, which made the link vanish).
   */
  protected link(): void {
    const element = this.editor().nativeElement;
    const selection = window.getSelection();
    const saved = selection && selection.rangeCount > 0 && element.contains(selection.getRangeAt(0).commonAncestorContainer) ? selection.getRangeAt(0).cloneRange() : null;

    const address = window.prompt('Adresse du lien (https://…)', 'https://')?.trim();
    if (!address || !/^(https?:\/\/|mailto:|tel:)/i.test(address)) {
      return;
    }

    // The prompt may have taken the selection away: put it back (or the cursor at the end of the text).
    element.focus();
    if (selection) {
      selection.removeAllRanges();
      if (saved) {
        selection.addRange(saved);
      } else {
        selection.selectAllChildren(element);
        selection.collapseToEnd();
      }
    }

    if (saved && !saved.collapsed) {
      this.run('createLink', address);
    } else {
      const safe = address.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
      this.run('insertHTML', `<a href="${safe}">${safe}</a>`);
    }
  }

  /** Pastes the text only: formatting copied from Word or a web page would bring unwanted markup. */
  protected pastePlainText(event: ClipboardEvent): void {
    event.preventDefault();
    const text = event.clipboardData?.getData('text/plain') ?? '';
    if (text) {
      this.run('insertText', text);
    }
  }
}
