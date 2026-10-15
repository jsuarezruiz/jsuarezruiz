// dotnet-bot, the .NET mascot: a small figure on the MAUI shelf, next to the MVP award.

import * as THREE from 'three';
import { scene, animated, clickable, canvasTexture, onClick, sfx } from './stage.js';
import { say } from './tips.js';

// Built at about a metre tall and scaled down, like the monkey.
const bot = new THREE.Group();
bot.position.set(-1.86, 1.125, -2.26);
bot.rotation.y = 0.35;
bot.scale.setScalar(0.24);
scene.add(bot);

const PURPLE = 0xa77fd8, DARK = 0x512bd4, WHITE = 0xf1ebfa, EYE = 0x2e1660;
const parts = [];

function part(geometry, color, x, y, z, parent = bot) {
    const mesh = new THREE.Mesh(geometry, new THREE.MeshStandardMaterial({ color, roughness: 0.45 }));
    mesh.position.set(x, y, z);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    parent.add(mesh);
    parts.push(mesh);
    return mesh;
}

function blob(radius, color, x, y, z, sx = 1, sy = 1, sz = 1, parent = bot) {
    const mesh = part(new THREE.SphereGeometry(radius, 24, 16), color, x, y, z, parent);
    mesh.scale.set(sx, sy, sz);
    return mesh;
}

function rod(radius, length, color, y, parent) {
    return part(new THREE.CylinderGeometry(radius, radius, length, 16), color, 0, y, 0, parent);
}

// Head and body are one ball.
const BODY = { r: 0.32, y: 0.62 };
blob(BODY.r, PURPLE, 0, BODY.y, 0);

// The white visor across the face: a slice of a slightly bigger sphere, round the front only.
part(new THREE.SphereGeometry(BODY.r + 0.004, 32, 12, Math.PI / 2 - 1.0, 2.0, 0.92, 0.55), WHITE, 0, BODY.y, 0);
for (const side of [-1, 1]) {
    const eye = blob(0.058, EYE, side * 0.105, BODY.y + 0.12, 0.268, 0.72, 1, 0.4);
    eye.rotation.z = side * 0.35;                                  // slanted, for the determined look
}
const chin = part(new THREE.ConeGeometry(0.05, 0.09, 12), DARK, 0, BODY.y + 0.02, 0.3);
chin.rotation.x = Math.PI;                                         // the dark point under the eyes
chin.scale.z = 0.5;

// The belt, with its .NET badge.
const belt = part(new THREE.TorusGeometry(0.288, 0.03, 10, 40), DARK, 0, BODY.y - 0.14, 0);
belt.rotation.x = Math.PI / 2;
const badge = new THREE.Mesh(new THREE.PlaneGeometry(0.09, 0.065), new THREE.MeshStandardMaterial({
    roughness: 0.5,
    map: canvasTexture(90, 64, (g, w, h) => {
        g.fillStyle = '#512bd4'; g.fillRect(0, 0, w, h);
        g.strokeStyle = '#f1ebfa'; g.lineWidth = 3; g.strokeRect(2, 2, w - 4, h - 4);
        g.fillStyle = '#ffffff'; g.font = '700 26px system-ui, sans-serif'; g.textAlign = 'center'; g.textBaseline = 'middle';
        g.fillText('.NET', w / 2, h / 2 + 1);
    }),
}));
badge.position.set(0, BODY.y - 0.14, 0.32);
bot.add(badge);
parts.push(badge);

// The antenna.
part(new THREE.CylinderGeometry(0.012, 0.012, 0.12, 8), WHITE, 0, BODY.y + BODY.r + 0.05, 0);
const tip = blob(0.035, PURPLE, 0, BODY.y + BODY.r + 0.12, 0);

// Arms: white joints between purple segments, ending in a small three-fingered hand.
for (const side of [-1, 1]) {
    const arm = new THREE.Group();
    arm.position.set(side * 0.3, BODY.y - 0.1, 0.03);
    arm.rotation.set(-0.25, 0, side * 0.55);
    bot.add(arm);
    rod(0.04, 0.03, WHITE, -0.02, arm);
    rod(0.034, 0.1, PURPLE, -0.08, arm);
    rod(0.036, 0.025, WHITE, -0.145, arm);
    rod(0.03, 0.08, PURPLE, -0.2, arm);
    blob(0.05, WHITE, 0, -0.27, 0, 1, 0.7, 0.6, arm);
    for (const x of [-0.03, 0, 0.03]) blob(0.018, WHITE, x, -0.315, 0, 1, 1.3, 1, arm);
}

// Legs: short, wide apart, in chunky boots with white toe caps.
for (const side of [-1, 1]) {
    const leg = new THREE.Group();
    leg.position.set(side * 0.14, 0.33, 0);
    leg.rotation.z = side * 0.12;
    bot.add(leg);
    rod(0.045, 0.05, WHITE, -0.03, leg);
    rod(0.05, 0.12, PURPLE, -0.11, leg);
    rod(0.052, 0.03, WHITE, -0.18, leg);
    blob(0.1, PURPLE, side * 0.17, 0.05, 0.05, 0.9, 0.55, 1.4);
    blob(0.07, WHITE, side * 0.17, 0.04, 0.15, 1.1, 0.6, 0.9);
}

for (const mesh of parts) clickable(mesh, 'bot', 'dotnet-bot');

// Clicked, it beeps, spins round once and says something.
const BOT_LINES = ['Hello, .NET!', 'Beep boop. Build succeeded.', '0 warnings, 0 errors.', 'dotnet run!', 'I compile, therefore I am.'];
let spinAt = -10;
let now = 0;
animated.push((t) => {
    now = t;
    const k = Math.min(1, Math.max(0, (t - spinAt) / 0.8));
    bot.rotation.y = 0.35 + (k < 1 ? (1 - Math.cos(k * Math.PI)) * Math.PI : 0);
    // The antenna ball glows for a moment after a click.
    tip.material.emissive.setHex(t - spinAt < 1.2 ? 0x7a4fe0 : 0x000000);
});

onClick('bot', (event) => {
    sfx('beep');
    spinAt = now;
    say(BOT_LINES[Math.floor(Math.random() * BOT_LINES.length)], event);
});
