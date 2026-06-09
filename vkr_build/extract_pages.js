/* Извлекает номер страницы каждого заголовка из текстового дампа PDF
 * (pages.txt, страницы разделены символом \f) и пишет toc_pages.json. */
const fs = require("fs");
const path = require("path");
const ENTRIES = require("./toc_entries");

const dump = fs.readFileSync(path.join(__dirname, "pages.txt"), "utf8");
const pages = dump.split("\f").map(p => p.split(/\r?\n/).map(s => s.trim()));

// страница «ВВЕДЕНИЕ» в теле — точка отсчёта, чтобы не цеплять содержание
let introPage = 0;
for (let i = 0; i < pages.length; i++) {
  if (pages[i].some(l => l === "ВВЕДЕНИЕ")) { introPage = i; break; }
}

const result = {};
for (const [, , key] of ENTRIES) {
  let found = null;
  for (let i = introPage; i < pages.length; i++) {
    if (pages[i].some(l => l.startsWith(key))) { found = i + 1; break; } // +1: 1-based
  }
  if (found == null) console.error("NOT FOUND:", key);
  result[key] = found;
}
fs.writeFileSync(path.join(__dirname, "toc_pages.json"), JSON.stringify(result, null, 2));
console.log("toc_pages.json written; introPage(idx)=", introPage);
console.log(result);
