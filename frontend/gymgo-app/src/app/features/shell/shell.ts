import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar';
import { ForbiddenMessageService } from '../../core/services/forbidden-message.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  templateUrl: './shell.html',
})
export class ShellComponent {
  readonly forbidden = inject(ForbiddenMessageService);
}
