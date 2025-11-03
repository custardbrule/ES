import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { BookPreviewComponent } from '@components/ThreeBooks/preview/preview.component';
import { ROUTE_DEF as navigator } from '@src/shared/constants/route-const';

@Component({
  selector: 'app-home-page',
  imports: [BookPreviewComponent, RouterLink],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class HomePageComponent {
  collections: {
    id: string;
    title: string;
    description: string;
    author: string;
  }[] = [
    {
      id: '1',
      title: 'Title',
      description:
        'Lorem, ipsum dolor sit amet consectetur adipisicing elit. Alias sapiente dolore delectus quo error reiciendis, ullam, sunt itaque eius est, recusandae ea deserunt pariatur praesentium eos maxime fuga consequatur ipsa!',
      author: 'lorem',
    },
    {
      id: '2',
      title: 'Title',
      description:
        'Lorem, ipsum dolor sit amet consectetur adipisicing elit. Alias sapiente dolore delectus quo error reiciendis, ullam, sunt itaque eius est, recusandae ea deserunt pariatur praesentium eos maxime fuga consequatur ipsa!',
      author: 'Author',
    },
    {
      id: '3',
      title: 'Title',
      description: 'Lorem, ipsum dolor sit amee fuga consequatur ipsa!',
      author: 'Author',
    },
  ];
  navigator = navigator;
  constructor() {}
}
