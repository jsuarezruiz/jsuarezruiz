// Every sound in the room and the OS, made with the Web Audio API: no audio files to download.
// Nothing plays until the visitor's first click or key press, which is when browsers allow sound.

const KEY = 'pixos.sound';

let context = null;
let master = null;
let on = true;
try { on = localStorage.getItem(KEY) !== 'off'; } catch { /* storage may be blocked */ }

function audio() {
    if (!context) {
        context = new AudioContext();
        master = context.createGain();
        master.gain.value = on ? 0.5 : 0;
        master.connect(context.destination);
    }
    if (context.state === 'suspended') context.resume();
    return context;
}

// One note: frequency glides from `from` to `to`, and the volume fades out.
function tone(from, to, at, length, { type = 'square', volume = 0.15 } = {}) {
    const ctx = audio();
    const start = ctx.currentTime + at;
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(from, start);
    if (to !== from) osc.frequency.exponentialRampToValueAtTime(to, start + length);
    gain.gain.setValueAtTime(volume, start);
    gain.gain.exponentialRampToValueAtTime(0.0001, start + length);
    osc.connect(gain).connect(master);
    osc.start(start);
    osc.stop(start + length + 0.02);
}

// A burst of filtered noise that swells and fades: breath, thuds, snores.
function noise(at, length, { volume = 0.2, cutoff = 400, attack = 0.3 } = {}) {
    const ctx = audio();
    const start = ctx.currentTime + at;
    const buffer = ctx.createBuffer(1, Math.ceil(ctx.sampleRate * length), ctx.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < data.length; i++) data[i] = Math.random() * 2 - 1;

    const source = ctx.createBufferSource();
    source.buffer = buffer;
    const filter = ctx.createBiquadFilter();
    filter.type = 'lowpass';
    filter.frequency.value = cutoff;
    const gain = ctx.createGain();
    gain.gain.setValueAtTime(0.0001, start);
    gain.gain.linearRampToValueAtTime(volume, start + length * attack);
    gain.gain.exponentialRampToValueAtTime(0.0001, start + length);
    source.connect(filter).connect(gain).connect(master);
    source.start(start);
}

const melody = (notes, step, options) => notes.forEach((note, i) => tone(note, note, i * step, step * 1.6, options));

const SOUNDS = {
    click: () => tone(1100, 900, 0, 0.035, { volume: 0.07 }),
    open: () => { tone(520, 520, 0, 0.06); tone(780, 780, 0.06, 0.09); },
    close: () => { tone(780, 780, 0, 0.06); tone(520, 520, 0.06, 0.09); },
    beep: () => tone(880, 880, 0, 0.14, { volume: 0.1 }),
    chime: () => melody([523, 659, 784, 1047], 0.11, { type: 'triangle', volume: 0.22 }),
    error: () => { tone(180, 150, 0, 0.45, { type: 'sawtooth', volume: 0.2 }); tone(120, 100, 0.05, 0.5, { type: 'sawtooth', volume: 0.15 }); },
    achievement: () => melody([659, 784, 1047, 1319], 0.07, { volume: 0.12 }),
    notify: () => { tone(880, 880, 0, 0.12, { type: 'sine', volume: 0.2 }); tone(1175, 1175, 0.12, 0.22, { type: 'sine', volume: 0.2 }); },
    jump: () => tone(380, 720, 0, 0.09, { volume: 0.08 }),
    catch: () => tone(600, 1300, 0, 0.1, { volume: 0.1 }),
    gem: () => { tone(1320, 1320, 0, 0.07, { type: 'sine', volume: 0.15 }); tone(1760, 1760, 0.07, 0.14, { type: 'sine', volume: 0.15 }); },
    fail: () => tone(320, 90, 0, 0.35, { type: 'sawtooth', volume: 0.15 }),
    squeak: () => { tone(900, 1900, 0, 0.09, { type: 'sine', volume: 0.2 }); tone(1900, 1000, 0.09, 0.12, { type: 'sine', volume: 0.2 }); },
    switch: () => noise(0, 0.05, { volume: 0.25, cutoff: 2500, attack: 0.05 }),
    snort: () => noise(0, 0.35, { volume: 0.3, cutoff: 500, attack: 0.2 }),
    paper: () => noise(0, 0.18, { volume: 0.12, cutoff: 4000, attack: 0.3 }),
    'ring-start': () => ring(true),
    'ring-stop': () => ring(false),
};

// The phone's ringtone: a two-note trill, repeated until the call ends.
let ringing = null;
function ring(start) {
    clearInterval(ringing);
    ringing = null;
    if (!start) return;
    const trill = () => { for (let i = 0; i < 8; i++) tone(i % 2 ? 1400 : 950, i % 2 ? 1400 : 950, i * 0.09, 0.08, { type: 'sine', volume: 0.16 }); };
    trill();
    ringing = setInterval(trill, 1900);
}

// The bulldog snores for as long as the room is in view.
let snoring = null;
function snore(start) {
    clearInterval(snoring);
    snoring = null;
    if (!start) return;
    const breath = () => {
        if (!context) return; // not before the first click
        noise(0, 1.5, { volume: 0.07, cutoff: 260, attack: 0.6 });   // in
        noise(1.9, 1.3, { volume: 0.035, cutoff: 700, attack: 0.3 }); // out
    };
    snoring = setInterval(breath, 4200);
}

window.pixsound = {
    get on() { return on; },
    play(name) { if (on) SOUNDS[name]?.(); },
    snore,
    toggle() {
        on = !on;
        try { localStorage.setItem(KEY, on ? 'on' : 'off'); } catch { /* storage may be blocked */ }
        if (on) audio();
        if (master) master.gain.value = on ? 0.5 : 0;
        if (!on) ring(false);
        window.dispatchEvent(new CustomEvent('pixsound:changed'));
        return on;
    },
};

// The room's own switch, for when the laptop (which has its own in the taskbar) is not in front.
const button = document.getElementById('sound');
const label = () => { button.textContent = on ? 'Sound on' : 'Sound off'; button.setAttribute('aria-pressed', String(on)); };
button.addEventListener('click', () => { window.pixsound.toggle(); window.pixsound.play('click'); });
addEventListener('pixsound:changed', label);
label();
