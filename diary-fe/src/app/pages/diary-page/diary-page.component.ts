import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  NgZone,
  ViewChild,
  ViewChildren,
} from '@angular/core';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';

@Component({
  selector: 'app-diary-page',
  imports: [],
  templateUrl: './diary-page.component.html',
  styleUrl: './diary-page.component.scss',
})
export class DiaryPageComponent implements AfterViewInit {
  @ViewChild('canvas', { static: true }) canvas!: ElementRef<HTMLCanvasElement>;
  @ViewChildren('child') children!: ElementRef<HTMLCanvasElement>[];
  private ngZone = inject(NgZone);

  renderer: THREE.WebGLRenderer | null = null;
  scenes: THREE.Scene[] = [];

  ngAfterViewInit(): void {
    this.initRenderer();

    // init scene for each canvas
    this.scenes = this.children.map((child) => {
      const canvas = child.nativeElement;
      const scene = this.initScene(canvas);
      const model = this.loadModel();
      scene.add(model);

      return scene;
    });

    this.ngZone.runOutsideAngular(() => {
      this.renderer?.setAnimationLoop(this.animate);
    });
  }

  initRenderer() {
    const canvas = this.canvas.nativeElement;
    const { clientWidth, clientHeight } = canvas;
    this.renderer = new THREE.WebGLRenderer({
      alpha: true,
      antialias: true,
      canvas: canvas,
    });
    this.renderer.setClearColor(0x000000, 0);
    this.renderer.setSize(clientWidth, clientHeight);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  }

  loadModel() {
    const geometry = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({ color: 0x00ff00 });
    return new THREE.Mesh(geometry, material);
  }

  initScene(canvas: HTMLCanvasElement) {
    const { clientWidth, clientHeight } = canvas;
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(
      25,
      clientWidth / clientHeight,
      0.1,
      1000
    );

    // IMPORTANT: Position camera away from the object
    camera.position.set(0, 0, 5); // Move camera back 5 units
    camera.lookAt(0, 0, 0); // Look at the center
    
    scene.userData['camera'] = camera;
    scene.userData['element'] = canvas;
    scene.userData['controls'] = new OrbitControls(camera, canvas);
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

  animate = () =>{
    if (!this.renderer) return;

    this.updateSize();

    this.renderer.setClearColor(0xffffff);
    this.renderer.setScissorTest(false);
    this.renderer.clear();

    this.renderer.setScissorTest(true);

    this.scenes.forEach((scene) => {
      // so something moves
      scene.children[0].rotation.y = Date.now() * 0.001;

      // get the element that is a place holder for where we want to
      // draw the scene
      const element = scene.userData['element'];

      // get its position relative to the page's viewport
      const rect = element.getBoundingClientRect();

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
      const width = rect.right - rect.left;
      const height = rect.bottom - rect.top;
      const left = rect.left;
      const bottom = this.canvas.nativeElement.clientHeight - rect.bottom;
      // const bottom = this.renderer!.domElement.clientHeight - rect.bottom;

      this.renderer!.setViewport(left, bottom, width, height);
      this.renderer!.setScissor(left, bottom, width, height);

      const camera = scene.userData['camera'];


      scene.userData['controls'].update();

      this.renderer!.render(scene, camera);
    });
  }
}
