// Checks the architecture rules of the web app (run: node scripts/check-module-boundaries.mjs):
//  - a module imports only from `shared` and from itself, never from another module;
//  - `shared` imports nothing from the modules.
// The host (src/app/*.ts, src/app/admin) may import modules, but only through their `index.ts`.
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';

const root = resolve('src/app');
const errors = [];

function files(dir) {
  return readdirSync(dir).flatMap((f) => {
    const p = join(dir, f);
    return statSync(p).isDirectory() ? files(p) : p.endsWith('.ts') ? [p] : [];
  });
}

function area(path) {
  const parts = relative(root, path).split(sep);
  if (parts[0] === 'modules') return `modules/${parts[1]}`;
  if (parts[0] === 'shared') return 'shared';
  return 'host';
}

for (const file of files(root)) {
  const from = area(file);
  const imports = [...readFileSync(file, 'utf8').matchAll(/from\s+'(\.[^']*)'/g)].map((m) => m[1]);
  for (const spec of imports) {
    const target = resolve(dirname(file), spec);
    const to = area(target);
    const where = `${relative(root, file)} -> ${spec}`;
    if (to === from) continue;
    if (from.startsWith('modules/') && to !== 'shared') errors.push(`module imports another area: ${where}`);
    if (from === 'shared' && to !== 'shared') errors.push(`shared imports a module: ${where}`);
    if (from === 'host' && to.startsWith('modules/') && relative(root, target) !== join('modules', to.split('/')[1])) {
      errors.push(`host must import a module through its index.ts: ${where}`);
    }
  }
}

if (errors.length) {
  console.error(errors.join('\n'));
  process.exit(1);
}
console.log('Module boundaries OK');
