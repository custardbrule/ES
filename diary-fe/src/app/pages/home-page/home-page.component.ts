import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  NgZone,
  OnDestroy,
  PLATFORM_ID,
  ViewChild,
  ViewChildren,
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';

import { LoadModelServiceService } from '@src/app/services/threejs/load-model-service.service';
import { ROUTE_DEF as navigator } from '@src/shared/constants/route-const';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class HomePageComponent implements AfterViewInit, OnDestroy {
  @ViewChild('canvas', { static: true }) canvas!: ElementRef<HTMLCanvasElement>;
  @ViewChildren('child') children!: ElementRef<HTMLDivElement>[];
  private loaderService = inject(LoadModelServiceService);
  private ngZone = inject(NgZone);
  private platformId = inject(PLATFORM_ID);

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
  ];
  navigator = navigator;
  ngAfterViewInit(): void {
    // CRITICAL: Only run in browser
    if (!isPlatformBrowser(this.platformId)) {
      console.log('Skipping Three.js initialization on server');
      return;
    }

    setTimeout(() => {
      this.initRenderer();

      // init scene for each canvas
      this.scenes = this.children.map((child) => {
        const canvas = child.nativeElement;
        const scene = this.initScene(canvas);
        // Add lights - CRITICAL for seeing textures!
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
        scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(5, 15, 5);
        scene.add(directionalLight);

        const hemisphereLight = new THREE.HemisphereLight(
          0xffffff,
          0x444444,
          0.4
        );
        scene.add(hemisphereLight);

        // CRITICAL: Initialize clock and animation state
        scene.userData['isLoaded'] = false;
        scene.userData['animationStarted'] = false;

        this.loaderService.loadPreviewBook().then((res) => {
          scene.userData['mixer'] = res!.mixer;
          scene.userData['actions'] = res!.actions;
          scene.userData['isLoaded'] = true;
          scene.add(res!.model);
        });
        return scene;
      });

      this.ngZone.runOutsideAngular(() => {
        this.renderer?.setAnimationLoop(this.animate);
      });
    }, 100);
  }

  initRenderer() {
    const canvas = this.canvas.nativeElement;
    const { clientWidth, clientHeight } = canvas;
    this.renderer = new THREE.WebGLRenderer({
      alpha: true,
      antialias: true,
      powerPreference: 'high-performance',
      canvas: canvas,
    });
    this.renderer.setClearColor(0x000000, 0);
    this.renderer.setSize(clientWidth, clientHeight);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  }

  initScene(canvas: HTMLElement) {
    const { clientWidth, clientHeight } = canvas;
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(
      25,
      clientWidth / clientHeight,
      0.1,
      1000
    );

    // IMPORTANT: Position camera away from the object
    camera.position.set(10, 45, 30);
    camera.lookAt(0, 0, 0); // Look at the center

    scene.userData['camera'] = camera;
    scene.userData['element'] = canvas;
    scene.userData['controls'] = new OrbitControls(camera, canvas);
    scene.userData['clock'] = new THREE.Clock();
    return scene;
  }

  updateSize() {
    const canvas = this.canvas.nativeElement;
    const width = canvas.clientWidth;
    const height = canvas.clientHeight;

    if (canvas.width !== width || canvas.height !== height) {
      this.renderer!.setSize(width, height, false);
    }
  }

  animate = () => {
    if (!this.renderer) return;

    this.updateSize();

    // Sync canvas with scroll
    this.canvas.nativeElement.style.transform = `translateY(${window.scrollY}px)`;

    this.renderer.setClearColor(0x000000, 0);
    this.renderer.setScissorTest(false);
    this.renderer.clear();

    this.renderer.setScissorTest(true);

    this.scenes.forEach((scene) => {
      // get the element that is a place holder for where we want to
      // draw the scene
      const element: HTMLElement = scene.userData['element'];

      // get its position relative to the page's viewport
      const rect = element.getBoundingClientRect();
      const containerRect = element.parentElement!.getBoundingClientRect();

      // check if it's offscreen. If so skip it
      if (
        rect.bottom < 0 ||
        rect.top > this.renderer!.domElement.clientHeight ||
        rect.right < 0 ||
        rect.left > this.renderer!.domElement.clientWidth
      ) {
        return; // it's off screen
      }

      // set the viewport
      const width = rect.width;
      const height = rect.height;
      const left = 2 * containerRect.left - rect.left;
      const bottom = (containerRect.top - containerRect.bottom)*-1 - rect.top + 2*height;

      this.renderer!.setViewport(left, bottom, width, height);
      this.renderer!.setScissor(left, bottom, width, height);

      const camera = scene.userData['camera'];

      // CRITICAL: Start animation only when visible and loaded
      const mixer = scene.userData['mixer'];
      const clock = scene.userData['clock'];
      const isLoaded = scene.userData['isLoaded'];
      const animationStarted = scene.userData['animationStarted'];
      const actions = scene.userData['actions'];
      const controls: OrbitControls = scene.userData['controls'];

      if (mixer && clock && isLoaded) {
        // Start animation on first visible frame
        if (!animationStarted && actions) {
          actions.forEach((action: THREE.AnimationAction) => action.play());
          scene.userData['animationStarted'] = true;
          console.log('Animation started for scene');
        }

        const delta = clock.getDelta();
        mixer.update(delta);
      }

      controls.update();
      this.renderer!.render(scene, camera);
    });
  };

  ngOnDestroy(): void {
    this.scenes.forEach((scene) => {
      scene.traverse((object) => {
        if (object instanceof THREE.Mesh) {
          object.geometry?.dispose();
          if (Array.isArray(object.material)) {
            object.material.forEach((mat) => mat.dispose());
          } else {
            object.material?.dispose();
          }
        }
      });

      const controls = scene.userData['controls'];
      if (controls) {
        controls.dispose();
      }
    });

    if (this.renderer) {
      this.renderer.setAnimationLoop(null);
      this.renderer.dispose();
      this.renderer = null;
    }
  }
}
