// The plant and the chair.

import * as THREE from 'three';
import { scene, animated, box } from './stage.js';

// A plant in a pot, by the window. Its leaves sway.
const pot = new THREE.Mesh(
    new THREE.CylinderGeometry(0.2, 0.15, 0.36, 20),
    new THREE.MeshStandardMaterial({ color: 0xc9683c, roughness: 0.8 }));
pot.position.set(3.2, 0.18, -1.9);
pot.castShadow = true;
pot.receiveShadow = true;
scene.add(pot);
const leaves = new THREE.Group();
leaves.position.set(3.2, 0.36, -1.9);
scene.add(leaves);
[[0, 0.3, 0, 0.3], [-0.16, 0.5, 0.08, 0.22], [0.15, 0.56, -0.06, 0.2], [0.02, 0.78, 0.04, 0.2], [-0.1, 0.24, -0.14, 0.18], [0.16, 0.26, 0.14, 0.16]]
    .forEach(([x, y, z, size], i) => {
        const leaf = new THREE.Mesh(
            new THREE.IcosahedronGeometry(size, 1),
            new THREE.MeshStandardMaterial({ color: i % 2 ? 0x3f9e3c : 0x2f8a3a, roughness: 0.9, flatShading: true }));
        leaf.position.set(x, y, z);
        leaf.scale.y = 1.15;
        leaf.castShadow = true;
        leaves.add(leaf);
    });
animated.push((t) => {
    leaves.rotation.z = Math.sin(t * 0.7) * 0.025;
    leaves.rotation.x = Math.sin(t * 0.5 + 1) * 0.02;
});

// Chair, pulled back from the desk.
const chair = new THREE.Group();
chair.position.set(2.55, 0, -0.9);
chair.rotation.y = -0.6;
scene.add(chair);
box(0.56, 0.1, 0.54, 0x2e3158, 0, 0.5, 0, chair);
const chairBack = box(0.54, 0.6, 0.09, 0x2e3158, 0, 0.92, 0.26, chair);
chairBack.rotation.x = 0.12;
const post = new THREE.Mesh(new THREE.CylinderGeometry(0.035, 0.035, 0.4, 12), new THREE.MeshStandardMaterial({ color: 0x1e1e2a, roughness: 0.4, metalness: 0.6 }));
post.position.y = 0.26;
post.castShadow = true;
chair.add(post);
for (let i = 0; i < 5; i++) {
    const angle = i / 5 * Math.PI * 2;
    const leg = box(0.3, 0.035, 0.05, 0x1e1e2a, Math.cos(angle) * 0.15, 0.07, Math.sin(angle) * 0.15, chair);
    leg.rotation.y = -angle;
    const wheel = new THREE.Mesh(new THREE.SphereGeometry(0.035, 10, 8), new THREE.MeshStandardMaterial({ color: 0x111118, roughness: 0.5 }));
    wheel.position.set(Math.cos(angle) * 0.29, 0.035, Math.sin(angle) * 0.29);
    chair.add(wheel);
}
