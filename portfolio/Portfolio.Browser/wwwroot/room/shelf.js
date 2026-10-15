// The bookshelf: one shelf per era, one book per repository, and the trophy on top.

import * as THREE from 'three';
import { scene, box, clickable, canvasTexture } from './stage.js';

const SHELF = { left: -3.2, right: -1.2, z: -2.28, boards: [0.4, 1.1, 1.8] };
for (const x of [SHELF.left, SHELF.right])
    box(0.06, 2.45, 0.34, 0x6f4a2c, x, 1.225, SHELF.z);
for (const y of [...SHELF.boards, 2.42])
    box(2.0, 0.05, 0.34, 0x6f4a2c, -2.2, y, SHELF.z);
box(2.0, 2.4, 0.02, 0x5a3c24, -2.2, 1.22, SHELF.z - 0.16);

const ERA_COLORS = { xamarin: 0x3498db, maui: 0x7b4fd6, avalonia: 0x1fb5a0 };

function spineTexture(name, color) {
    return canvasTexture(64, 256, (g, w, h) => {
        g.fillStyle = '#' + color.getHexString(); g.fillRect(0, 0, w, h);
        g.fillStyle = 'rgba(255,255,255,0.75)'; g.fillRect(6, 14, w - 12, 4); g.fillRect(6, h - 18, w - 12, 4);
        g.translate(w / 2, h / 2); g.rotate(Math.PI / 2);
        g.fillStyle = '#ffffff'; g.font = '600 22px system-ui, sans-serif'; g.textAlign = 'center'; g.textBaseline = 'middle';
        let label = name;
        while (g.measureText(label).width > h - 56 && label.length > 4) label = label.slice(0, -2);
        g.fillText(label === name ? name : label + '…', 0, 0);
    });
}

function buildBooks(content) {
    content.eras.forEach((era, shelf) => {
        let x = SHELF.left + 0.07;
        content.projects.filter(p => p.era === era.id).forEach((project, i) => {
            const thickness = 0.09 + Math.min(project.stars, 2700) / 2700 * 0.13;
            const height = 0.4 + (i % 3) * 0.05;
            const color = new THREE.Color(ERA_COLORS[era.id]).offsetHSL(0, 0, (i % 4 - 1.5) * 0.04);
            const book = box(thickness, height, 0.24, color, x + thickness / 2, SHELF.boards[shelf] + 0.025 + height / 2, SHELF.z + 0.03, scene, false);
            // Face 4 is the spine, which looks out into the room.
            const plain = book.material;
            const spine = new THREE.MeshStandardMaterial({ map: spineTexture(project.name, color), roughness: 0.85 });
            book.material = [plain, plain, plain, plain, spine, plain];
            clickable(book, 'project', `${project.name} · ★ ${project.stars}`, { ...project, eraName: era.name });
            x += thickness + 0.012;
        });
    });
}

const library = { content: null };
fetch('./content.json')
    .then(r => r.json())
    .then(loaded => { library.content = loaded; buildBooks(loaded); })
    .catch(e => console.error('content.json', e));

// The trophy appears on top of the shelf once the game is finished.
const trophy = new THREE.Group();
trophy.position.set(-1.6, 2.445, SHELF.z);
box(0.16, 0.04, 0.12, 0x6f4a2c, 0, 0.02, 0, trophy);
box(0.04, 0.1, 0.04, 0xf2c230, 0, 0.09, 0, trophy);
box(0.16, 0.14, 0.1, 0xf2c230, 0, 0.21, 0, trophy);
trophy.visible = false;
scene.add(trophy);

try { trophy.visible = localStorage.getItem('trophy') === '1'; } catch { /* storage may be blocked */ }

addEventListener('pixos:trophy', () => {
    trophy.visible = true;
    try { localStorage.setItem('trophy', '1'); } catch { /* storage may be blocked */ }
});

export { library };
