import { dotnet } from './_framework/dotnet.js';

// The room (room.js) and the OS (Avalonia) talk through DOM events and this object.
const os = window.pixos = { ready: false, setMode: () => {}, boot: () => {}, openApp: () => {} };
const emit = (name) => window.dispatchEvent(new CustomEvent(name));

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

dotnetRuntime.setModuleImports('main.js', {
    standUp: () => emit('pixos:standup'),
    trophy: () => emit('pixos:trophy'),
    playSound: (name) => window.pixsound?.play(name),
    soundOn: () => window.pixsound?.on ?? false,
    toggleSound: () => window.pixsound?.toggle() ?? false,
    // Small values kept between visits. Storage can be blocked, so both fail quietly.
    readSetting: (key) => { try { return localStorage.getItem('pixos.' + key) ?? ''; } catch { return ''; } },
    writeSetting: (key, value) => { try { localStorage.setItem('pixos.' + key, value); } catch { /* blocked */ } },
    openPhoneApp: (url) => window.dispatchEvent(new CustomEvent('pixos:phoneapp', { detail: url })),
    ready: () => { os.ready = true; emit('pixos:ready'); },
});

const config = dotnetRuntime.getConfig();
const exports = await dotnetRuntime.getAssemblyExports(config.mainAssemblyName);
os.setMode = (mode) => exports.Portfolio.Browser.Interop.SetMode(mode);
os.boot = () => exports.Portfolio.Browser.Interop.Boot();
os.openApp = (id) => exports.Portfolio.Browser.Interop.OpenApp(id);
// Debug builds only: plays the boss fight with a bot and returns its report.
if (exports.Portfolio.Browser.Interop.TestBoss) os.testBoss = () => exports.Portfolio.Browser.Interop.TestBoss();

await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
