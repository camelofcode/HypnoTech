import { promises as fs, watch } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import browserslist from 'browserslist';
import { browserslistToTargets, transform as transformCss } from 'lightningcss';
import * as sass from 'sass';
import { minify } from 'terser';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const browserTargets = browserslistToTargets(browserslist(undefined, { path: root }));
const args = new Set(process.argv.slice(2));
const checkOnly = args.has('--check');
const watchMode = args.has('--watch');

const projects = {
  demo: {
    source: 'examples/HypnoTech.Demo.Website.Frontend',
    output: 'examples/HypnoTech.Demo.Website/wwwroot/js',
    bundle: 'site',
    stylesheet: {
      input: 'scss/site.scss',
      output: 'examples/HypnoTech.Demo.Website/wwwroot/css/site'
    },
    scripts: [
      'js/site.js'
    ]
  },
  frontend: {
    source: 'src/Frontend/HypnoTech.Frontend.Frontend',
    output: 'src/Frontend/HypnoTech.Frontend/wwwroot/js',
    bundle: 'hypnotech.frontend',
    scripts: [
      { dependency: 'jquery/dist/jquery.js' },
      'lib/navigo/navigo.min.js',
      'js/utilities.js',
      'js/events.js',
      'js/ajax.js',
      'js/site.js'
    ]
  },
  media: {
    source: 'src/Media/HypnoTech.Media.Frontend.Frontend',
    output: 'src/Media/HypnoTech.Media.Frontend/wwwroot/js',
    bundle: 'hypnotech.media',
    scripts: [
      { dependency: 'knockout/build/output/knockout-latest.js' },
      'lib/knockout/knockout.validation.js',
      'js/player.js'
    ]
  },
  ecommerce: {
    source: 'src/Ecommerce/HypnoTech.Ecommerce.Frontend.Frontend',
    output: 'src/Ecommerce/HypnoTech.Ecommerce.Frontend/wwwroot/js',
    bundle: 'hypnotech.ecommerce',
    scripts: [
      'lib/cookie-js/Cookie.js',
      { dependency: 'knockout/build/output/knockout-latest.js' },
      'lib/knockout/knockout.validation.js',
      'js/cart.js',
      'js/checkout.js'
    ]
  },
  cms: {
    source: 'src/Cms/HypnoTech.Cms.Frontend',
    output: 'src/Cms/HypnoTech.Cms/wwwroot/js',
    bundle: 'script',
    stylesheet: {
      input: 'scss/style.scss',
      output: 'src/Cms/HypnoTech.Cms/wwwroot/css/style'
    },
    scripts: [
      { dependency: 'jquery/dist/jquery.js' },
      'lib/bootstrap/popper.min.js',
      'lib/bootstrap/dist/js/bootstrap.js',
      { dependency: 'knockout/build/output/knockout-latest.js' },
      'lib/lodash/lodash.js',
      'lib/cookie-js/Cookie.js',
      'lib/tinymce/wysiwyg.js',
      'js/tinymceInit.js',
      { directory: 'js/pages', modularise: true },
      'js/site.js',
      'js/ajax.js',
      'js/validation.js',
      'js/messages.js',
      'js/cmsList.js',
      'js/features.js',
      'js/types.js',
      'js/config.js',
      'js/CmsViewModels/CheckBoxInputViewModel.js',
      'js/CmsViewModels/FileUploadViewModel.js',
      'js/CmsViewModels/ImageUploadViewModel.js',
      'js/CmsViewModels/ko.bindingHandlers.fileUpload.js',
      'js/CmsViewModels/ListInputViewModel.js',
      'js/CmsViewModels/NestedSelectListViewModel.js',
      'js/CmsViewModels/PhotoGalleryViewModel.js',
      'js/CmsViewModels/YouTubeViewModel.js',
      'js/tinymce.plugins.imageUpload.js',
      'js/tinymce.plugins.youTube.js'
    ]
  }
};

function selectedProjects() {
  if (args.has('--all')) return Object.entries(projects);
  const projectArgument = process.argv.indexOf('--project');
  const projectName = projectArgument === -1 ? undefined : process.argv[projectArgument + 1];
  if (!projectName || !projects[projectName]) {
    throw new Error(`Use --all or --project <${Object.keys(projects).join('|')}>.`);
  }
  return [[projectName, projects[projectName]]];
}

async function listJavaScriptFiles(directory) {
  let entries;
  try {
    entries = await fs.readdir(directory, { withFileTypes: true });
  } catch (error) {
    if (error.code === 'ENOENT') return [];
    throw error;
  }
  entries.sort((left, right) => left.name.localeCompare(right.name, 'en'));
  const files = [];
  for (const entry of entries) {
    const location = path.join(directory, entry.name);
    if (entry.isDirectory()) files.push(...await listJavaScriptFiles(location));
    else if (entry.isFile() && entry.name.endsWith('.js')) files.push(location);
  }
  return files;
}

function modularisePageScript(content, file) {
  const name = path.basename(file, '.js');
  const indented = content.replace(/\r\n|\n|\r/g, '\r\n            ');
  return `(function(root, factory) {\r\n\t'use strict';\r\n\tif (typeof define === 'function' && define.amd) {\r\n\t\tdefine([], factory);\r\n\t} else if (typeof module === 'object' && module.exports) {\r\n\t\tmodule.exports = factory();\r\n\t} else {\r\n\t\troot.${name} = factory();\r\n\t}\r\n}(this, function() {\r\n\t'use strict';\r\n\treturn {\r\n\t\texecute: function() {\r\n            ${indented}\r\n\t\t}\r\n\t};\r\n}));`;
}

async function readText(file) {
  return (await fs.readFile(file, 'utf8')).replace(/^\uFEFF/, '');
}

async function collectScripts(project) {
  const source = path.join(root, project.source);
  const parts = [];
  for (const entry of project.scripts) {
    if (typeof entry === 'string') {
      parts.push(await readText(path.join(source, entry)));
      continue;
    }
    if (entry.dependency) {
      parts.push(await readText(path.join(root, 'node_modules', entry.dependency)));
      continue;
    }
    for (const file of await listJavaScriptFiles(path.join(source, entry.directory))) {
      const content = await readText(file);
      parts.push(entry.modularise ? modularisePageScript(content, file) : content);
    }
  }
  return parts.join('\n');
}

async function createMinifiedJavaScript(code) {
  const result = await minify(code, {
    compress: { ecma: 5 },
    mangle: true,
    format: { comments: 'some', ecma: 5 }
  });
  if (!result.code) throw new Error('Terser produced an empty bundle.');
  return result.code;
}

async function createStylesheet(project) {
  const input = path.join(root, project.source, project.stylesheet.input);
  const result = sass.compile(input, {
    style: 'expanded',
    quietDeps: true,
    // Bootstrap 4 and Font Awesome 5 are vendored legacy Sass; keep their
    // deprecations from obscuring warnings in HypnoTech's own source.
    silenceDeprecations: ['import', 'global-builtin', 'color-functions', 'if-function', 'slash-div']
  });
  const cleanCss = result.css.replace(/[ \t]+$/gm, '');
  const expanded = cleanCss.endsWith('\n') ? cleanCss : `${cleanCss}\n`;
  const minified = transformCss({
    filename: input,
    code: Buffer.from(expanded),
    minify: true,
    targets: browserTargets
  }).code.toString();
  return { expanded, minified: minified.endsWith('\n') ? minified : `${minified}\n` };
}

async function emit(file, content, differences) {
  let current;
  try {
    current = await fs.readFile(file);
  } catch (error) {
    if (error.code !== 'ENOENT') throw error;
  }
  const next = Buffer.isBuffer(content) ? content : Buffer.from(content);
  if (current?.equals(next)) return;
  differences.push(path.relative(root, file));
  if (!checkOnly) {
    await fs.mkdir(path.dirname(file), { recursive: true });
    await fs.writeFile(file, next);
  }
}

async function buildProject(name, project) {
  const differences = [];
  const javascript = await collectScripts(project);
  const output = path.join(root, project.output);
  await emit(path.join(output, `${project.bundle}.js`), javascript, differences);
  await emit(path.join(output, `${project.bundle}.min.js`), await createMinifiedJavaScript(javascript), differences);
  if (project.stylesheet) {
    const stylesheet = await createStylesheet(project);
    const cssOutput = path.join(root, project.stylesheet.output);
    await emit(`${cssOutput}.css`, stylesheet.expanded, differences);
    await emit(`${cssOutput}.min.css`, stylesheet.minified, differences);
  }
  if (checkOnly && differences.length) {
    throw new Error(`${name} has stale generated assets:\n  ${differences.join('\n  ')}`);
  }
  console.log(`${name}: ${differences.length ? `updated ${differences.length} asset(s)` : 'up to date'}`);
}

const chosenProjects = selectedProjects();
for (const [name, project] of chosenProjects) await buildProject(name, project);

if (watchMode) {
  if (checkOnly) throw new Error('--watch and --check cannot be combined.');
  let timer;
  let building = false;
  let pending = false;
  const rebuild = async () => {
    if (building) {
      pending = true;
      return;
    }
    building = true;
    try {
      for (const [name, project] of chosenProjects) await buildProject(name, project);
    } catch (error) {
      console.error(error);
    } finally {
      building = false;
      if (pending) {
        pending = false;
        await rebuild();
      }
    }
  };
  for (const [, project] of chosenProjects) {
    watch(path.join(root, project.source), { recursive: true }, (_event, filename) => {
      if (filename?.includes('node_modules')) return;
      clearTimeout(timer);
      timer = setTimeout(rebuild, 150);
    });
  }
  console.log(`Watching ${chosenProjects.map(([name]) => name).join(', ')}...`);
}
