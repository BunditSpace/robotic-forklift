const fs = require('node:fs');
const path = require('node:path');

const filePath = path.join(__dirname, '..', 'node_modules', 'typescript', 'lib', 'typescript.js');

if (!fs.existsSync(filePath)) {
  console.warn('[postinstall] TypeScript runtime not found, skipping patch.');
  process.exit(0);
}

const source = fs.readFileSync(filePath, 'utf8');

// Patch case 4 (ReferenceFile)
const target4 = `    case 4 /* ReferenceFile */:\n      ({ pos, end } = file.referencedFiles[index]);\n      break;`;
const replacement4 = `    case 4 /* ReferenceFile */:\n      if (file.referencedFiles[index]) {\n        ({ pos, end } = file.referencedFiles[index]);\n      } else {\n        pos = end = 0;\n      }\n      break;`;

// Patch case 5 (TypeReferenceDirective)
const target5line1 = `      ({ pos, end } = file.typeReferenceDirectives[index]);`;
const replacement5line1 = `      if (file.typeReferenceDirectives[index]) { ({ pos, end } = file.typeReferenceDirectives[index]); } else { pos = end = 0; }`;

// Patch case 7 (LibReferenceDirective)
const target7 = `    case 7 /* LibReferenceDirective */:\n      ({ pos, end } = file.libReferenceDirectives[index]);\n      break;`;
const replacement7 = `    case 7 /* LibReferenceDirective */:\n      if (file.libReferenceDirectives[index]) {\n        ({ pos, end } = file.libReferenceDirectives[index]);\n      } else {\n        pos = end = 0;\n      }\n      break;`;

const alreadyPatched4 = source.includes(replacement4);
const alreadyPatched7 = source.includes(replacement7);

if (alreadyPatched4 && alreadyPatched7) {
  console.log('[postinstall] TypeScript reference-file guards already applied.');
  process.exit(0);
}

let patched = source;

if (!alreadyPatched4) {
  if (!patched.includes(target4)) {
    console.warn('[postinstall] case 4 snippet not found, skipping that guard.');
  } else {
    patched = patched.replace(target4, replacement4);
    console.log('[postinstall] Applied case 4 (ReferenceFile) guard.');
  }
}

if (!alreadyPatched7) {
  if (!patched.includes(target7)) {
    console.warn('[postinstall] case 7 snippet not found, skipping that guard.');
  } else {
    patched = patched.replace(target7, replacement7);
    console.log('[postinstall] Applied case 7 (LibReferenceDirective) guard.');
  }
}

const replaced5 = patched.includes(target5line1);
if (replaced5) {
  patched = patched.replace(target5line1, replacement5line1);
  console.log('[postinstall] Applied case 5 (TypeReferenceDirective) guard.');
}

if (patched !== source) {
  fs.writeFileSync(filePath, patched, 'utf8');
  console.log('[postinstall] Patched TypeScript runtime guards successfully.');
} else {
  console.log('[postinstall] No changes needed.');
}
