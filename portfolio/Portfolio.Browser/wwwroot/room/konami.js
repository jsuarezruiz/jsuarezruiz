import { status, state, sfx } from './stage.js';
import { setLamp, isLampOn } from './desk.js';
import { wakeDog } from './dog.js';
import { treatsOut } from './drawer.js';

// The Konami code, typed in the room: the dog wakes, the lamp flickers, and the game on the
// laptop starts with thirty lives from then on.
const KONAMI = ['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'b', 'a'];
let konamiAt = 0;
addEventListener('keydown', (event) => {
    if (state.mode !== 'room') { konamiAt = 0; return; }
    const key = event.key.length === 1 ? event.key.toLowerCase() : event.key;
    konamiAt = key === KONAMI[konamiAt] ? konamiAt + 1 : key === KONAMI[0] ? 1 : 0;
    if (konamiAt < KONAMI.length) return;
    konamiAt = 0;

    try { localStorage.setItem('pixos.cheat', 'lives'); } catch { /* storage may be blocked */ }
    sfx('achievement');
    status.textContent = 'Cheat enabled: +30 lives. In the game, not at work.';
    status.classList.remove('gone');
    setTimeout(() => { if (state.mode === 'room') status.classList.add('gone'); }, 5000);

    const wasOn = isLampOn();
    for (let i = 0; i < 7; i++) setTimeout(() => setLamp(i === 6 ? wasOn : i % 2 === 0 ? !wasOn : wasOn), i * 110);

    wakeDog(true);
    setTimeout(() => { if (!(treatsOut())) wakeDog(false); }, 4500);
});
