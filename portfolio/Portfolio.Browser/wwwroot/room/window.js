// The window: the skyline behind the glass, a pigeon, the blind, and the light coming in.

import * as THREE from 'three';
import { renderer, scene, camera, time, MOOD, animated, box, clickable, canvasTexture, onClick } from './stage.js';
import { sevillaTexture } from './skyline.js';

// Window: a wide one, looking out over Sevilla. The sky follows the visitor's clock.
const WIN = { x: 2.72, y: 2.05, w: 1.96, h: 1.32 };
box(WIN.w + 0.2, WIN.h + 0.2, 0.06, 0xffffff, WIN.x, WIN.y, -2.46);

const paneTexture = sevillaTexture();
// A little of the view is held back at each side, so it can slide as the visitor's eye moves.
paneTexture.repeat.set(0.9, 1);
paneTexture.offset.set(0.05, 0);
const pane = new THREE.Mesh(new THREE.PlaneGeometry(WIN.w, WIN.h), new THREE.MeshBasicMaterial({ toneMapped: false, map: paneTexture }));
pane.position.set(WIN.x, WIN.y, -2.42);
scene.add(pane);
animated.push(() => {
    // Things far away seem to move with you: the city shifts against the window frame.
    paneTexture.offset.x = THREE.MathUtils.clamp(0.05 + (camera.position.x - WIN.x) * 0.012, 0, 0.1);
});
box(0.05, WIN.h, 0.03, 0xffffff, WIN.x, WIN.y, -2.41);
box(WIN.w, 0.05, 0.03, 0xffffff, WIN.x, WIN.y + 0.12, -2.41);
box(WIN.w + 0.3, 0.05, 0.14, 0xffffff, WIN.x, WIN.y - WIN.h / 2 - 0.1, -2.4);

// Click the window and a pigeon hops up outside the glass, looks in for a moment, and flies off.
renderer.localClippingEnabled = true;
const PANE_BOTTOM = WIN.y - WIN.h / 2;
// One drawing per pose: standing, and the two halves of a wingbeat.
const pigeonPose = (wing) => canvasTexture(170, 170, (g) => {
    if (wing === 'folded') { g.fillStyle = '#e58a9a'; g.fillRect(74, 138, 5, 30); g.fillRect(92, 138, 5, 30); } // legs, tucked up in flight
    g.fillStyle = '#4a515b'; g.beginPath(); g.moveTo(34, 104); g.lineTo(2, 128); g.lineTo(40, 126); g.fill();   // tail
    if (wing === 'up') { g.fillStyle = '#5c6570'; g.beginPath(); g.moveTo(52, 100); g.lineTo(70, 22); g.lineTo(108, 96); g.fill(); }
    g.fillStyle = '#7d8794'; g.beginPath(); g.ellipse(80, 108, 52, 36, -0.15, 0, Math.PI * 2); g.fill();        // body
    g.fillStyle = '#5c6570';
    if (wing === 'folded') { g.beginPath(); g.ellipse(64, 112, 34, 20, -0.1, 0, Math.PI * 2); g.fill(); }
    if (wing === 'down') { g.beginPath(); g.moveTo(48, 104); g.lineTo(66, 168); g.lineTo(108, 110); g.fill(); }
    g.fillStyle = '#5e8f7c'; g.beginPath(); g.ellipse(116, 76, 17, 22, 0.3, 0, Math.PI * 2); g.fill();          // the shiny neck
    g.fillStyle = '#7d8794'; g.beginPath(); g.arc(124, 50, 21, 0, Math.PI * 2); g.fill();                      // head
    g.fillStyle = '#e8a23c'; g.beginPath(); g.moveTo(142, 46); g.lineTo(166, 54); g.lineTo(142, 60); g.fill();  // beak
    g.fillStyle = '#f2a12e'; g.beginPath(); g.arc(130, 46, 6.5, 0, Math.PI * 2); g.fill();
    g.fillStyle = '#14151a'; g.beginPath(); g.arc(131, 46, 3.2, 0, Math.PI * 2); g.fill();                     // a round, unblinking eye
});
const PIGEON = { folded: pigeonPose('folded'), up: pigeonPose('up'), down: pigeonPose('down') };
const pigeon = new THREE.Mesh(
    new THREE.PlaneGeometry(0.34, 0.34),
    new THREE.MeshBasicMaterial({
        transparent: true,
        toneMapped: false,
        map: PIGEON.folded,
        // Cut off at the edges of the glass: it rises from behind the sill and flies out of frame.
        clippingPlanes: [
            new THREE.Plane(new THREE.Vector3(0, 1, 0), -PANE_BOTTOM),
            new THREE.Plane(new THREE.Vector3(0, -1, 0), WIN.y + WIN.h / 2),
            new THREE.Plane(new THREE.Vector3(1, 0, 0), -(WIN.x - WIN.w / 2)),
            new THREE.Plane(new THREE.Vector3(-1, 0, 0), WIN.x + WIN.w / 2),
        ],
    }));
pigeon.visible = false;
scene.add(pigeon);
clickable(pane, 'window', 'Window · Sevilla');

let pigeonSince = -1;   // when it was called, in the loop's seconds
let pigeonCalled = false;
let pigeonPerch = 0;
function callPigeon() {
    if (pigeon.visible || pigeonCalled) return;
    pigeonCalled = true;
    // On the left half, which stays in view even on a narrow screen.
    pigeonPerch = WIN.x - 0.15 - Math.random() * 0.6;
}
animated.push((t) => {
    if (pigeonCalled) { pigeonCalled = false; pigeonSince = t; pigeon.visible = true; }
    if (!pigeon.visible) return;
    const age = t - pigeonSince;
    const UP = 0.45, STAY = 4.2, AWAY = 2.2;
    const perchY = PANE_BOTTOM + 0.15;
    if (age < UP + STAY) {
        // Hops up, then stands: quick pecks of the head, and a look the other way now and then.
        const shown = age < UP ? 1 - (1 - age / UP) ** 2 : 1;
        const bob = age > UP ? Math.max(0, Math.sin(age * 9)) ** 6 * 0.012 : 0;
        pigeon.material.map = PIGEON.folded;
        pigeon.position.set(pigeonPerch, perchY - 0.34 + shown * 0.34 - bob, -2.416);
        pigeon.scale.set(Math.floor(age / 1.3) % 2 === 0 ? 1 : -1, 1, 1);
        return;
    }

    // Takes off to the right, across the whole window, flapping, up and away over the city.
    const k = (age - UP - STAY) / AWAY;
    const facing = 1;
    const size = 1 - k * 0.6;
    pigeon.material.map = Math.floor(age * 11) % 2 === 0 ? PIGEON.up : PIGEON.down;
    pigeon.position.set(pigeonPerch + facing * k * 1.9, perchY + k * k * 0.95 + Math.sin(k * 14) * 0.012, -2.416);
    pigeon.scale.set(facing * size, size, 1);
    if (k >= 1) pigeon.visible = false;
});

// A roller blind, almost fully up so the skyline shows.
const blindTop = WIN.y + WIN.h / 2 + 0.11;
box(WIN.w + 0.12, 0.09, 0.09, 0xf0ece2, WIN.x, blindTop + 0.04, -2.38);                    // the roll, in its cassette
box(WIN.w, 0.13, 0.012, 0xe6dcc8, WIN.x, blindTop - 0.07, -2.385, scene, false);           // the fabric
box(WIN.w, 0.03, 0.03, 0xcfc4ac, WIN.x, blindTop - 0.14, -2.38);                           // the weighted bar
box(0.008, 0.55, 0.008, 0xcfc4ac, WIN.x + WIN.w / 2 - 0.08, blindTop - 0.3, -2.37, scene, false); // the cord

// Light falling in through the window, and the dust drifting in it.
// The beam lands on the floor to the right of the desk, clear of the dog.
const SHAFT = { from: new THREE.Vector3(WIN.x, WIN.y, -2.38), to: new THREE.Vector3(2.3, 0.02, 0.5) };
{
    const [hw, hh] = [WIN.w / 2, WIN.h / 2];
    const a = SHAFT.from, b = SHAFT.to;
    // The four corners at the window, then the same four where the light meets the floor.
    const corners = [
        [a.x - hw, a.y + hh, a.z], [a.x + hw, a.y + hh, a.z], [a.x + hw, a.y - hh, a.z], [a.x - hw, a.y - hh, a.z],
        [b.x - hw * 1.5, b.y, b.z - 1.1], [b.x + hw * 1.5, b.y, b.z - 1.1], [b.x + hw * 1.5, b.y, b.z + 0.3], [b.x - hw * 1.5, b.y, b.z + 0.3],
    ];
    const sides = [[0, 1, 5, 4], [1, 2, 6, 5], [2, 3, 7, 6], [3, 0, 4, 7]];
    const positions = [];
    for (const [p, q, r, s] of sides)
        for (const i of [p, q, r, p, r, s]) positions.push(...corners[i]);
    const geometry = new THREE.BufferGeometry();
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
    const shaft = new THREE.Mesh(geometry, new THREE.MeshBasicMaterial({
        color: MOOD.sun,
        transparent: true,
        opacity: time === 'night' ? 0.01 : 0.016,
        blending: THREE.AdditiveBlending,
        side: THREE.DoubleSide,
        depthWrite: false,
    }));
    scene.add(shaft);
}

const DUST = 70;
const dustAt = new Float32Array(DUST * 3);
const dustSeed = Array.from({ length: DUST }, () => ({ along: Math.random(), x: Math.random() - 0.5, y: Math.random() - 0.5, speed: 0.01 + Math.random() * 0.02 }));
const dustGeometry = new THREE.BufferGeometry();
dustGeometry.setAttribute('position', new THREE.BufferAttribute(dustAt, 3));
scene.add(new THREE.Points(dustGeometry, new THREE.PointsMaterial({
    color: MOOD.sun,
    size: 0.014,
    transparent: true,
    opacity: time === 'night' ? 0.25 : 0.7,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
})));
animated.push((t) => {
    dustSeed.forEach((mote, i) => {
        // Each mote drifts slowly down the shaft and wanders a little across it.
        const along = (mote.along + t * mote.speed) % 1;
        const spread = 0.5 + along * 0.6;
        dustAt[i * 3] = THREE.MathUtils.lerp(SHAFT.from.x, SHAFT.to.x, along) + (mote.x + Math.sin(t * 0.3 + i) * 0.05) * spread;
        dustAt[i * 3 + 1] = THREE.MathUtils.lerp(SHAFT.from.y, SHAFT.to.y, along) + mote.y * 0.5;
        dustAt[i * 3 + 2] = THREE.MathUtils.lerp(SHAFT.from.z, SHAFT.to.z, along) + Math.cos(t * 0.25 + i * 2) * 0.08;
    });
    dustGeometry.attributes.position.needsUpdate = true;
});

onClick('window', callPigeon);
