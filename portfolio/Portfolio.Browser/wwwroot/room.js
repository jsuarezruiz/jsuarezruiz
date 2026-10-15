// The room: sitting down at the laptop or picking up the phone, the cards, and the loop.

import * as THREE from 'three';
import { EffectComposer } from 'three/addons/postprocessing/EffectComposer.js';
import { RenderPass } from 'three/addons/postprocessing/RenderPass.js';
import { UnrealBloomPass } from 'three/addons/postprocessing/UnrealBloomPass.js';
import { OutputPass } from 'three/addons/postprocessing/OutputPass.js';
import { canvas, out, card, hint, status, phoneApp, renderer, scene, camera, animated, state, onClick, click, sfx } from './room/stage.js';
import { mouse, pick, hideTip } from './room/tips.js';
import { HOME, homePos, view, go, screenCamera, stepCamera } from './room/camera.js';
import { SCREEN, PHONE } from './room/devices.js';
import { paper } from './room/desk.js';
import { isDogAwake } from './room/dog.js';
import { library } from './room/shelf.js';
import { closePhoneApp } from './room/phoneapp.js';
import { startIntro, stepIntro } from './room/intro.js';
import './room/shell.js';
import './room/window.js';
import './room/furniture.js';
import './room/monkey.js';
import './room/award.js';
import './room/bot.js';
import './room/avalonia.js';
import './room/plant.js';
import './room/konami.js';

let device = SCREEN; // the screen Avalonia is showing on: the laptop's or the phone's
let pendingApp = null;

// Avalonia is laid over exactly the area the screen occupies once the camera has arrived.
function placeOverlay() {
    if (!device.fit) {
        out.classList.remove('phone');
        out.style.width = out.style.height = out.style.borderRadius = '';
        return;
    }
    const height = Math.min(innerHeight, innerWidth * device.h / device.w) * device.fit;
    out.classList.add('phone');
    out.style.height = Math.round(height) + 'px';
    out.style.width = Math.round(height * device.w / device.h) + 'px';
    out.style.borderRadius = Math.round(height * device.r / device.h) + 'px';
    phoneApp.style.height = out.style.height;
    phoneApp.style.width = out.style.width;
    phoneApp.style.setProperty('--corner', out.style.borderRadius);
}

// The picture goes through a soft bloom, so the lamp, the screens and the window glow.
const composer = new EffectComposer(renderer, new THREE.WebGLRenderTarget(1, 1, { samples: 4, type: THREE.HalfFloatType }));
composer.addPass(new RenderPass(scene, camera));
composer.addPass(new UnrealBloomPass(new THREE.Vector2(1, 1), 0.32, 0.7, 0.86));
composer.addPass(new OutputPass());

function resize() {
    renderer.setPixelRatio(Math.min(devicePixelRatio, 2));
    renderer.setSize(innerWidth, innerHeight, false);
    composer.setPixelRatio(Math.min(devicePixelRatio, 2));
    composer.setSize(innerWidth, innerHeight);
    camera.aspect = innerWidth / innerHeight;
    camera.updateProjectionMatrix();
    if (state.mode === 'phone') {
        placeOverlay();
        view.pos.copy(screenCamera(device));
    } else if (state.mode === 'room' || state.mode === 'card') {
        view.pos.copy(homePos());
    }
}
addEventListener('resize', resize);
resize();

function sitDown(app, target = SCREEN) {
    state.mode = 'moving';
    device = target;
    pendingApp = app ?? null;
    hideTip();
    closeCard();
    hint.classList.add('gone');
    go(screenCamera(target), target.center.clone(), 1100, () => {
        state.mode = target === PHONE ? 'phone' : 'crt';
        window.pixsound?.snore(state.mode !== 'crt' && !isDogAwake());
        enterOs();
    });
}

function enterOs() {
    if (!window.pixos?.ready) {
        status.textContent = 'Suárez XP is still booting…';
        status.classList.remove('gone');
        return;
    }
    placeOverlay();
    if (device === PHONE) {
        window.pixos.setMode('phone');
        status.textContent = 'Click outside the phone to put it down';
        status.classList.remove('gone');
    } else {
        window.pixos.setMode('desktop');
        window.pixos.boot();
        if (pendingApp) window.pixos.openApp(pendingApp);
    }
    pendingApp = null;
    out.classList.add('active');
}

function standUp() {
    window.pixsound?.snore(!isDogAwake());
    closePhoneApp(true);
    out.classList.remove('active');
    // Once the screen has faded out, tell the OS nobody is looking, so it stops animating.
    setTimeout(() => window.pixos?.setMode('room'), 400);
    status.classList.add('gone');
    state.mode = 'moving';
    go(homePos(), HOME.look.clone(), 900, () => { state.mode = 'room'; });
}

addEventListener('pixos:ready', () => {
    status.textContent = 'Suárez XP ready';
    setTimeout(() => { if (state.mode !== 'phone') status.classList.add('gone'); }, 2500);
    if (state.mode === 'crt' || state.mode === 'phone') enterOs();
});
addEventListener('pixos:standup', standUp);

function el(tag, text, className) {
    const node = document.createElement(tag);
    if (text) node.textContent = text;
    if (className) node.className = className;
    return node;
}

function closeButton() {
    const close = el('button', 'Close', 'quiet');
    close.addEventListener('click', () => {
        closeCard();
        if (state.mode === 'paper') standUp(); else state.mode = 'room';
    });
    return close;
}

function closeCard() {
    card.hidden = true;
    card.replaceChildren();
}

function showProject(project, linkText = 'Open on GitHub') {
    state.mode = 'card';
    hideTip();
    const actions = el('div', null, 'actions');
    const link = el('a', linkText);
    link.href = project.url;
    link.target = '_blank';
    link.rel = 'noopener';
    actions.append(link, closeButton());

    const meta = [project.eraName, project.stars != null ? `★ ${project.stars}` : null].filter(Boolean).join(' · ');
    card.replaceChildren(el('h2', project.name), el('p', meta, 'meta'), el('p', project.description), actions);
    card.hidden = false;
}

function showPaper() {
    state.mode = 'moving';
    hideTip();
    hint.classList.add('gone');
    const target = paper.position.clone();
    go(target.clone().add(new THREE.Vector3(0, 0.8, 0.3)), target, 900, () => {
        state.mode = 'paper';
        const actions = el('div', null, 'actions');
        const inOs = el('button', 'Open Resume.doc in Suárez XP');
        inOs.addEventListener('click', () => sitDown('resume'));
        actions.append(inOs, closeButton());
        // The same CV that Resume.doc shows, from content.json.
        const resume = library.content?.resume;
        const list = (heading, lines) => lines?.length ? [el('h3', heading), ...lines.map(line => el('p', line))] : [];
        const jobs = (resume?.experience ?? []).flatMap(job => [
            el('p', `${job.role}, ${job.company}`, 'job'),
            el('p', job.period, 'meta'),
            ...job.points.map(point => el('p', point)),
        ]);
        card.replaceChildren(
            el('h2', library.content?.profile.name ?? 'Javier Suárez'),
            el('p', `${resume?.title ?? 'Software Engineer'} · ${library.content?.profile.location ?? 'Sevilla'}`, 'meta'),
            el('p', resume?.summary ?? ''),
            el('h3', 'Experience'), ...jobs,
            ...list('Open source', resume?.openSource),
            ...list('Achievements', resume?.achievements),
            ...list('Community and teaching', resume?.communities),
            ...list('Education', resume?.education),
            ...list('Skills', resume?.skills && [resume.skills.join(' · ')]),
            actions);
        card.hidden = false;
    });
}

onClick('crt', () => sitDown());
onClick('phone', () => sitDown(null, PHONE));
onClick('paper', () => { sfx('paper'); showPaper(); });
onClick('project', (event, project) => { sfx('paper'); showProject(project); });
onClick('meetup', (event, group) => { sfx('paper'); showProject(group, 'Open on Meetup'); });

canvas.addEventListener('click', (event) => {
    // The room stays visible around the phone: a click on it puts the phone down.
    if (state.mode === 'phone') { standUp(); return; }
    if (state.mode !== 'room') return;
    const mesh = pick(event);
    if (mesh) click(mesh, event);
});

// The dog snores while the room is in view. Browsers only allow sound after a first click.
addEventListener('pointerdown', () => window.pixsound?.snore(state.mode !== 'crt' && !isDogAwake()), { once: true });

addEventListener('keydown', (event) => {
    if (event.key !== 'Escape') return;
    if (state.mode === 'paper') { closeCard(); standUp(); }
    else if (state.mode === 'phone') standUp();
    else if (state.mode === 'card') { closeCard(); state.mode = 'room'; }
});

startIntro();

renderer.setAnimationLoop((now) => {
    // Nothing to draw while the OS covers the page.
    if (state.mode === 'crt' && out.classList.contains('active')) return;

    stepIntro(now);

    stepCamera(now);

    camera.position.copy(view.pos);
    if (state.mode === 'room' || state.mode === 'card') {
        camera.position.x += mouse.x * 0.35;
        camera.position.y -= mouse.y * 0.15;
    }
    camera.lookAt(view.look);
    for (const step of animated) step(now / 1000);
    composer.render();
});
