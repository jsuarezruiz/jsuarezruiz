// The Avalonia logo in 3D, standing on the Avalonia shelf.

import * as THREE from 'three';
import { scene, animated, box, clickable, onClick, sfx } from './stage.js';
import { say } from './tips.js';

// The logo, measured from Assets/avalonia-logo.ico, in units of its half-width: a circle whose
// bottom-right quarter is a rounded square corner, with an "a" cut through it. The "a" is the same
// idea smaller: a circle with a square bottom-right corner, a solid disc in its middle, and a small
// dot on its left, held in a thin ring.
const LOGO = { corner: 0.2, cut: 0.659, disc: 0.314, dot: { x: -0.683, ring: 0.151, hole: 0.103 }, stretch: 1.048 };

const outline = new THREE.Shape();
outline.absarc(0, 0, 1, 0, Math.PI * 1.5, false);
outline.lineTo(1 - LOGO.corner, -1);
outline.absarc(1 - LOGO.corner, -1 + LOGO.corner, LOGO.corner, -Math.PI / 2, 0, false);
outline.lineTo(1, 0);

// The cut: the "a" outline, joined with the dot's ring where the two circles overlap.
const { x: dx, ring } = LOGO.dot;
const meetX = (dx * dx + LOGO.cut * LOGO.cut - ring * ring) / (2 * dx);
const meetY = Math.sqrt(LOGO.cut * LOGO.cut - meetX * meetX);
const onCut = Math.atan2(meetY, meetX);
const onDot = Math.atan2(meetY, meetX - dx);
const cut = new THREE.Path();
cut.absarc(0, 0, LOGO.cut, 0, onCut, false);
cut.absarc(dx, 0, ring, onDot, Math.PI * 2 - onDot, false);
cut.absarc(0, 0, LOGO.cut, Math.PI * 2 - onCut, Math.PI * 1.5, false);
cut.lineTo(LOGO.cut, -LOGO.cut);
cut.lineTo(LOGO.cut, 0);
outline.holes.push(cut);

const disc = new THREE.Shape();
disc.absarc(0, 0, LOGO.disc, 0, Math.PI * 2, false);

const dotRing = new THREE.Shape();
dotRing.absarc(dx, 0, ring, 0, Math.PI * 2, false);
const dotHole = new THREE.Path();
dotHole.absarc(dx, 0, LOGO.dot.hole, 0, Math.PI * 2, true);
dotRing.holes.push(dotHole);

const logoGeometry = new THREE.ExtrudeGeometry([outline, disc, dotRing], {
    depth: 0.22, curveSegments: 48, bevelEnabled: true, bevelSize: 0.025, bevelThickness: 0.025, bevelSegments: 3,
});
logoGeometry.translate(0, 0, -0.11);

// Standing on a small dark base, turned a little towards the room.
const stand = new THREE.Group();
stand.position.set(-1.75, 1.825, -2.27);
stand.rotation.y = 0.2;
scene.add(stand);
const base = box(0.2, 0.03, 0.08, 0x1e1e2a, 0, 0.015, 0, stand);

const logo = new THREE.Mesh(logoGeometry, new THREE.MeshStandardMaterial({ color: 0x0d6efd, roughness: 0.3, metalness: 0.1 }));
logo.scale.set(0.11, 0.11 * LOGO.stretch, 0.11);
logo.position.y = 0.03 + 0.11 * LOGO.stretch + 0.003;
logo.castShadow = true;
stand.add(logo);

clickable(logo, 'avalonia', 'Avalonia UI');
clickable(base, 'avalonia', 'Avalonia UI');

// Clicked, it spins round once.
const LINES = ['Avalonia UI · where I work now', 'Same pixels on every platform.', 'Windows, macOS, Linux, iOS, Android and the browser.', 'The laptop runs Avalonia in your browser. Try it!'];
let spinAt = -10;
let now = 0;
animated.push((t) => {
    now = t;
    const k = Math.min(1, Math.max(0, (t - spinAt) / 0.9));
    logo.rotation.y = k < 1 ? (1 - Math.cos(k * Math.PI)) * Math.PI : 0;
});

onClick('avalonia', (event) => {
    sfx('chime');
    spinAt = now;
    say(LINES[Math.floor(Math.random() * LINES.length)], event);
});
