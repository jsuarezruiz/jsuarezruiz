// A phone app: the project's real WebAssembly build, loaded on the phone's screen.

import { status, phoneApp, state } from './stage.js';

// It closes the way phone apps do: drag the bar at the bottom upwards and let go.
let phoneClosing = false;

function openPhoneApp(url) {
    if (state.mode !== 'phone') return;

    const frame = document.createElement('iframe');
    frame.src = url;
    frame.title = 'Live demo';
    frame.referrerPolicy = 'no-referrer';
    const sheet = document.createElement('div');
    sheet.className = 'sheet';
    sheet.append(frame);

    const bar = document.createElement('button');
    bar.type = 'button';
    bar.className = 'homebar';
    bar.setAttribute('aria-label', 'Close the app: drag up, or press Enter');

    let startY = 0;
    let lastY = 0;
    let lastT = 0;
    let speed = 0; // px per ms, negative is upwards
    let dragging = false;

    const follow = (dy) => {
        const progress = Math.min(1, Math.max(0, -dy) / (phoneApp.offsetHeight * 0.6));
        sheet.style.transform = `translateY(${Math.min(0, dy) * 0.55}px) scale(${1 - progress * 0.45})`;
        sheet.style.opacity = String(1 - progress * 0.35);
        return progress;
    };

    bar.addEventListener('pointerdown', (event) => {
        dragging = true;
        startY = lastY = event.clientY;
        lastT = event.timeStamp;
        speed = 0;
        bar.setPointerCapture(event.pointerId);
        phoneApp.classList.add('dragging');
    });
    bar.addEventListener('pointermove', (event) => {
        if (!dragging) return;
        const dt = event.timeStamp - lastT;
        if (dt > 0) speed = (event.clientY - lastY) / dt;
        lastY = event.clientY;
        lastT = event.timeStamp;
        follow(event.clientY - startY);
    });
    const release = (event) => {
        if (!dragging) return;
        dragging = false;
        phoneApp.classList.remove('dragging');
        const progress = follow(event.clientY - startY);
        // Far enough, or a quick flick upwards, closes it; otherwise it settles back.
        if (progress > 0.3 || speed < -0.6) closePhoneApp();
        else sheet.style.transform = sheet.style.opacity = '';
    };
    bar.addEventListener('pointerup', release);
    bar.addEventListener('pointercancel', release);
    bar.addEventListener('keydown', (event) => { if (event.key === 'Enter' || event.key === ' ') closePhoneApp(); });

    phoneClosing = false;
    phoneApp.classList.remove('closing', 'dragging');
    phoneApp.replaceChildren(sheet, bar);
    phoneApp.hidden = false;
    status.textContent = 'Swipe up from the bar to close the app';
}

function closePhoneApp(instant = false) {
    if (phoneApp.hidden || phoneClosing) return;
    const done = () => {
        phoneClosing = false;
        phoneApp.hidden = true;
        phoneApp.classList.remove('closing');
        phoneApp.replaceChildren();
    };
    if (state.mode === 'phone') status.textContent = 'Click outside the phone to put it down';
    if (instant) { done(); return; }

    // Shrink the rest of the way towards the home screen, then remove it.
    phoneClosing = true;
    phoneApp.classList.add('closing');
    const sheet = phoneApp.querySelector('.sheet');
    sheet.style.transform = 'translateY(-12%) scale(0.2)';
    sheet.style.opacity = '0';
    setTimeout(done, 260);
}

addEventListener('pixos:phoneapp', (event) => openPhoneApp(event.detail));

export { closePhoneApp };
