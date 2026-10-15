// The pointer: what it is over, the tooltip that follows it, and the lines the room's
// inhabitants say when clicked.

import * as THREE from 'three';
import { canvas, tip, camera, interactive, state } from './stage.js';

const mouse = { x: 0, y: 0 };

const raycaster = new THREE.Raycaster();
let hovered = null;

function pick(event) {
    const ndc = new THREE.Vector2(event.clientX / innerWidth * 2 - 1, -(event.clientY / innerHeight) * 2 + 1);
    raycaster.setFromCamera(ndc, camera);
    return raycaster.intersectObjects(interactive, false)[0]?.object ?? null;
}

function glowOf(mesh, hex) {
    if (!mesh) return;
    for (const material of [].concat(mesh.material))
        material.emissive?.setHex(hex || mesh.userData.glow || 0x000000);
}

function setHover(mesh) {
    if (hovered === mesh) return;
    glowOf(hovered, 0x000000);
    hovered = mesh;
    glowOf(hovered, 0x2a2a2a);
    canvas.style.cursor = mesh ? 'pointer' : 'default';
}

function hideTip() {
    setHover(null);
    tip.style.display = 'none';
}

canvas.addEventListener('pointermove', (event) => {
    mouse.x = event.clientX / innerWidth * 2 - 1;
    mouse.y = event.clientY / innerHeight * 2 - 1;
    if (state.mode !== 'room') return;

    const mesh = pick(event);
    setHover(mesh);
    // Something just spoke: leave its words up for a moment.
    if (performance.now() < spokenUntil) return;
    if (!mesh) { tip.style.display = 'none'; return; }
    showTip(mesh.userData.label, event);
});

let spokenUntil = 0;

function showTip(text, event) {
    tip.textContent = text;
    tip.style.display = 'block';
    tip.style.left = Math.max(8, Math.min(event.clientX + 14, innerWidth - tip.offsetWidth - 8)) + 'px';
    tip.style.top = event.clientY + 16 + 'px';
}

// A line from one of the room's inhabitants, shown where it was clicked.
function say(text, event) {
    showTip(text, event);
    spokenUntil = performance.now() + 2600;
    setTimeout(() => { if (performance.now() >= spokenUntil) tip.style.display = 'none'; }, 2650);
}

export { say, mouse, pick, hideTip };
