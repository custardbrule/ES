import { isPlatformBrowser } from '@angular/common';
import {
  Component,
  ElementRef,
  viewChild,
  inject,
  PLATFORM_ID,
  NgZone,
  OnInit,
} from '@angular/core';
import * as THREE from 'three';

@Component({
  selector: 'app-home-page',
  imports: [],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class HomePageComponent implements OnInit {
  private container = viewChild<ElementRef<HTMLDivElement>>('canvasContainer');
  private platformId = inject(PLATFORM_ID);
  private ngZone = inject(NgZone);

  constructor() {}
  ngOnInit(): void {
    this.initThreeJS();
  }

  private initThreeJS(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const element = this.container()!.nativeElement;
    const { clientWidth, clientHeight } = element;

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(
      75,
      clientWidth / clientHeight,
      0.1,
      1000
    );
    const renderer = new THREE.WebGLRenderer();

    renderer.setSize(clientWidth, clientHeight);
    element.appendChild(renderer.domElement);

    const geometry = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({
      color: 0x00ff00,
    });
    const cube = new THREE.Mesh(geometry, material);
    scene.add(cube);

    camera.position.z = 5;

    const animate = () => {
      cube.rotation.x += 0.01;
      cube.rotation.y += 0.01;

      renderer.render(scene, camera);
    };

    // Run animation outside Angular zone so it doesn't affect stability
    this.ngZone.runOutsideAngular(() => renderer.setAnimationLoop(animate));
  }
}
