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
  
  // Add these properties to store animation state
  private mixer: THREE.AnimationMixer | null = null;
  private clock = new THREE.Clock();

  constructor() {}
  
  ngAfterViewInit(): void {
    this.initThreeJS();
    this.curveBox();
  }

  private initThreeJS(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const element = this.container()!.nativeElement;
    const { clientWidth, clientHeight } = element;
    console.log(clientWidth, clientHeight);

    this.scene = new THREE.Scene();
    this.scene.background = null;
    
    // Add lights - CRITICAL for seeing textures!
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
    this.scene.add(ambientLight);

    const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
    directionalLight.position.set(5, 10, 7);
    this.scene.add(directionalLight);

    const hemisphereLight = new THREE.HemisphereLight(0xffffff, 0x444444, 0.4);
    this.scene.add(hemisphereLight);
    
    this.camera = new THREE.PerspectiveCamera(
      25,
      clientWidth / clientHeight,
      0.1,
      100
    );
    this.renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
    this.renderer.setClearColor(0x000000, 0);
    this.renderer.setSize(clientWidth, clientHeight);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    element.appendChild(this.renderer.domElement);
  }

  private async curveBox(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;

    const loader = new GLTFLoader();
    const textureLoader = new TextureLoader();

    try {
      // Load all assets concurrently using built-in loadAsync
      const [gltf, coverTexture, fabricTexture] = await Promise.all([
        loader.loadAsync('assets/models/preview-book/book.gltf'),
        textureLoader.loadAsync('assets/models/preview-book/bood-side.jpg'),
        textureLoader.loadAsync('assets/models/preview-book/fabric-1.jpg')
      ]);

      console.log('All assets loaded successfully!');

      const model = gltf.scene;

      // Apply textures to the model
      model.traverse((child) => {
        if (child instanceof THREE.Mesh && child.material) {
          console.log('Mesh name:', child.name, 'Material:', child.material.name);
          
          // Clone material to avoid affecting other meshes
          const material = child.material.clone();
          
          // Apply textures based on material name from GLTF
          if (material.name === 'Material.003' || child.name === 'BookMesh') {
            material.map = coverTexture;
            material.needsUpdate = true;
          } else if (material.name === 'Material.004') {
            material.map = fabricTexture;
            material.needsUpdate = true;
          }
          
          child.material = material;
        }
      });

      // Add model to scene
      this.scene.add(model);

      // Setup animations - CRITICAL: Store mixer as instance property
      this.mixer = new THREE.AnimationMixer(model);
      
      // Play all animations ONCE
      if (gltf.animations && gltf.animations.length > 0) {
        console.log(`Found ${gltf.animations.length} animations:`, gltf.animations.map(a => a.name));
        
        gltf.animations.forEach((clip) => {
          const action = this.mixer!.clipAction(clip);
          
          // Configure to play only once
          action.setLoop(THREE.LoopOnce, 1);
          action.clampWhenFinished = true; // Stay on last frame when finished
          
          action.play();
          console.log(`Playing animation once: ${clip.name}`);
        });
      } else {
        console.warn('No animations found in GLTF');
      }

      // Start the animation loop after everything is loaded
      this.startAnimation();

    } catch (error) {
      console.error('Error loading assets:', error);
    }

    this.camera.position.set(-10, 30, 45);
    const controls = new OrbitControls(this.camera, this.renderer.domElement);
  }

  private loadGLTF(loader: GLTFLoader, url: string): Promise<any> {
    return new Promise((resolve, reject) => {
      loader.load(
        url,
        (gltf) => resolve(gltf),
        (xhr) => {
          console.log(`GLTF: ${(xhr.loaded / xhr.total * 100).toFixed(2)}% loaded`);
        },
        (error) => reject(error)
      );
    });
  }

  private loadTexture(loader: TextureLoader, url: string): Promise<THREE.Texture> {
    return new Promise((resolve, reject) => {
      loader.load(
        url,
        (texture) => {
          console.log(`Texture loaded: ${url}`);
          resolve(texture);
        },
        undefined,
        (error) => {
          console.error(`Error loading texture: ${url}`, error);
          reject(error);
        }
      );
    });
  }

  private startAnimation(): void {
    const animate = () => {
      // CRITICAL: Update the mixer with delta time
      if (this.mixer) {
        const delta = this.clock.getDelta();
        this.mixer.update(delta);
      }

      this.renderer.render(this.scene, this.camera);
    };

    // Run animation outside Angular zone
    this.ngZone.runOutsideAngular(() =>
      this.renderer.setAnimationLoop(animate)
    );
  }

  // Optional: Clean up on destroy
  ngOnDestroy(): void {
    if (this.mixer) {
      this.mixer.stopAllAction();
    }
    if (this.renderer) {
      this.renderer.dispose();
    }
  }
}