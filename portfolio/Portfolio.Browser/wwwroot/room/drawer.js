// The desk drawer, and what turns up inside it.

import * as THREE from 'three';
import { scene, animated, box, clickable, canvasTexture, onClick, sfx } from './stage.js';
import { say } from './tips.js';
import { HOME, homePos, go } from './camera.js';
import { wakeDog } from './dog.js';

// The drawer slides open on a click. What is inside is picked each time it opens.
const drawer = new THREE.Group();
drawer.position.set(1.45, 0.8, -1.4);
scene.add(drawer);
const drawerParts = [
    box(0.74, 0.2, 0.03, 0xb07a45, 0, 0, 0.016, drawer),         // front
    box(0.3, 0.03, 0.02, 0x4a3320, 0, 0, 0.04, drawer),          // handle
    box(0.68, 0.015, 0.5, 0x8a5f36, 0, -0.085, -0.25, drawer),   // bottom
    box(0.015, 0.15, 0.5, 0x8a5f36, -0.34, -0.015, -0.25, drawer),
    box(0.015, 0.15, 0.5, 0x8a5f36, 0.34, -0.015, -0.25, drawer),
];
for (const part of drawerParts) clickable(part, 'drawer', 'Drawer · open it');

// Find one: a box of dog treats, and a few that fell out.
const treats = new THREE.Group();
treats.position.set(0, -0.075, -0.2);
drawer.add(treats);
const treatBox = box(0.26, 0.2, 0.12, 0xd8452e, -0.12, 0.1, -0.08, treats);
treatBox.rotation.y = 0.25;
const treatLabel = new THREE.Mesh(
    new THREE.PlaneGeometry(0.22, 0.16),
    new THREE.MeshStandardMaterial({
        roughness: 0.9,
        map: canvasTexture(220, 160, (g, w, h) => {
            g.fillStyle = '#f4f1e6'; g.fillRect(0, 0, w, h);
            g.textAlign = 'center'; g.fillStyle = '#d8452e';
            g.font = '800 50px system-ui, sans-serif'; g.fillText('DOG', w / 2, 62);
            g.fillText('TREATS', w / 2, 116);
            g.fillStyle = '#5a5a6a'; g.font = '600 20px system-ui, sans-serif'; g.fillText('bulldog size', w / 2, 146);
        }),
    }));
treatLabel.position.set(0, 0, 0.062);
treatBox.add(treatLabel);
for (const [x, z, turn] of [[0.12, 0.02, 0.4], [0.2, -0.1, -0.7], [0.08, -0.14, 1.3]]) {
    const bone = new THREE.Group();
    bone.position.set(x, 0.022, z);
    bone.rotation.y = turn;
    treats.add(bone);
    const biscuit = new THREE.MeshStandardMaterial({ color: 0xd9a868, roughness: 0.8 });
    const shaft = new THREE.Mesh(new THREE.CylinderGeometry(0.014, 0.014, 0.08, 10), biscuit);
    shaft.rotation.z = Math.PI / 2;
    bone.add(shaft);
    for (const end of [-1, 1])
        for (const side of [-1, 1]) {
            const knob = new THREE.Mesh(new THREE.SphereGeometry(0.018, 10, 8), biscuit);
            knob.position.set(end * 0.04, 0, side * 0.013);
            bone.add(knob);
        }
}

// Find two: every cable ever owned, in one knot.
const cables = new THREE.Group();
cables.position.set(0, -0.075, -0.22);
drawer.add(cables);
for (const [color, x, z, size, tilt, p, q] of [
    [0x1e1e2a, -0.14, -0.02, 0.07, 0.3, 2, 3], [0xf4f1e6, 0.05, -0.06, 0.08, -0.2, 3, 4],
    [0x3a6ea5, 0.17, 0.02, 0.06, 0.5, 2, 5], [0x5a5a6a, -0.02, 0.05, 0.065, 0.1, 3, 2]]) {
    const cable = new THREE.Mesh(
        new THREE.TorusKnotGeometry(size, 0.007, 90, 6, p, q),
        new THREE.MeshStandardMaterial({ color, roughness: 0.6 }));
    cable.position.set(x, 0.05, z);
    cable.rotation.set(Math.PI / 2 + tilt, 0, tilt * 2);
    cable.scale.y = 0.55;
    cables.add(cable);
}
box(0.07, 0.035, 0.07, 0xf4f1e6, 0.2, 0.03, -0.12, cables);   // a charger with no cable of its own
box(0.05, 0.02, 0.03, 0x8a8674, -0.2, 0.02, 0.08, cables);

let drawerOpen = false;

treats.visible = cables.visible = false;
function toggleDrawer(event) {
    drawerOpen = !drawerOpen;
    sfx(drawerOpen ? 'open' : 'close');
    for (const part of drawerParts) part.userData.label = drawerOpen ? 'Drawer · close it' : 'Drawer · open it';
    // Lean in over the open drawer, with the dog still in view; step back when it shuts.
    if (drawerOpen) go(new THREE.Vector3(1.05, 1.8, 0.75), new THREE.Vector3(1.0, 0.55, -1.35), 700);
    else go(homePos(), HOME.look.clone(), 700);
    if (!drawerOpen) { wakeDog(false); return; }

    const found = Math.random() < 0.5 ? treats : cables;
    treats.visible = found === treats;
    cables.visible = found === cables;
    if (found === treats) {
        wakeDog(true);
        say('Dog treats. Someone under the desk heard that.', event);
    } else {
        say('Every cable since 2009. Never the right one.', event);
    }
}
animated.push(() => {
    drawer.position.z += ((drawerOpen ? -0.98 : -1.4) - drawer.position.z) * 0.14;
});

function treatsOut() {
    return drawerOpen && treats.visible;
}

onClick('drawer', toggleDrawer);

export { treatsOut };
