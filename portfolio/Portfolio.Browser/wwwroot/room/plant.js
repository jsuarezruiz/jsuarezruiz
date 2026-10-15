// A Venus flytrap on the Avalonia shelf. Tap it: every trap snaps shut, and it tells a joke.

import * as THREE from 'three';
import { scene, animated, clickable, onClick, sfx } from './stage.js';
import { say } from './tips.js';

const plant = new THREE.Group();
plant.position.set(-1.42, 1.825, -2.25);
scene.add(plant);

const parts = [];
function add(geometry, material, parent = plant) {
    const mesh = new THREE.Mesh(geometry, material);
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    parent.add(mesh);
    parts.push(mesh);
    return mesh;
}

const mat = (color, roughness = 0.7, side = THREE.FrontSide) => new THREE.MeshStandardMaterial({ color, roughness, side });
const POT = mat(0xc4693d, 0.9), SOIL = mat(0x3b2a1e, 1), STEM = mat(0x5b9a35);
const TRAP_OUT = mat(0x6aa83a, 0.6), TRAP_IN = mat(0xc8323c, 0.5, THREE.BackSide), CILIA = mat(0xb6d77a, 0.6);

// The pot: tapered terracotta, with a rim and a layer of soil.
add(new THREE.CylinderGeometry(0.052, 0.04, 0.07, 24), POT).position.y = 0.035;
add(new THREE.CylinderGeometry(0.058, 0.058, 0.018, 24), POT).position.y = 0.068;
add(new THREE.CylinderGeometry(0.05, 0.05, 0.004, 24), SOIL).position.y = 0.076;

// One lobe of a trap: half of a flattened ellipsoid, green outside and red inside, hinged along
// its back edge, with a fringe of teeth round the free edge.
const LOBE = { r: 0.024, sx: 1.25, sy: 0.42 };
function lobe(trap, upper) {
    const pivot = new THREE.Group();
    trap.add(pivot);
    const geometry = new THREE.SphereGeometry(LOBE.r, 18, 6, 0, Math.PI * 2, upper ? 0 : Math.PI / 2, Math.PI / 2);
    for (const material of [TRAP_OUT, TRAP_IN]) {
        const shell = add(geometry, material, pivot);
        shell.scale.set(LOBE.sx, LOBE.sy, 1);
        shell.position.z = LOBE.r;                      // the hinge is the back edge, at the pivot
    }
    const up = new THREE.Vector3(0, 1, 0);
    for (let i = 0; i <= 10; i++) {
        const phi = -Math.PI / 3 + i / 10 * (Math.PI * 5 / 3);   // round the front, not along the hinge
        const tooth = add(new THREE.ConeGeometry(0.0016, 0.013, 4), CILIA, pivot);
        tooth.position.set(Math.cos(phi) * LOBE.r * LOBE.sx, 0, LOBE.r + Math.sin(phi) * LOBE.r);
        tooth.quaternion.setFromUnitVectors(up, new THREE.Vector3(Math.cos(phi), upper ? 0.35 : -0.35, Math.sin(phi)).normalize());
        tooth.translateY(0.006);
    }
    return pivot;
}

// Five traps on curved stalks, leaning out mostly towards the room.
const traps = [20, 60, 100, 140, 185].map((deg, i) => {
    const az = THREE.MathUtils.degToRad(deg);
    const reach = 0.07 + (i % 3) * 0.012, rise = 0.05 + ((i + 1) % 3) * 0.02;
    const out = new THREE.Vector3(Math.cos(az), 0, Math.sin(az));
    const end = out.clone().multiplyScalar(reach).setY(0.078 + rise);
    const stalk = new THREE.CatmullRomCurve3([
        new THREE.Vector3(0, 0.076, 0),
        out.clone().multiplyScalar(reach * 0.45).setY(0.078 + rise * 0.85),
        end,
    ]);
    add(new THREE.TubeGeometry(stalk, 12, 0.0035, 6, false), STEM);

    const trap = new THREE.Group();
    trap.position.copy(end);
    plant.add(trap);
    trap.lookAt(end.clone().add(out.clone().setY(0.75)).add(plant.position));
    return { trap, upper: lobe(trap, true), lower: lobe(trap, false), phase: i * 1.7 };
});

// A thin flower stalk with a few small white flowers.
add(new THREE.CylinderGeometry(0.002, 0.0025, 0.2, 6), STEM).position.set(-0.01, 0.176, -0.012);
for (const [x, y, z] of [[-0.01, 0.282, -0.012], [0.004, 0.272, -0.004], [-0.022, 0.27, -0.004]]) {
    const flower = add(new THREE.SphereGeometry(0.008, 10, 8), mat(0xf6f3ea, 0.6));
    flower.position.set(x, y, z);
    flower.scale.y = 0.6;
}

for (const mesh of parts) clickable(mesh, 'flytrap', 'Venus flytrap · tap it');

// Open, the traps breathe a little. Tapped, they snap shut, stay shut a moment, then reopen slowly.
let snapAt = -10;
let now = 0;
animated.push((t) => {
    now = t;
    const dt = t - snapAt;
    const open = dt < 0.08 ? 1 - dt / 0.08 : dt < 1.4 ? 0 : dt < 3.4 ? (dt - 1.4) / 2 : 1;
    for (const { trap, upper, lower, phase } of traps) {
        const angle = 0.04 + open * (0.5 + Math.sin(t * 1.2 + phase) * 0.04);
        upper.rotation.x = -angle;
        lower.rotation.x = angle;
        trap.rotation.z = Math.sin(t * 0.7 + phase) * 0.05;
    }
});

const JOKES = [
    'Why do programmers prefer dark mode? Because light attracts bugs.',
    'I only eat bugs. Got any in your code?',
    'My diet? Strictly bug-driven development.',
    'I don\'t have a garbage collector. I have teeth.',
    'There are 10 kinds of plants: those that eat bugs, and those that don\'t.',
    'A SQL query walks into a bar, sees two tables and asks: may I join you?',
    'Why did the developer go broke? He used up all his cache.',
    'I\'d tell you a UDP joke, but you might not get it.',
    'Knock knock. Race condition. Who\'s there?',
    'It works on my machine. Then I ate the machine.',
];
let lastJoke = -1;

onClick('flytrap', (event) => {
    sfx('click');
    snapAt = now;
    let next;
    do next = Math.floor(Math.random() * JOKES.length); while (next === lastJoke);
    lastJoke = next;
    say('*chomp* ' + JOKES[next], event);
});
