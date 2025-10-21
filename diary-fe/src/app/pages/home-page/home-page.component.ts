import { Component } from '@angular/core';

import { BookPreviewComponent } from '@components/ThreeBooks/preview/preview.component';

@Component({
  selector: 'app-home-page',
  imports: [BookPreviewComponent],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class HomePageComponent {
  constructor() {}
}
