// The stage everything else is built on: the renderer, the scene, its light, and the helpers
// the room's objects are made with.

import * as THREE from 'three';
import { RoundedBoxGeometry } from 'three/addons/geometries/RoundedBoxGeometry.js';

const canvas = document.getElementById('room');
const out = document.getElementById('out');
const tip = document.getElementById('tip');
const card = document.getElementById('card');
const hint = document.getElementById('hint');
const status = document.getElementById('status');
const phoneApp = document.getElementById('phoneapp');

const renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.4;

const scene = new THREE.Scene();
scene.background = new THREE.Color(0xd8d2c4);
const camera = new THREE.PerspectiveCamera(45, 1, 0.1, 50);

// The window sky, and with it the mood of the lighting, follows the visitor's clock.
const hour = new Date().getHours();
const byClock = hour >= 8 && hour < 18 ? 'day' : hour >= 18 && hour < 21 ? 'dusk' : 'night';
// ?time=day|dusk|night overrides the clock, to preview each mood.
const asked = new URLSearchParams(location.search).get('time');
const time = ['day', 'dusk', 'night'].includes(asked) ? asked : byClock;
const MOOD = {
    day: { sky: 0x9fd0ff, sun: 0xfff4dc, sunPower: 2.8, fill: 1.7 },
    dusk: { sky: 0xf2a45c, sun: 0xffb070, sunPower: 1.8, fill: 0.9 },
    night: { sky: 0x1b2450, sun: 0x8aa0ff, sunPower: 0.5, fill: 0.35 },
}[time];

scene.add(new THREE.HemisphereLight(0xfff3e2, 0x5a4a44, MOOD.fill));

// Light comes in through the window on the right.
const sun = new THREE.DirectionalLight(MOOD.sun, MOOD.sunPower);
sun.position.set(4.5, 4.2, 2.5);
sun.target.position.set(0, 1, -1.8);
sun.castShadow = true;
sun.shadow.mapSize.set(2048, 2048);
sun.shadow.camera.left = -5;
sun.shadow.camera.right = 5;
sun.shadow.camera.top = 5;
sun.shadow.camera.bottom = -3;
sun.shadow.bias = -0.0005;
sun.shadow.radius = 4;
scene.add(sun, sun.target);

const interactive = [];

// Things that move a little each frame register here; the loop calls them with the time in seconds.
const animated = [];

// Furniture has slightly rounded edges, which catch the light; pass round = false for a hard box.
function box(w, h, d, color, x, y, z, parent = scene, round = true) {
    const material = new THREE.MeshStandardMaterial({ color, roughness: 0.85 });
    const smallest = Math.min(w, h, d);
    const geometry = round && smallest >= 0.02
        ? new RoundedBoxGeometry(w, h, d, 3, Math.min(0.022, smallest * 0.3))
        : new THREE.BoxGeometry(w, h, d);
    const mesh = new THREE.Mesh(geometry, material);
    mesh.position.set(x, y, z);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    parent.add(mesh);
    return mesh;
}

function clickable(mesh, id, label, data) {
    mesh.userData = { id, label, data };
    interactive.push(mesh);
    return mesh;
}

function canvasTexture(width, height, draw, pixelated = false) {
    const c = document.createElement('canvas');
    c.width = width;
    c.height = height;
    draw(c.getContext('2d'), width, height);
    const texture = new THREE.CanvasTexture(c);
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = renderer.capabilities.getMaxAnisotropy();
    if (pixelated) {
        texture.magFilter = THREE.NearestFilter;
        texture.minFilter = THREE.NearestFilter;
    }
    return texture;
}

// A soft, round-ish blob for steam and the dog's Zs.
function puff(text, color) {
    const sprite = new THREE.Sprite(new THREE.SpriteMaterial({
        transparent: true,
        depthWrite: false,
        map: canvasTexture(64, 64, (g) => {
            g.fillStyle = color;
            if (text) {
                g.font = '700 52px system-ui, sans-serif';
                g.textAlign = 'center';
                g.textBaseline = 'middle';
                g.fillText(text, 32, 34);
            } else {
                g.beginPath(); g.arc(32, 32, 22, 0, Math.PI * 2); g.fill();
            }
        }),
    }));
    scene.add(sprite);
    return sprite;
}

// What the visitor is doing: room | intro | moving | crt | phone | paper | card.
const state = { mode: 'room' };

// What a click does, by the id its object was registered with.
const handlers = {};
function onClick(id, handler) {
    handlers[id] = handler;
}

function click(mesh, event) {
    handlers[mesh.userData.id]?.(event, mesh.userData.data);
}

function sfx(name) {
    window.pixsound?.play(name);
}

export { canvas, tip, camera, interactive, state, time, canvasTexture, scene, box, clickable, onClick, sfx, renderer, MOOD, animated, puff, status, phoneApp, hint, out, card, click };
