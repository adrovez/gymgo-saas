import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'replace', standalone: true })
export class ReplacePipe implements PipeTransform {
  transform(value: string | null | undefined, search: string, replacement: string): string {
    if (value == null) return '';
    return value.split(search).join(replacement);
  }
}
