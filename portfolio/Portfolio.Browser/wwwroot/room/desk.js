// The desk and the small things on it: the CV, the mug, the rubber duck and the lamp.

import * as THREE from 'three';
import { scene, time, animated, box, clickable, canvasTexture, puff, onClick, sfx } from './stage.js';
import { say } from './tips.js';

box(2.6, 0.08, 1.1, 0xb9824e, 0.7, 0.96, -1.85);
for (const [x, z] of [[-0.5, -1.4], [1.9, -1.4], [-0.5, -2.3], [1.9, -2.3]])
    box(0.1, 0.92, 0.1, 0x6f4a2c, x, 0.46, z);
box(0.8, 0.5, 0.9, 0xa8733f, 1.45, 0.67, -1.85);

// The CV.
const paper = new THREE.Mesh(
    new THREE.BoxGeometry(0.32, 0.006, 0.44),
    new THREE.MeshStandardMaterial({
        roughness: 0.95,
        map: canvasTexture(320, 440, (g, w, h) => {
            g.fillStyle = '#ffffff'; g.fillRect(0, 0, w, h);
            g.fillStyle = '#1e1e2a'; g.font = '700 30px system-ui, sans-serif'; g.fillText('Javier Suárez', 32, 62);
            g.fillStyle = '#2456c8'; g.fillRect(32, 78, 120, 5);
            g.fillStyle = '#b4b8c8';
            for (let y = 112; y < h - 40; y += 22) g.fillRect(32, y, (y / 22) % 4 === 1 ? 150 : 256, 7);
        }),
    }));
paper.position.set(-0.1, 1.004, -1.55);
paper.rotation.y = 0.22;
paper.receiveShadow = true;
scene.add(clickable(paper, 'paper', 'Paper · read the CV'));

// Coffee mug, with steam.
const mug = new THREE.Group();
mug.position.set(1.3, 1.0, -2.1);
scene.add(mug);
const mugParts = [
    new THREE.Mesh(new THREE.CylinderGeometry(0.048, 0.042, 0.1, 16), new THREE.MeshStandardMaterial({ color: 0xf4f1e6, roughness: 0.4 })),
    new THREE.Mesh(new THREE.TorusGeometry(0.03, 0.009, 8, 14, Math.PI), new THREE.MeshStandardMaterial({ color: 0xf4f1e6, roughness: 0.4 })),
];
mugParts[0].position.y = 0.05;
mugParts[1].position.set(0.048, 0.05, 0);
mugParts[1].rotation.z = -Math.PI / 2;
const coffee = new THREE.Mesh(new THREE.CircleGeometry(0.042, 16), new THREE.MeshStandardMaterial({ color: 0x3a2314, roughness: 0.3 }));
coffee.rotation.x = -Math.PI / 2;
coffee.position.y = 0.094;
mug.add(...mugParts, coffee);
for (const mesh of mugParts) {
    mesh.castShadow = true;
    clickable(mesh, 'mug', 'Coffee · still warm');
}

const steam = [0, 1, 2].map(() => puff(null, 'rgba(255,255,255,0.5)'));
animated.push((t) => steam.forEach((wisp, i) => {
    const life = (t * 0.35 + i / steam.length) % 1;
    wisp.position.set(1.3 + Math.sin(t * 1.3 + i * 2) * 0.012, 1.11 + life * 0.2, -2.1);
    wisp.scale.setScalar(0.03 + life * 0.05);
    wisp.material.opacity = 0.55 * (1 - life);
}));

// Rubber duck: click it and it asks the questions a rubber duck asks.
const duck = new THREE.Group();
duck.position.set(-0.02, 1.0, -2.0);
duck.rotation.y = 0.5;
scene.add(duck);
const duckYellow = new THREE.MeshStandardMaterial({ color: 0xf7cf2e, roughness: 0.45 });
const duckBody = new THREE.Mesh(new THREE.SphereGeometry(0.06, 14, 10), duckYellow);
duckBody.scale.set(1.2, 0.8, 0.95);
duckBody.position.y = 0.048;
const duckHead = new THREE.Mesh(new THREE.SphereGeometry(0.04, 14, 10), duckYellow);
duckHead.position.set(0.045, 0.115, 0);
const duckTail = new THREE.Mesh(new THREE.ConeGeometry(0.025, 0.05, 8), duckYellow);
duckTail.position.set(-0.07, 0.075, 0);
duckTail.rotation.z = 0.9;
duck.add(duckBody, duckHead, duckTail);
for (const mesh of [duckBody, duckHead, duckTail]) {
    mesh.castShadow = true;
    clickable(mesh, 'duck', 'Rubber duck · explain your bug');
}
box(0.035, 0.014, 0.034, 0xf08a2c, 0.088, 0.108, 0, duck);
for (const z of [-0.018, 0.018])
    box(0.008, 0.01, 0.008, 0x1e1e2a, 0.076, 0.128, z, duck);

const DUCK_LINES = [
    'Explain it to me, line by line.',
    'What did you expect to happen?',
    'Did it ever work?',
    'Have you deleted bin and obj?',
    'Quack. (Read the error message.)',
    'Is it Measure, or is it Arrange?',
];
let duckLine = Math.floor(Math.random() * DUCK_LINES.length);
let duckSquish = 0;
animated.push(() => {
    // Squashes when pressed, then springs back.
    duckSquish *= 0.86;
    duck.scale.set(1 + duckSquish * 0.25, 1 - duckSquish * 0.4, 1 + duckSquish * 0.25);
});

// The desk lamp.

const shade = new THREE.Mesh(
    new THREE.CylinderGeometry(0.075, 0.14, 0.16, 24, 1, true),
    new THREE.MeshStandardMaterial({ color: 0xf2c230, roughness: 0.6, side: THREE.DoubleSide }));
shade.position.set(-0.42, 1.55, -2.12);
shade.castShadow = true;
scene.add(shade);
const lampParts = [
    box(0.2, 0.03, 0.2, 0x2e3158, -0.45, 1.015, -2.15),
    box(0.03, 0.5, 0.03, 0x2e3158, -0.45, 1.27, -2.15),
    shade,
];
const lamp = new THREE.PointLight(0xffd9a0, 0, 4);
lamp.position.set(-0.42, 1.42, -2.0);
lamp.castShadow = true;
lamp.shadow.mapSize.set(512, 512);
lamp.shadow.bias = -0.002;
scene.add(lamp);
for (const mesh of lampParts) clickable(mesh, 'lamp', 'Lamp · switch on or off');

// The lamp starts on unless it is daytime outside.
let lampOn = time !== 'day';
function setLamp(on) {
    lampOn = on;
    lamp.intensity = on ? 2.6 : 0;
    shade.userData.glow = on ? 0xc89a1c : 0x000000;
    shade.material.emissive.setHex(shade.userData.glow);
}
setLamp(lampOn);

// Dark for the opening shot, whatever the switch says.
function dimLamp() {
    lamp.intensity = 0;
    shade.material.emissive.setHex(0x000000);
}

function isLampOn() {
    return lampOn;
}

onClick('lamp', () => { sfx('switch'); setLamp(!lampOn); });
onClick('mug', (event) => say('Coffee: the real runtime.', event));
onClick('duck', (event) => {
    sfx('squeak');
    duckSquish = 1;
    duckLine = (duckLine + 1) % DUCK_LINES.length;
    say(DUCK_LINES[duckLine], event);
});

export { setLamp, isLampOn, dimLamp, paper };
