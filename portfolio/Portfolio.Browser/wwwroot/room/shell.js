// Floor, walls, ceiling, and the poster on the wall.

import * as THREE from 'three';
import { scene, time, box, clickable, canvasTexture, onClick, sfx } from './stage.js';

const floor = new THREE.Mesh(
    new THREE.BoxGeometry(8, 0.1, 7),
    new THREE.MeshStandardMaterial({
        roughness: 0.7,
        map: canvasTexture(512, 512, (g, w, h) => {
            g.fillStyle = '#9a6a44'; g.fillRect(0, 0, w, h);
            for (let i = 0; i < 8; i++) {
                g.fillStyle = i % 2 ? '#a27248' : '#946440';
                g.fillRect(i * 64, 0, 62, h);
                g.fillStyle = '#7d5434';
                for (let y = (i * 97) % 200; y < h; y += 220) g.fillRect(i * 64, y, 62, 2);
            }
        }),
    }));
floor.position.set(0, -0.05, 0.5);
floor.receiveShadow = true;
scene.add(floor);

box(8, 3.6, 0.1, 0xece6d8, 0, 1.8, -2.5, scene, false);
box(0.1, 3.6, 7, 0xe0d9ca, -4, 1.8, 0.5, scene, false);
box(0.1, 3.6, 7, 0xe0d9ca, 4, 1.8, 0.5, scene, false);
// The ceiling is lit only from below, so it carries a little light of its own.
box(8, 0.1, 7, 0xf6f2e8, 0, 3.65, 0.5, scene, false).material.emissive.setHex(time === 'night' ? 0x1c1a22 : 0x8f8a80);
box(8, 0.12, 0.05, 0xffffff, 0, 3.54, -2.43);
box(8, 0.16, 0.04, 0xffffff, 0, 0.08, -2.43);

// A framed poster above the desk. One is picked at random; clicking it swaps in the next.
const POSTERS = [
    { back: '#1e1e2a', ink: '#f4f1e6', accent: '#f2c230', small: 'CERTIFIED', lines: ['IT WORKS', 'ON MY', 'MACHINE'], foot: 'No warranty on yours' },
    { back: '#d8452e', ink: '#f4f1e6', accent: '#f4f1e6', small: 'KEEP CALM', lines: ['AND', 'DELETE', 'BIN + OBJ'], foot: 'Then rebuild' },
    { back: '#f4f1e6', ink: '#1e1e2a', accent: '#2456c8', small: '2 HARD PROBLEMS', lines: ['1. NAMING', '2. CACHING', '3. OFF BY ONE'], foot: 'in computer science' },
];

function posterTexture(poster) {
    return canvasTexture(360, 480, (g, w, h) => {
        g.fillStyle = poster.back; g.fillRect(0, 0, w, h);
        g.strokeStyle = poster.accent; g.lineWidth = 6; g.strokeRect(18, 18, w - 36, h - 36);
        g.textAlign = 'center';
        g.fillStyle = poster.accent; g.font = '600 26px system-ui, sans-serif'; g.fillText(poster.small, w / 2, 96);
        g.fillStyle = poster.ink; g.font = '800 54px system-ui, sans-serif';
        poster.lines.forEach((line, i) => g.fillText(line, w / 2, 190 + i * 74, w - 60));
        g.fillStyle = poster.accent; g.font = '500 22px system-ui, sans-serif'; g.fillText(poster.foot, w / 2, h - 52);
    });
}

let posterIndex = Math.floor(Math.random() * POSTERS.length);
box(0.66, 0.86, 0.03, 0x2a1c12, 0.7, 2.42, -2.45);
const poster = new THREE.Mesh(
    new THREE.PlaneGeometry(0.6, 0.8),
    new THREE.MeshStandardMaterial({ roughness: 0.9, map: posterTexture(POSTERS[posterIndex]) }));
poster.position.set(0.7, 2.42, -2.43);
scene.add(clickable(poster, 'poster', 'Poster · click for another'));

function nextPoster() {
    posterIndex = (posterIndex + 1) % POSTERS.length;
    poster.material.map.dispose();
    poster.material.map = posterTexture(POSTERS[posterIndex]);
}

onClick('poster', () => { sfx('paper'); nextPoster(); });

// A second poster, left of the first: the SevillaDotNet logo in pixels. Clicking it shows the meetup.
const MEETUP = {
    name: 'SevillaDotNet',
    eraName: '.NET user group · Sevilla',
    description: 'The .NET user group in Sevilla, where we meet to share what we learn about .NET and Microsoft technologies. I help organise it.',
    url: 'https://www.meetup.com/sevilladotnet/',
};

const LOGO_SCALE = 5; // the logo is 56×36 pixels; each one becomes a 5×5 block
const meetupTexture = canvasTexture(360, 480, (g, w, h) => {
    g.fillStyle = '#f4f1e6'; g.fillRect(0, 0, w, h);
}, true);
const logo = new Image();
logo.onload = () => {
    const g = meetupTexture.image.getContext('2d');
    const w = logo.width * LOGO_SCALE, h = logo.height * LOGO_SCALE;
    g.imageSmoothingEnabled = false;
    g.drawImage(logo, Math.round((360 - w) / 2), Math.round((480 - h) / 2), w, h);
    meetupTexture.needsUpdate = true;
};
logo.src = 'room/sevilladotnet.png';

box(0.66, 0.86, 0.03, 0x2a1c12, -0.42, 2.42, -2.45);
const meetupPoster = new THREE.Mesh(
    new THREE.PlaneGeometry(0.6, 0.8),
    new THREE.MeshStandardMaterial({ roughness: 0.9, map: meetupTexture }));
meetupPoster.position.set(-0.42, 2.42, -2.43);
scene.add(clickable(meetupPoster, 'meetup', 'SevillaDotNet · click for the meetup', MEETUP));
