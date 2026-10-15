// The laptop and the phone: the two screens the visitor can step into.

import * as THREE from 'three';
import { scene, box, clickable, canvasTexture, onClick, sfx } from './stage.js';
import { say } from './tips.js';

// The computer: a slim aluminium laptop, lid tilted back.
const ALU = 0xc9ccd2;
const laptopHit = [];
laptopHit.push(box(0.92, 0.03, 0.62, ALU, 0.7, 1.015, -1.75));
box(0.8, 0.004, 0.3, 0x2a2c33, 0.7, 1.032, -1.84);
box(0.28, 0.004, 0.17, 0xb4b8c0, 0.7, 1.032, -1.56);

const lid = new THREE.Group();
lid.position.set(0.7, 1.03, -2.05);
lid.rotation.x = -0.2;
scene.add(lid);
laptopHit.push(box(0.92, 0.6, 0.02, ALU, 0, 0.3, 0, lid));
box(0.88, 0.56, 0.004, 0x14151a, 0, 0.3, 0.011, lid);

const SCREEN = { w: 0.84, h: 0.525, center: new THREE.Vector3(), normal: new THREE.Vector3() };
const screen = new THREE.Mesh(
    new THREE.PlaneGeometry(SCREEN.w, SCREEN.h),
    new THREE.MeshBasicMaterial({
        toneMapped: false,
        // The pixel OS really is pixel art, so its preview is the one texture left unsmoothed.
        map: canvasTexture(160, 100, (g, w, h) => {
            ['#2f6fe0', '#4a8bef', '#6fa8f7', '#9cc6fb'].forEach((c, i) => { g.fillStyle = c; g.fillRect(0, i * 15, w, 15); });
            g.fillStyle = '#3f9e3c';
            for (let x = 0; x < w; x++) {
                const top = Math.round(h * (0.62 - 0.09 * Math.sin(x / w * Math.PI * 0.9 + 0.5)));
                g.fillRect(x, top, 1, h - top);
            }
            g.fillStyle = '#2a5fd6'; g.fillRect(0, h - 8, w, 8);
            g.fillStyle = '#3da83d'; g.fillRect(0, h - 8, 22, 8);
            g.fillStyle = '#f2c230';
            for (let i = 0; i < 4; i++) g.fillRect(6, 6 + i * 15, 9, 8);
        }, true),
    }));
screen.position.set(0, 0.3, 0.014);
lid.add(screen);
laptopHit.push(screen);
for (const mesh of laptopHit) clickable(mesh, 'crt', 'Laptop · sit down');

// A sticky note on the corner of the screen. Clicking it peels it off the one underneath.
const NOTES = [
    { lines: ['Do NOT', 'update', 'before', 'the demo'], tip: 'Note · "Do NOT update before the demo"' },
    { lines: ['Updated', 'before', 'the demo.'], tip: 'Note · "Updated before the demo."' },
];
const noteTexture = (note) => canvasTexture(200, 200, (g, w, h) => {
    g.fillStyle = '#f7dc5a'; g.fillRect(0, 0, w, h);
    g.fillStyle = '#e9c93e'; g.fillRect(0, 0, w, 26);
    g.fillStyle = '#2a2a33'; g.textAlign = 'center';
    g.font = 'italic 700 38px "Comic Sans MS", "Marker Felt", cursive';
    const top = 118 - note.lines.length * 21;
    note.lines.forEach((line, i) => g.fillText(line, w / 2, top + i * 42));
});
let noteIndex = 0;
const note = new THREE.Mesh(
    new THREE.PlaneGeometry(0.15, 0.15),
    new THREE.MeshStandardMaterial({ roughness: 0.95, map: noteTexture(NOTES[0]) }));
note.position.set(0.39, 0.1, 0.018);
note.rotation.z = -0.09;
lid.add(note);
clickable(note, 'note', NOTES[0].tip);

function nextNote(event) {
    noteIndex = (noteIndex + 1) % NOTES.length;
    note.material.map.dispose();
    note.material.map = noteTexture(NOTES[noteIndex]);
    note.rotation.z = noteIndex ? 0.07 : -0.09;
    note.userData.label = NOTES[noteIndex].tip;
    say(NOTES[noteIndex].lines.join(' '), event);
}

// Where the screen sits in the room, and which way it faces, for the zoom.
lid.updateMatrixWorld(true);
screen.getWorldPosition(SCREEN.center);
SCREEN.normal.set(0, 0, 1).transformDirection(screen.matrixWorld);

// The screen lights the desk in front of it.
const glow = new THREE.PointLight(0x9cc6fb, 0.9, 2.2);
glow.position.set(0.7, 1.35, -1.4);
scene.add(glow);

// The phone, propped on a stand: each app on it is one of the Avalonia projects.
const phone = new THREE.Group();
phone.rotation.order = 'YXZ';
phone.position.set(1.6, 1.0, -1.55);
phone.rotation.set(-0.28, -0.3, 0);
scene.add(phone);
// A rounded rectangle centred on the origin, for the phone's body and screen.
function roundedRect(w, h, r) {
    const shape = new THREE.Shape();
    const x = -w / 2, y = -h / 2;
    shape.moveTo(x + r, y);
    shape.lineTo(x + w - r, y);
    shape.quadraticCurveTo(x + w, y, x + w, y + r);
    shape.lineTo(x + w, y + h - r);
    shape.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
    shape.lineTo(x + r, y + h);
    shape.quadraticCurveTo(x, y + h, x, y + h - r);
    shape.lineTo(x, y + r);
    shape.quadraticCurveTo(x, y, x + r, y);
    return shape;
}

const PHONE = { w: 0.156, h: 0.336, r: 0.02, fit: 0.86, center: new THREE.Vector3(), normal: new THREE.Vector3() };

const phoneBody = new THREE.Mesh(
    new THREE.ExtrudeGeometry(roundedRect(0.17, 0.35, 0.027), { depth: 0.014, bevelEnabled: false, curveSegments: 10 }),
    new THREE.MeshStandardMaterial({ color: 0x1e1e2a, roughness: 0.5 }));
phoneBody.position.set(0, 0.19, -0.007);
phoneBody.castShadow = true;
phone.add(phoneBody);
const phoneHit = [phoneBody];
box(0.1, 0.03, 0.12, 0x2e3158, 0, 0.012, -0.03, phone).rotation.x = 0.28;
box(0.02, 0.16, 0.02, 0x2e3158, 0, 0.09, -0.05, phone).rotation.x = 0.5;

// ShapeGeometry leaves UVs in model units; stretch them over the whole screen.
const phoneGlass = new THREE.ShapeGeometry(roundedRect(PHONE.w, PHONE.h, PHONE.r), 10);
const glassAt = phoneGlass.attributes.position;
for (let i = 0; i < glassAt.count; i++)
    phoneGlass.attributes.uv.setXY(i, glassAt.getX(i) / PHONE.w + 0.5, glassAt.getY(i) / PHONE.h + 0.5);

const phoneScreen = new THREE.Mesh(
    phoneGlass,
    new THREE.MeshBasicMaterial({
        toneMapped: false,
        map: canvasTexture(234, 504, (g, w, h) => {
            const sky = g.createLinearGradient(0, 0, w, h);
            sky.addColorStop(0, '#2b1f5c'); sky.addColorStop(1, '#0f2a4a');
            g.fillStyle = sky; g.fillRect(0, 0, w, h);
            const tiles = ['#7b4fd6', '#1fb5a0', '#e5484d', '#3498db', '#f2a45c'];
            tiles.forEach((color, i) => {
                g.fillStyle = color;
                g.beginPath();
                g.roundRect(22 + (i % 3) * 70, 60 + Math.floor(i / 3) * 92, 50, 50, 13);
                g.fill();
            });
            g.fillStyle = 'rgba(255,255,255,0.85)';
            g.beginPath(); g.roundRect(w / 2 - 40, h - 16, 80, 5, 3); g.fill();
        }),
    }));
phoneScreen.position.set(0, 0.19, 0.008);
phone.add(phoneScreen);
phoneHit.push(phoneScreen);
for (const mesh of phoneHit) clickable(mesh, 'phone', 'Phone · pick it up');

phone.updateMatrixWorld(true);
phoneScreen.getWorldPosition(PHONE.center);
PHONE.normal.set(0, 0, 1).transformDirection(phoneScreen.matrixWorld);

onClick('note', (event) => { sfx('paper'); nextNote(event); });

export { screen, SCREEN, PHONE };
