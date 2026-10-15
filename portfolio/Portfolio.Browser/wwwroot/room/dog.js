// The bulldog asleep under the desk.

import * as THREE from 'three';
import { scene, animated, box, clickable, puff, state, onClick, sfx } from './stage.js';
import { say } from './tips.js';

// An English bulldog, asleep under the desk, built from soft shapes.
const dog = new THREE.Group();
dog.position.set(0.25, 0.1, -1.5);
dog.rotation.y = -1.25;
dog.scale.setScalar(1.08);
scene.add(dog);

// His bed: a thick grey mattress, with a seam around the edge.
const mattress = box(1.2, 0.11, 0.86, 0x8f9299, 0.2, 0.055, -1.5);
mattress.rotation.y = 0.12;
mattress.material.roughness = 1;
const seam = box(1.215, 0.014, 0.875, 0x74777e, 0.2, 0.06, -1.5);
seam.rotation.y = 0.12;
const dogParts = [];

// A sphere stretched into the part it stands for.
function blob(radius, color, x, y, z, sx = 1, sy = 1, sz = 1) {
    const mesh = new THREE.Mesh(
        new THREE.SphereGeometry(radius, 20, 14),
        new THREE.MeshStandardMaterial({ color, roughness: 0.72 }));
    mesh.position.set(x, y, z);
    mesh.scale.set(sx, sy, sz);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    dog.add(mesh);
    dogParts.push(mesh);
    return mesh;
}

// A cartoon bulldog, all in one warm tan like a clay figure. What keeps him from reading as a
// bear or a cat: a broad square head; ear flaps folded forward over its corners, never upright;
// a muzzle as wide as the head, with jowls that sag past the jaw; a lower jaw that juts out
// under two teeth; heavy brows; and wide shoulders on thick, splayed front legs.
const TAN = 0xd9b085, TAN_DARK = 0xa67c52, MUZZLE = 0xe7cba6, NOSE = 0x1c1a1c;

const dogBody = blob(0.24, TAN, -0.22, 0.2, 0, 1.3, 0.85, 1.12);     // the dome of his back
blob(0.2, TAN, 0.04, 0.19, 0, 1.0, 0.9, 1.45);                       // shoulders, wider than the rump
blob(0.035, TAN, -0.53, 0.28, 0);                                    // a nub of a tail
blob(0.09, TAN, -0.38, 0.09, -0.21, 1.4, 0.9, 0.9);                  // back legs, tucked under
blob(0.09, TAN, -0.38, 0.09, 0.21, 1.4, 0.9, 0.9);
blob(0.05, TAN, -0.5, 0.05, 0.23, 1.3, 0.8, 1.0);                    // one hind foot, showing
blob(0.035, TAN_DARK, 0.12, 0.345, 0, 0.5, 0.3, 3.4);                // a fold of skin behind the head

blob(0.19, TAN, 0.3, 0.19, 0, 0.95, 0.88, 1.42);                     // the head: broad and square
blob(0.19, 0xf4efe6, 0.305, 0.195, 0, 0.95, 0.9, 0.36);                  // a white blaze, from brow to crown
blob(0.13, MUZZLE, 0.42, 0.118, 0, 0.8, 0.72, 1.75);                 // the muzzle, as wide as the head
blob(0.1, MUZZLE, 0.44, 0.082, -0.135, 0.85, 1.0, 0.95);             // jowls, sagging past the jaw
blob(0.1, MUZZLE, 0.44, 0.082, 0.135, 0.85, 1.0, 0.95);
blob(0.062, MUZZLE, 0.515, 0.046, 0, 0.75, 0.45, 1.9);               // the lower jaw, jutting out
blob(0.045, 0x7a2e2e, 0.512, 0.09, 0, 0.35, 0.36, 1.7);              // the open mouth
blob(0.05, NOSE, 0.5, 0.207, 0, 0.66, 0.62, 1.25);                   // a big black nose
blob(0.03, TAN_DARK, 0.468, 0.255, 0, 0.5, 0.32, 2.3);               // the roll over it
blob(0.07, TAN, 0.4, 0.31, -0.105, 0.9, 0.5, 1.3);                   // heavy brows
blob(0.07, TAN, 0.4, 0.31, 0.105, 0.9, 0.5, 1.3);
blob(0.03, TAN_DARK, 0.435, 0.285, 0, 0.6, 0.6, 0.8);                // the furrow between them

// One ear flap: a rounded triangle with some thickness, hanging from its wide edge.
const earShape = new THREE.Shape();
earShape.moveTo(-0.075, 0);
earShape.quadraticCurveTo(0, 0.022, 0.075, 0);
earShape.quadraticCurveTo(0.07, -0.085, 0.012, -0.135);
earShape.quadraticCurveTo(0, -0.145, -0.012, -0.135);
earShape.quadraticCurveTo(-0.07, -0.085, -0.075, 0);
const earGeometry = new THREE.ExtrudeGeometry(earShape, { depth: 0.014, bevelEnabled: true, bevelSize: 0.009, bevelThickness: 0.009, bevelSegments: 3 });
earGeometry.translate(0, 0, -0.007);

const dogEars = [-1, 1].map((side) => {
    const ear = new THREE.Mesh(earGeometry, new THREE.MeshStandardMaterial({ color: TAN_DARK, roughness: 0.72 }));
    // Rooted on the corner of the skull, folded forward and a little outward, tip towards the eye.
    ear.position.set(0.25, 0.325, side * 0.185);
    ear.rotation.set(-side * 1.0, 0, 0.95);
    ear.castShadow = true;
    dog.add(ear);
    dogParts.push(ear);
    return ear;
});

blob(0.075, TAN, 0.3, 0.065, -0.3, 2.0, 0.85, 1.0);                  // front legs: thick, wide apart
blob(0.075, TAN, 0.3, 0.065, 0.3, 2.0, 0.85, 1.0);
blob(0.07, TAN, 0.46, 0.055, -0.305, 1.2, 0.8, 1.15);                // big paws
blob(0.07, TAN, 0.46, 0.055, 0.305, 1.2, 0.8, 1.15);

const dogEyes = [];
const dogOpenEyes = [];
for (const side of [-1, 1]) {
    // A closed eye: a thin dark curve, like a lash.
    const eye = new THREE.Mesh(
        new THREE.TorusGeometry(0.036, 0.0055, 6, 16, Math.PI),
        new THREE.MeshStandardMaterial({ color: NOSE, roughness: 0.5 }));
    eye.position.set(0.462, 0.252, side * 0.11);
    eye.rotation.set(Math.PI, Math.PI / 2 - side * 0.38, 0);
    dog.add(eye);
    dogEyes.push(eye);

    // The same eye, open: shown only when something wakes him.
    const open = new THREE.Group();
    open.position.copy(eye.position);
    open.rotation.y = -side * 0.38;
    open.visible = false;
    const white = new THREE.Mesh(new THREE.SphereGeometry(0.034, 16, 12), new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.3 }));
    white.scale.set(0.5, 1, 1);
    const pupil = new THREE.Mesh(new THREE.SphereGeometry(0.019, 12, 10), new THREE.MeshStandardMaterial({ color: NOSE, roughness: 0.3 }));
    pupil.position.set(0.012, 0.004, 0);
    pupil.scale.set(0.5, 1, 1);
    open.add(white, pupil);
    dog.add(open);
    dogOpenEyes.push(open);

    // Two lower teeth, the crease from nose to jowl, whisker dots, and toenails.
    const tooth = new THREE.Mesh(new THREE.ConeGeometry(0.012, 0.03, 8), new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.3 }));
    tooth.position.set(0.535, 0.082, side * 0.055);
    dog.add(tooth);
    blob(0.02, TAN_DARK, 0.515, 0.15, side * 0.07, 0.4, 1.5, 0.35).rotation.x = side * 0.5;
    for (const [dx, dy, dz] of [[0, 0.03, 0.02], [0.012, 0, 0], [0, -0.028, 0.025], [-0.018, 0.012, 0.045]])
        blob(0.006, TAN_DARK, 0.513 + dx, 0.115 + dy, side * (0.125 + dz));
    for (const dz of [-0.04, 0, 0.04])
        blob(0.01, NOSE, 0.54, 0.045, side * 0.305 + dz, 1.2, 1, 1);
}
for (const mesh of dogParts) clickable(mesh, 'dog', 'Bulldog · asleep');

let dogAwake = false;

// Awake, he opens his eyes, lifts an ear and stops snoring. It never lasts.
function wakeDog(awake) {
    if (dogAwake === awake) return;
    dogAwake = awake;
    dogEyes.forEach((eye) => { eye.visible = !awake; });
    dogOpenEyes.forEach((eye) => { eye.visible = awake; });
    for (const mesh of dogParts) mesh.userData.label = awake ? 'Bulldog · wide awake' : 'Bulldog · asleep';
    if (awake) sfx('snort');
    window.pixsound?.snore(!awake && state.mode !== 'crt');
}

const DOG_LINES = ['Zzz…', '*snores*', '*one eye opens, then closes*', '*dreams of sausages*', '*sighs heavily*'];
const zs = [0, 1, 2].map(() => puff('z', 'rgba(90,90,106,0.9)'));
dog.updateMatrixWorld(true);
const dogHead = dog.localToWorld(new THREE.Vector3(0.3, 0.44, 0));
animated.push((t) => {
    // Slow breathing, an ear that twitches now and then, and a trail of Zs.
    const breath = Math.sin(t * 1.5);
    dogBody.scale.y = 0.85 * (1 + breath * 0.045);
    dogBody.scale.z = 1.12 * (1 + breath * 0.02);
    const twitch = Math.max(0, Math.sin(t * 0.9)) ** 24;
    dogEars[1].rotation.x = -1.0 + (dogAwake ? 0.22 : twitch * 0.35);
    dogEars[0].rotation.x = 1.0 - (dogAwake ? 0.22 : 0);
    zs.forEach((z, i) => {
        const life = (t * 0.22 + i / zs.length) % 1;
        z.position.set(dogHead.x + life * 0.12, dogHead.y + life * 0.4, dogHead.z + 0.1);
        z.scale.setScalar(0.05 + life * 0.07);
        z.material.opacity = dogAwake ? 0 : Math.sin(life * Math.PI);
    });
});

function isDogAwake() {
    return dogAwake;
}

onClick('dog', (event) => {
    if (dogAwake) {
        say('*stares at the drawer*', event);
        return;
    }
    sfx('snort');
    say(DOG_LINES[Math.floor(Math.random() * DOG_LINES.length)], event);
});

export { wakeDog, isDogAwake };
