import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { ToastsContainerComponent } from './shared/components/toasts-container/toasts-container.component';
@Component({
  selector: 'app-root',
  imports: [RouterOutlet,NavbarComponent,ToastsContainerComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('frontend');
}
