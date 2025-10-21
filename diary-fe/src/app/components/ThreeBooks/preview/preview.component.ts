import { CommonModule, isPlatformBrowser, NgStyle } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  Input,
  NgZone,
  PLATFORM_ID,
  viewChild,
} from '@angular/core';
import * as THREE from 'three';
import { TextureLoader } from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';

@Component({
  selector: 'book-preview',
  imports: [CommonModule, NgStyle],
  templateUrl: './preview.component.html',
  styleUrl: './preview.component.scss',
})
export class BookPreviewComponent implements AfterViewInit {
  @Input() height: string = '200px';
  @Input() width: string = '200px';

  private container = viewChild<ElementRef<HTMLDivElement>>('canvasContainer');
  private platformId = inject(PLATFORM_ID);
  private ngZone = inject(NgZone);

  private camera!: THREE.PerspectiveCamera;
  private scene!: THREE.Scene;
  private renderer!: THREE.WebGLRenderer;

  constructor() {}
  ngAfterViewInit(): void {
    this.initThreeJS();
    // this.renderCube();
    this.curveBox();
  }

  private initThreeJS(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const element = this.container()!.nativeElement;
    const { clientWidth, clientHeight } = element;
    console.log(clientWidth, clientHeight);

    this.scene = new THREE.Scene();
    this.scene.background = null;
    this.camera = new THREE.PerspectiveCamera(
      25,
      clientWidth / clientHeight,
      0.1,
      100
    );
    this.renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
    this.renderer.setClearColor(0x000000, 0); // Sets clear color to black with 0 opacity
    this.renderer.setSize(clientWidth, clientHeight);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    element.appendChild(this.renderer.domElement);
  }

  private renderCube(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const geometry = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({
      color: 0x00ff00,
    });
    const cube = new THREE.Mesh(geometry, material);
    this.scene.add(cube);

    this.camera.position.z = 3;
    const controls = new OrbitControls(this.camera, this.renderer.domElement);

    const animate = () => {
      this.renderer.render(this.scene, this.camera);
    };

    // Run animation outside Angular zone so it doesn't affect stability
    this.ngZone.runOutsideAngular(() =>
      this.renderer.setAnimationLoop(animate)
    );
  }

  private curveBox(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    // const geometry = new THREE.PlaneGeometry(1, 2, 10, 10);
    // const position = geometry.getAttribute('position');
    // for (let i = 0; i < position.count; i++) {
    //   const x = position.getX(i);
    //   const y = position.getY(i);

    //   // Curve to one side like a cylinder
    //   // x ranges from -0.5 to 0.5 (normalized plane coordinates)
    //   const normalizedX = x + 0.5; // Now ranges from 0 to 1
    //   const angle = normalizedX * Math.PI * 0.5; // 0 to 90 degrees (quarter cylinder)

    //   const radius = 0.5; // Cylinder radius
    //   const newZ = radius * (1 - Math.cos(angle)); // Depth curve
    //   const newX = radius * Math.sin(angle); // Horizontal displacement

    //   position.setX(i, newX);
    //   position.setZ(i, newZ);
    // }
    // position.needsUpdate = true; // Important to update the geometry
    // geometry.computeVertexNormals(); // Recalculate normals for proper lighting

    // const material = new THREE.MeshBasicMaterial({
    //   color: 0x00ff00,
    //   side: THREE.DoubleSide,
    //   wireframe: true,
    // });
    // const mesh = new THREE.Mesh(geometry, material);
    const loader = new GLTFLoader();
    const textureLoader = new TextureLoader();

    loader.load(
      'assets/models/preview-book/book.gltf',
      (gltf) => {
        const model = gltf.scene;

        // Load textures
        const coverTexture = textureLoader.load(
          'assets/models/preview-book/bood-side.jpg'
        );
        const fabricTexture = textureLoader.load(
          'assets/models/preview-book/fabric-1.jpg'
        );

        // Apply textures to specific parts
        model.traverse((child) => {
          if (child instanceof THREE.Mesh) {
            // Apply to all meshes
            child.material.map = coverTexture;
            child.material.needsUpdate = true;

            // Or apply based on name
            if (child.name === 'BookMesh') {
              child.material.map = coverTexture;
            } else if (child.name === 'BookSpine') {
              child.material.map = fabricTexture;
            }
          }
        });

        this.scene.add(model);
      },
      function (xhr) {
        console.log((xhr.loaded / xhr.total) * 100 + '% loaded');
      },
      function (error) {
        console.error('An error occurred:', error);
      }
    );

    this.camera.position.z = 50;
    const controls = new OrbitControls(this.camera, this.renderer.domElement);

    const animate = () => {
      this.renderer.render(this.scene, this.camera);
    };

    // Run animation outside Angular zone so it doesn't affect stability
    this.ngZone.runOutsideAngular(() =>
      this.renderer.setAnimationLoop(animate)
    );
  }
}
