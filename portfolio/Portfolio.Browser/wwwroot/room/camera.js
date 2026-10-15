// Where the camera stands, and how it moves from one place to another.

import * as THREE from 'three';
import { camera } from './stage.js';

const HOME = { pos: new THREE.Vector3(0, 1.95, 4.4), look: new THREE.Vector3(0, 1.4, -2) };

// On a tall, narrow window the camera steps back so the desk and shelf still fit.
function homePos() {
    const back = Math.min(2.4, Math.max(1, 1.25 / camera.aspect));
    return HOME.look.clone().add(HOME.pos.clone().sub(HOME.look).multiplyScalar(back));
}
const view = { pos: HOME.pos.clone(), look: HOME.look.clone() };
let tween = null;

function go(pos, look, ms, done) {
    tween = { from: { pos: view.pos.clone(), look: view.look.clone() }, to: { pos, look }, start: performance.now(), ms, done };
}

function screenCamera(target) {
    // On the screen's own axis. The laptop's screen covers the whole viewport; the phone's
    // fits inside it (target.fit of the height, or of the width on a narrow window).
    const t = Math.tan(THREE.MathUtils.degToRad(camera.fov / 2));
    const distance = target.fit
        ? Math.max(target.h / 2 / t, target.w / 2 / (t * camera.aspect)) / target.fit
        : Math.min(target.h / 2 / t, target.w / 2 / (t * camera.aspect)) * 0.97;
    return target.center.clone().addScaledVector(target.normal, distance);
}

const ease = (k) => k < 0.5 ? 4 * k * k * k : 1 - Math.pow(-2 * k + 2, 3) / 2;

function stopCamera() {
    tween = null;
}

// Moves the camera one frame along its way.
function stepCamera(now) {
    if (!tween) return;
    const k = Math.min(1, (now - tween.start) / tween.ms);
    view.pos.lerpVectors(tween.from.pos, tween.to.pos, ease(k));
    view.look.lerpVectors(tween.from.look, tween.to.look, ease(k));
    if (k === 1) {
        const done = tween.done;
        tween = null;
        done?.();
    }
}

export { HOME, homePos, go, view, ease, stopCamera, screenCamera, stepCamera };
