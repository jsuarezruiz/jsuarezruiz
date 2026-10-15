// The Microsoft MVP award: glass, on the MAUI shelf, with its stack of year rings.

import * as THREE from 'three';
import { scene, clickable, canvasTexture, onClick, sfx } from './stage.js';
import { say } from './tips.js';

const award = new THREE.Group();
award.position.set(-1.55, 1.125, -2.28);
award.rotation.y = 0.25;
award.scale.setScalar(1.2);
scene.add(award);

const parts = [];
function add(geometry, material, x, y, z) {
    const mesh = new THREE.Mesh(geometry, material);
    mesh.position.set(x, y, z);
    award.add(mesh);
    parts.push(mesh);
    return mesh;
}

// Glass is drawn see-through and without writing depth, so the pieces behind it still show.
function glass(color, opacity, roughness = 0.08) {
    return new THREE.MeshStandardMaterial({ color, transparent: true, opacity, roughness, metalness: 0.1, depthWrite: false });
}

// The clear slab: barely tinted, so what reads as glass is its bright edges. The globe near its
// top is frosted into it.
const slab = add(new THREE.BoxGeometry(0.13, 0.32, 0.035), glass(0xcfe4f4, 0.12), 0.02, 0.16, 0);
slab.add(new THREE.LineSegments(new THREE.EdgesGeometry(slab.geometry),
    new THREE.LineBasicMaterial({ color: 0xffffff, transparent: true, opacity: 0.7 })));
const globe = canvasTexture(128, 128, (g, w, h) => {
    g.strokeStyle = 'rgba(255,255,255,0.9)'; g.lineWidth = 4;
    g.beginPath(); g.arc(w / 2, h / 2, 58, 0, Math.PI * 2); g.stroke();
    g.lineWidth = 2; g.strokeStyle = 'rgba(255,255,255,0.55)';
    for (const k of [0.35, 0.75]) { g.beginPath(); g.ellipse(w / 2, h / 2, 58 * k, 58, 0, 0, Math.PI * 2); g.stroke(); }
    for (const y of [-30, 0, 30]) { g.beginPath(); g.moveTo(w / 2 - Math.sqrt(58 * 58 - y * y), h / 2 + y); g.lineTo(w / 2 + Math.sqrt(58 * 58 - y * y), h / 2 + y); g.stroke(); }
});
add(new THREE.PlaneGeometry(0.064, 0.064), new THREE.MeshStandardMaterial({ map: globe, transparent: true, depthWrite: false, roughness: 0.6 }), 0.02, 0.25, 0.0185);

// The blue wedge in front of it, rising to the right.
const wedgeShape = new THREE.Shape();
wedgeShape.moveTo(-0.015, 0);
wedgeShape.lineTo(0.085, 0);
wedgeShape.lineTo(0.085, 0.26);
wedgeShape.closePath();
const wedge = add(new THREE.ExtrudeGeometry(wedgeShape, { depth: 0.03, bevelEnabled: false }), glass(0x1f5fd6, 0.8, 0.05), 0, 0, 0.02);
wedge.castShadow = true;

// The silver MVP diamond on the wedge, with the words under it.
const badge = canvasTexture(128, 160, (g, w) => {
    g.save();
    g.translate(w / 2, 52); g.rotate(Math.PI / 4);
    g.fillStyle = '#c9ced6'; g.fillRect(-34, -34, 68, 68);
    g.restore();
    g.textAlign = 'center';
    g.fillStyle = '#1f4fc0'; g.font = '800 30px system-ui, sans-serif'; g.fillText('MVP', w / 2, 63);
    g.fillStyle = '#ffffff'; g.font = '600 15px system-ui, sans-serif';
    ['Microsoft', 'Most Valuable', 'Professional'].forEach((line, i) => g.fillText(line, w / 2, 118 + i * 16));
});
add(new THREE.PlaneGeometry(0.05, 0.0625), new THREE.MeshStandardMaterial({ map: badge, transparent: true, roughness: 0.4, depthWrite: false }), 0.058, 0.055, 0.0505);

// The year rings, stacked on a clear base in front of the slab: one per year, then a blue MVP ring.
const RINGS = ['2014', '2015', '2016', '2017', '2018', '2019', 'MVP'];
const RING = { r: 0.032, h: 0.024, x: -0.05, z: 0.035 };
add(new THREE.CylinderGeometry(RING.r + 0.003, RING.r + 0.003, 0.012, 32), glass(0xdfeaf4, 0.35), RING.x, 0.006, RING.z);
RINGS.forEach((label, i) => {
    const top = i === RINGS.length - 1;
    // The label wraps around the ring, so the canvas has the ring's proportions: about 9 to 1.
    const map = canvasTexture(576, 64, (g, w, h) => {
        g.fillStyle = top ? '#1f4fc0' : '#e6eaee'; g.fillRect(0, 0, w, h);
        g.fillStyle = top ? '#ffffff' : '#2c3440';
        g.font = '700 36px system-ui, sans-serif'; g.textAlign = 'center'; g.textBaseline = 'middle';
        g.fillText(label, w / 2, h / 2 + 2);
    });
    const ring = add(new THREE.CylinderGeometry(RING.r, RING.r, RING.h - 0.002, 32),
        new THREE.MeshStandardMaterial({ map, roughness: top ? 0.2 : 0.6 }),
        RING.x, 0.012 + RING.h * (i + 0.5), RING.z);
    // A cylinder's texture starts at its back; turn it so the label faces the room.
    ring.rotation.y = Math.PI;
    ring.castShadow = true;
});

for (const mesh of parts) clickable(mesh, 'award', 'Microsoft MVP award');

onClick('award', (event) => {
    sfx('achievement');
    say('Microsoft MVP, 2014 to 2019 · Xamarin MVP', event);
});
