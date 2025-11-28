import {
  Component,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import * as THREE from 'three';

import { ROUTE_DEF as navigator } from '@src/shared/constants/route-const';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class HomePageComponent {
  renderer: THREE.WebGLRenderer | null = null;
  scenes: THREE.Scene[] = [];
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
    {
      id: '4',
      title: 'Title',
      description:
        'Lorem, ipsum dolor sit amet consectetur adipisicing elit. Alias sapiente dolore delectus quo error reiciendis, ullam, sunt itaque eius est, recusandae ea deserunt pariatur praesentium eos maxime fuga consequatur ipsa! praesentium eos maxime fuga consequatur ipsa! praesentium eos maxime fuga consequatur ipsa! praesentium eos maxime fuga consequatur ipsa! praesentium eos maxime fuga consequatur ipsa! praesentium eos maxime fuga consequatur ipsa! praesentium eos maxime fuga consequatur ipsa! praesentium eos maxime fuga consequatur ipsa!',
      author: 'Author',
    },
  ];
  navigator = navigator;
}
