// The Xamarin monkey: a plush toy sitting on the Xamarin shelf, next to the books.

import * as THREE from 'three';
import { scene, animated, clickable, canvasTexture, onClick, sfx } from './stage.js';
import { say } from './tips.js';

// Built at about a metre tall and scaled down, so the numbers stay readable.
const monkey = new THREE.Group();
monkey.position.set(-1.62, 0.425, -2.29);
monkey.rotation.y = 0.3;
monkey.scale.setScalar(0.38);
scene.add(monkey);

const FUR = 0x1c1c24, TAN = 0xb5773f, EYE = 0x0c0c10, MOUTH = 0x3a2418;
const parts = [];

// A sphere stretched into the part it stands for; plush, so nothing is shiny.
function blob(radius, color, x, y, z, sx = 1, sy = 1, sz = 1, parent = monkey) {
    const mesh = new THREE.Mesh(
        new THREE.SphereGeometry(radius, 20, 14),
        new THREE.MeshStandardMaterial({ color, roughness: 0.95 }));
    mesh.position.set(x, y, z);
    mesh.scale.set(sx, sy, sz);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    parent.add(mesh);
    parts.push(mesh);
    return mesh;
}

// Sitting with its legs out in front, big tan pads on the soles, like the real one.
const body = blob(0.3, FUR, 0, 0.38, 0, 1, 1.05, 0.85);
for (const side of [-1, 1]) {
    const leg = blob(0.11, FUR, side * 0.16, 0.11, 0.24, 1, 1, 2.0);
    leg.rotation.y = side * 0.35;
    const pad = blob(0.11, TAN, side * 0.24, 0.13, 0.45, 1, 1.15, 0.45);
    pad.rotation.y = side * 0.35;
    const arm = blob(0.1, FUR, side * 0.3, 0.38, 0.06, 0.85, 1.9, 0.85);
    arm.rotation.set(-0.3, 0, side * 0.25);
}

// The shirt: "Xamarin" printed around the front of the body. A slice of a cylinder, parented to
// the body so it takes the body's squash and sits just outside its surface.
const shirt = new THREE.Mesh(
    new THREE.CylinderGeometry(0.305, 0.305, 0.12, 24, 1, true, -0.8, 1.6),
    new THREE.MeshStandardMaterial({
        transparent: true,
        alphaTest: 0.3,
        roughness: 0.95,
        map: canvasTexture(512, 126, (g, w, h) => {
            g.fillStyle = '#c9ccd3';
            g.font = '600 64px system-ui, sans-serif';
            g.textAlign = 'center';
            g.textBaseline = 'middle';
            g.fillText('Xamarin', w / 2, h / 2 + 4);
        }),
    }));
shirt.position.y = -0.02;
body.add(shirt);

// The head: black, with a tan face shaped like a heart, two eyes and a little smile.
blob(0.27, FUR, 0, 0.83, 0.02, 1.05, 0.95, 0.95);
blob(0.17, TAN, 0, 0.74, 0.17, 1.25, 0.85, 0.85);                    // the muzzle
blob(0.1, TAN, -0.08, 0.88, 0.25, 1, 1.05, 0.6);                     // around the eyes
blob(0.1, TAN, 0.08, 0.88, 0.25, 1, 1.05, 0.6);
blob(0.1, TAN, 0, 0.86, 0.26, 1.4, 0.8, 0.6);
for (const side of [-1, 1]) {
    blob(0.038, EYE, side * 0.078, 0.888, 0.3, 1, 1.1, 0.7);           // big, glossy button eyes
    blob(0.012, 0xffffff, side * 0.078 + 0.012, 0.903, 0.324);         // the shine in each eye
    const brow = blob(0.04, TAN, side * 0.085, 0.948, 0.285, 1.3, 0.4, 0.6); // a soft brow over each eye
    brow.rotation.z = -side * 0.25;
    blob(0.011, MOUTH, side * 0.022, 0.802, 0.308, 1.2, 0.8, 0.6);    // embroidered nostrils
    const ear = blob(0.095, FUR, side * 0.27, 0.86, -0.02, 1, 1, 0.4);
    ear.rotation.y = side * 0.5;
    const inner = blob(0.075, TAN, side * 0.275, 0.86, 0.0, 1, 1, 0.4);
    inner.rotation.y = side * 0.5;
}
// An embroidered smile, wide enough to read from across the room.
const smile = new THREE.Mesh(
    new THREE.TorusGeometry(0.05, 0.008, 8, 20, Math.PI),
    new THREE.MeshStandardMaterial({ color: MOUTH, roughness: 0.9 }));
smile.position.set(0, 0.772, 0.31);
smile.rotation.set(-0.35, 0, Math.PI);
monkey.add(smile);
parts.push(smile);

for (const mesh of parts) clickable(mesh, 'monkey', 'Xamarin monkey');

// Squeezed, it squeaks and hops.
const MONKEY_LINES = ['*squeak*', 'Ooh ooh, ah ah!', 'Write once, run everywhere!', 'I was here before MAUI.', 'Still waiting for my Avalonia shirt.'];
let hopAt = -10;
let now = 0;
animated.push((t) => {
    now = t;
    const k = (t - hopAt) / 0.45;
    const hop = k >= 0 && k < 1 ? Math.sin(k * Math.PI) : 0;
    monkey.position.y = 0.425 + hop * 0.06;
    monkey.rotation.z = hop * Math.sin(k * Math.PI * 2) * 0.12;
});

onClick('monkey', (event) => {
    sfx('squeak');
    hopAt = now;
    say(MONKEY_LINES[Math.floor(Math.random() * MONKEY_LINES.length)], event);
});
