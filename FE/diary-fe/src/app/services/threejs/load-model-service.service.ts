import { Injectable } from '@angular/core';
import { generateImage } from '@src/shared/utils/image';
import * as THREE from 'three';
import { TextureLoader } from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';

@Injectable({
  providedIn: 'root',
})
export class LoadModelServiceService {
  loader: GLTFLoader = new GLTFLoader();
  textureLoader: TextureLoader = new TextureLoader();
  private _previewBookModel?: GLTF;
  constructor() {}

  private async loadPreviewBookMoel() {
    if (!this._previewBookModel) {
      this._previewBookModel = await this.loader.loadAsync(
        '/assets/models/preview-book/book.gltf'
      );
    }
    return this._previewBookModel;
  }

  async loadPreviewBook(data: { title: string; author: string }) {
    const c = document.createElement('canvas');
    try {
      // Load all assets concurrently using built-in loadAsync
      const [gltf, coverTexture, fabricTexture, pageTexture] =
        await Promise.all([
          this.loadPreviewBookMoel(),
          this.textureLoader.loadAsync(
            '/assets/models/preview-book/bood-side.jpg'
          ),
          this.textureLoader.loadAsync(
            '/assets/models/preview-book/fabric-1.jpg'
          ),
          this.textureLoader
            .loadAsync(
              generateImage(c, {
                ...data,
                fontSize: 20,
                fontFamily: 'serif',
                textColor: 'black',
                text: 'holaaaaaaa',
                bgColor: 'white',
              })
            )
            .finally(() => c.remove()),
        ]);

      console.log('All assets loaded successfully!');

      const model = gltf.scene;

      // Apply textures to the model
      model.traverse((child) => {
        if (child instanceof THREE.Mesh && child.material) {
          console.log(
            'Mesh name:',
            child.name,
            'Material:',
            child.material.name
          );

          // Clone material to avoid affecting other meshes
          const material = child.material.clone();

          // Apply textures based on material name from GLTF
          if (child.isMesh && child.parent && child.parent.name === 'Page1') {
            material.map = pageTexture;
            material.needsUpdate = true;
          } else if (
            child.isMesh &&
            child.parent &&
            child.parent.name === 'Page2'
          ) {
            material.map = pageTexture;
            material.needsUpdate = true;
          } else if (
            material.name === 'Material.003' ||
            child.name === 'BookMesh'
          ) {
            material.map = coverTexture;
            material.needsUpdate = true;
          } else if (material.name === 'Material.004') {
            material.map = fabricTexture;
            material.needsUpdate = true;
          }

          child.material = material;
        }
      });

      // Setup animations - CRITICAL: Store mixer as instance property
      const mixer = new THREE.AnimationMixer(model);

      // Setup animations but DON'T play yet
      const actions: THREE.AnimationAction[] = [];
      if (gltf.animations && gltf.animations.length > 0) {
        gltf.animations.forEach((clip) => {
          const action = mixer.clipAction(clip);
          action.setLoop(THREE.LoopOnce, 1);
          action.clampWhenFinished = true;
          actions.push(action);
        });
      }
      return { model, mixer, actions };
    } catch (error) {
      console.error('Error loading assets:', error);
      return null;
    }
  }
}
