// The view from the window: Sevilla, drawn on a canvas.

import { time, canvasTexture } from './stage.js';

// The city, drawn in three depths that fade with distance, and mirrored in the river.
function sevillaTexture() {
    return canvasTexture(980, 660, (g, w, h) => {
        const ground = 548;   // where the buildings stand
        const water = 566;    // where the river begins
        const tint = {
            day: { sky: ['#5fa6ee', '#a9d3f8', '#eaf5fd'], far: '#a9bad2', mid: '#7f93b1', near: '#4a5d79', river: ['#8db6de', '#5f8fc4'], lit: 'rgba(255,255,255,0.3)' },
            dusk: { sky: ['#2b2a66', '#b5527e', '#f6a15c'], far: '#7d5681', mid: '#55375e', near: '#2a1c33', river: ['#b0627a', '#4a3063'], lit: '#ffd98a' },
            night: { sky: ['#050818', '#0f1740', '#27306a'], far: '#141b44', mid: '#0d1230', near: '#060817', river: ['#17215a', '#080c26'], lit: '#ffd98a' },
        }[time];

        // Sky
        const sky = g.createLinearGradient(0, 0, 0, ground);
        tint.sky.forEach((color, i, all) => sky.addColorStop(i / (all.length - 1), color));
        g.fillStyle = sky; g.fillRect(0, 0, w, h);

        const sunAt = time === 'dusk' ? [250, 470, 46] : time === 'night' ? [200, 130, 30] : [230, 150, 40];
        const halo = g.createRadialGradient(sunAt[0], sunAt[1], sunAt[2] * 0.6, sunAt[0], sunAt[1], sunAt[2] * 5);
        halo.addColorStop(0, time === 'night' ? 'rgba(210,220,255,0.35)' : 'rgba(255,236,190,0.75)');
        halo.addColorStop(1, 'rgba(255,255,255,0)');
        g.fillStyle = halo; g.fillRect(0, 0, w, ground);
        g.fillStyle = time === 'night' ? '#f4f1e6' : time === 'dusk' ? '#ffe2a8' : '#ffffff';
        g.beginPath(); g.arc(sunAt[0], sunAt[1], sunAt[2], 0, Math.PI * 2); g.fill();

        if (time === 'night') {
            for (let i = 0; i < 90; i++) {
                g.globalAlpha = 0.4 + (i % 5) * 0.15;
                g.fillRect((i * 139) % w, (i * 83) % 380, i % 7 ? 2 : 3, i % 7 ? 2 : 3);
            }
            g.globalAlpha = 1;
        } else {
            // A few long, soft clouds.
            g.fillStyle = time === 'dusk' ? 'rgba(255,200,170,0.5)' : 'rgba(255,255,255,0.75)';
            for (const [x, y, s] of [[560, 110, 1], [760, 190, 0.7], [90, 250, 0.8], [400, 60, 0.6]]) {
                for (const [dx, dy, rx, ry] of [[0, 0, 70, 16], [40, -10, 50, 18], [-45, 4, 46, 12], [80, 6, 40, 10]]) {
                    g.beginPath(); g.ellipse(x + dx * s, y + dy * s, rx * s, ry * s, 0, 0, Math.PI * 2); g.fill();
                }
            }
            // And a few birds.
            g.strokeStyle = tint.near; g.lineWidth = 2;
            for (const [x, y] of [[650, 280], [676, 268], [700, 290], [420, 210]]) {
                g.beginPath(); g.moveTo(x - 7, y - 3); g.quadraticCurveTo(x - 3, y - 7, x, y); g.quadraticCurveTo(x + 3, y - 7, x + 7, y - 3); g.stroke();
            }
        }

        const rect = (x, y, width, height) => g.fillRect(x, y, width, height);
        const windows = [];   // filled in as the buildings are drawn, painted last
        const arch = (x, y, width, height) => { g.beginPath(); g.roundRect(x, y, width, height, [width / 2, width / 2, 0, 0]); g.fill(); };

        // Far: Torre Sevilla, the towers of the Plaza de España, the Setas
        g.fillStyle = tint.far;
        g.beginPath(); g.roundRect(860, 196, 46, ground - 196, [23, 23, 0, 0]); g.fill();
        for (const x of [640, 792]) {
            rect(x, 372, 20, ground - 372);
            rect(x - 4, 366, 28, 8);
            rect(x + 3, 338, 14, 30);
            g.beginPath(); g.moveTo(x + 1, 338); g.lineTo(x + 10, 300); g.lineTo(x + 19, 338); g.fill();
            rect(x + 9, 288, 2, 14);
        }
        rect(660, 486, 132, ground - 486);
        for (let x = 664; x < 790; x += 14) { g.beginPath(); g.arc(x + 6, 486, 6, Math.PI, 0); g.fill(); }
        // The Setas: a rippling honeycomb canopy on trunks.
        g.beginPath();
        g.moveTo(16, 470);
        for (let x = 16; x < 220; x += 34) g.quadraticCurveTo(x + 17, 436, x + 34, 470);
        g.lineTo(220, 486); g.lineTo(16, 486); g.fill();
        for (const x of [50, 118, 186]) { g.beginPath(); g.moveTo(x - 16, 486); g.lineTo(x + 16, 486); g.lineTo(x + 6, ground); g.lineTo(x - 6, ground); g.fill(); }
        [[0, 500, 60], [226, 492, 50], [600, 500, 44], [906, 506, 74]].forEach(([x, y, width]) => rect(x, y, width, ground - y));

        // Middle: the cathedral, long and Gothic, with its dome
        g.fillStyle = tint.mid;
        rect(286, 452, 250, ground - 452);
        rect(300, 420, 150, 34);
        for (let x = 288; x < 534; x += 18) {
            rect(x + 5, 430 - (x < 452 ? 22 : 0), 4, 24);
            g.beginPath(); g.moveTo(x + 1, 432 - (x < 452 ? 22 : 0)); g.lineTo(x + 7, 404 - (x < 452 ? 22 : 0)); g.lineTo(x + 13, 432 - (x < 452 ? 22 : 0)); g.fill();
        }
        g.beginPath(); g.arc(372, 420, 38, Math.PI, 0); g.fill();
        rect(366, 362, 12, 24);
        g.beginPath(); g.arc(372, 362, 8, Math.PI, 0); g.fill();
        rect(371, 340, 2, 16);
        for (let x = 300; x < 530; x += 26) windows.push([x, 476, 9, 34]);

        // Near: the Giralda
        g.fillStyle = tint.near;
        rect(536, 186, 82, ground - 186);                       // the Moorish shaft
        rect(530, 178, 94, 10);                                 // its cornice
        rect(544, 128, 66, 52);                                 // the belfry
        rect(538, 122, 78, 8);
        for (const x of [540, 610]) { rect(x, 106, 5, 18); g.beginPath(); g.arc(x + 2.5, 104, 4, 0, Math.PI * 2); g.fill(); }
        rect(556, 92, 42, 32);                                  // the clock stage
        rect(552, 87, 50, 6);
        rect(565, 62, 24, 26);                                  // the lantern
        g.beginPath(); g.arc(577, 62, 12, Math.PI, 0); g.fill();
        rect(574, 36, 6, 16);                                   // the Giraldillo, with her banner
        g.beginPath(); g.arc(577, 33, 5, 0, Math.PI * 2); g.fill();
        g.beginPath(); g.moveTo(580, 38); g.lineTo(596, 43); g.lineTo(580, 50); g.fill();
        rect(576, 18, 2, 14);
        // Balconied windows up the shaft, and the arches of the belfry.
        for (let y = 214; y < 500; y += 58) { windows.push([552, y, 12, 26], [590, y, 12, 26]); rect(548, y + 26, 20, 3); rect(586, y + 26, 20, 3); }
        for (let x = 550; x < 600; x += 15) windows.push([x, 140, 9, 32]);
        windows.push([571, 100, 12, 14]);

        // Near: the Torre del Oro, on the bank
        rect(92, 456, 84, ground - 456);
        for (let x = 92; x < 176; x += 14) rect(x, 446, 9, 11);
        rect(112, 414, 44, 34);
        for (let x = 112; x < 156; x += 11) rect(x, 407, 7, 8);
        rect(126, 386, 16, 24);
        g.beginPath(); g.arc(134, 386, 9, Math.PI, 0); g.fill();
        rect(133, 362, 2, 16);
        g.beginPath(); g.moveTo(135, 362); g.lineTo(148, 366); g.lineTo(135, 371); g.fill();
        for (const x of [106, 130, 154]) windows.push([x, 484, 8, 22]);
        windows.push([130, 424, 8, 14]);

        // Palms and orange trees along the bank
        g.strokeStyle = tint.near; g.lineCap = 'round';
        for (const [x, top] of [[40, 462], [210, 474], [250, 456], [470, 470], [700, 466], [742, 478]]) {
            g.lineWidth = 4;
            g.beginPath(); g.moveTo(x, ground); g.quadraticCurveTo(x + 5, (ground + top) / 2, x, top); g.stroke();
            g.lineWidth = 3.5;
            for (let a = -2.75; a <= -0.4; a += 0.34) {
                g.beginPath(); g.moveTo(x, top);
                g.quadraticCurveTo(x + Math.cos(a) * 18, top + Math.sin(a) * 26, x + Math.cos(a) * 34, top + Math.sin(a) * 12 + 8);
                g.stroke();
            }
        }
        for (const x of [20, 190, 232, 280, 500, 640, 760, 840, 930]) { g.beginPath(); g.arc(x, ground - 12, 15, 0, Math.PI * 2); g.fill(); rect(x - 2, ground - 8, 4, 10); }

        // The bank, the Triana bridge and the Guadalquivir
        rect(0, ground, w, water - ground);
        const river = g.createLinearGradient(0, water, 0, h);
        river.addColorStop(0, tint.river[0]); river.addColorStop(1, tint.river[1]);
        g.fillStyle = river; rect(0, water, w, h - water);

        // Everything standing above the water, mirrored and faded.
        g.save();
        g.globalAlpha = 0.28;
        g.beginPath(); g.rect(0, water, w, h - water); g.clip();
        // Flip about the waterline and squash: the top of each tower lands deepest in the river.
        g.translate(0, water); g.scale(1, -0.5);
        g.drawImage(g.canvas, 0, water - 380, w, 380, 0, -380, w, 380);
        g.restore();
        g.fillStyle = 'rgba(255,255,255,0.22)';
        for (let i = 0; i < 16; i++) rect((i * 113) % (w - 80), water + 12 + (i * 17) % 70, 30 + (i % 4) * 16, 2);

        // The bridge: a flat deck on iron arches, with a row of rings in each.
        g.fillStyle = tint.near; g.strokeStyle = tint.near;
        rect(640, ground - 12, w - 640, 9);
        for (let x = 640; x < w; x += 104) {
            g.lineWidth = 6; g.beginPath(); g.arc(x + 52, ground + 34, 56, Math.PI * 1.13, Math.PI * 1.87); g.stroke();
            g.lineWidth = 2.5;
            for (let i = 0; i < 6; i++) { g.beginPath(); g.arc(x + 12 + i * 16, ground - 3 + Math.abs(i - 2.5) * 5.5, 5, 0, Math.PI * 2); g.stroke(); }
            rect(x - 6, ground - 6, 12, water - ground + 26);
        }

        // Windows: a hint by day, lit at dusk and at night
        g.fillStyle = tint.lit;
        for (const [x, y, width, height] of windows) arch(x, y, width, height);
        if (time !== 'day') {
            for (let y = 220; y < ground - 20; y += 20) rect(872 + (y % 3) * 9, y, 6, 8);
            for (let i = 0; i < 30; i++) rect(8 + (i * 97) % (w - 20), 508 + (i * 31) % 30, 4, 5);
            for (let x = 650; x < w; x += 34) { g.beginPath(); g.arc(x, ground - 16, 3, 0, Math.PI * 2); g.fill(); }
        }
    });
}

export { sevillaTexture };
