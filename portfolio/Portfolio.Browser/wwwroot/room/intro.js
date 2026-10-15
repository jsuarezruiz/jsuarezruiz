import * as THREE from 'three';
import { canvas, hint, renderer, scene, animated, canvasTexture, state } from './stage.js';
import { HOME, homePos, view, go, ease, stopCamera } from './camera.js';
import { screen } from './devices.js';
import { setLamp, dimLamp, isLampOn } from './desk.js';

// The page opens on the window and the skyline, in the dark. The camera pulls back into the
// room as the light comes up, the lamp switches on, and the laptop wakes. Any click or key
// skips it; visitors who ask for reduced motion, or who have seen it, get a short fade.

const intro = document.getElementById('intro');
const EXPOSURE = renderer.toneMappingExposure;
const INTRO = { camera: 3400, light: 2200, lampAt: 2100, wakeAt: 2700, titleOut: 2600, end: 3900 };
let introStart = 0;
let introLampDone = false;

// A label that bobs above the laptop until the visitor first clicks something.
const marker = new THREE.Sprite(new THREE.SpriteMaterial({
    transparent: true,
    depthTest: false,
    map: canvasTexture(320, 150, (g, w) => {
        g.fillStyle = '#1e1e2a';
        g.beginPath(); g.roundRect(8, 8, w - 16, 84, 14); g.fill();
        g.beginPath(); g.moveTo(w / 2 - 22, 90); g.lineTo(w / 2 + 22, 90); g.lineTo(w / 2, 128); g.fill();
        g.fillStyle = '#f4f1e6';
        g.font = '700 40px system-ui, sans-serif';
        g.textAlign = 'center';
        g.textBaseline = 'middle';
        g.fillText('Start here', w / 2, 51);
    }),
}));
marker.scale.set(0.62, 0.29, 1);
marker.visible = false;
marker.renderOrder = 10;
scene.add(marker);
let started = false;
animated.push((t) => {
    marker.visible = state.mode === 'room' && !started;
    marker.position.set(0.7, 1.92 + Math.sin(t * 2.4) * 0.035, -2.0);
});
canvas.addEventListener('click', () => { if (state.mode !== 'intro') started = true; });

function startIntro() {
    const quick = matchMedia('(prefers-reduced-motion: reduce)').matches || seenIntro();
    state.mode = 'intro';
    hint.classList.add('gone');
    intro.hidden = false;

    renderer.toneMappingExposure = 0.12;
    screen.material.color.setScalar(0);
    if (isLampOn()) dimLamp();

    if (quick) {
        // No camera move: the room simply fades up.
        Object.assign(INTRO, { camera: 1, light: 900, lampAt: 300, wakeAt: 400, titleOut: 500, end: 1200 });
    } else {
        view.pos.set(2.15, 2.2, 0.2);
        view.look.set(2.8, 2.05, -2.4);
        go(homePos(), HOME.look.clone(), INTRO.camera);
    }
    introStart = performance.now();
    addEventListener('pointerdown', endIntro, { once: true });
    addEventListener('keydown', endIntro, { once: true });
}

function seenIntro() {
    try { return localStorage.getItem('pixos.intro') === 'seen'; } catch { return false; }
}

function stepIntro(now) {
    if (state.mode !== 'intro') return;
    const t = now - introStart;

    renderer.toneMappingExposure = THREE.MathUtils.lerp(0.12, EXPOSURE, ease(Math.min(1, t / INTRO.light)));
    if (t >= INTRO.lampAt && !introLampDone) {
        introLampDone = true;
        setLamp(isLampOn());
    }
    screen.material.color.setScalar(THREE.MathUtils.clamp((t - INTRO.wakeAt) / 500, 0, 1));
    if (t >= INTRO.titleOut) intro.classList.add('gone');
    if (t >= INTRO.end) endIntro();
}

function endIntro() {
    if (state.mode !== 'intro') return;
    removeEventListener('pointerdown', endIntro);
    removeEventListener('keydown', endIntro);

    stopCamera();
    view.pos.copy(homePos());
    view.look.copy(HOME.look);
    renderer.toneMappingExposure = EXPOSURE;
    screen.material.color.setScalar(1);
    setLamp(isLampOn());
    intro.classList.add('gone');
    setTimeout(() => { intro.hidden = true; }, 700);
    hint.classList.remove('gone');
    state.mode = 'room';
    try { localStorage.setItem('pixos.intro', 'seen'); } catch { /* storage may be blocked */ }
}

export { startIntro, stepIntro };
