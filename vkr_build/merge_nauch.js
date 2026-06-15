/* Сливает правки научного руководителя (nauch_text.txt) в vkr_text.txt:
 * тело берётся из его версии (со всеми правками), таблицы заменяются
 * маркерами, тире/опечатки нормализуются, список источников — мой (19). */
const fs = require("fs");
const path = require("path");

const FILE = path.join(__dirname, "..", "vkr_text.txt");
const HEAD = "СПИСОК ИСПОЛЬЗОВАННЫХ ИСТОЧНИКОВ";

// --- мои 19 источников из текущего vkr_text.txt (тире -> en, как у науч.) ---
function currentRefs() {
  const raw = fs.readFileSync(FILE, "utf8").replace(/^﻿/, "");
  const lines = raw.split(/\r?\n/).map(s => s.trim()).filter(s => s !== "");
  const i = lines.findIndex(l => l === HEAD);
  return lines.slice(i + 1).map(s => s.replace(/—/g, "–"));
}
const REFS = currentRefs();

// --- тело из версии руководителя ---
let body = fs.readFileSync(path.join(__dirname, "nauch_text.txt"), "utf8")
  .replace(/^﻿/, "").split(/\r?\n/).map(s => s.trim()).filter(s => s !== "");
const a = body.indexOf("ВВЕДЕНИЕ");
const b = body.indexOf(HEAD);
body = body.slice(a, b);

// --- замена блоков таблиц (подпись + ячейки) маркерами ---
function replaceTable(arr, captionRe, cellCount, marker) {
  const idx = arr.findIndex(l => captionRe.test(l));
  if (idx < 0) { console.error("table caption not found:", captionRe); return arr; }
  const del = new Set();
  del.add(idx);
  for (let k = 1; k <= cellCount; k++) del.add(idx + k);
  const out = [];
  arr.forEach((l, k) => {
    if (k === idx) out.push(marker);
    else if (!del.has(k)) out.push(l);
  });
  return out;
}
body = replaceTable(body, /^Таблица 1\.1\s/, 30, "@@T1@@");
body = replaceTable(body, /^Таблица 1\.2\s/, 30, "@@T2@@");
body = replaceTable(body, /^Таблица 4\.1\s/, 20, "@@T3@@");

// --- нормализация: тире, регистр «Где», опечатка «n4» ---
body = body.map(l => l
  .replace(/—/g, "–")       // em-dash -> en-dash (выбор науч. рук.)
  .replace(/^Где$/, "где")
  .replace(/оборотов n4$/, "оборотов n;"));

// --- сборка нового vkr_text.txt ---
const out = ["СОДЕРЖАНИЕ", ...body, HEAD, ...REFS].join("\r\n") + "\r\n";
fs.writeFileSync(FILE, out, "utf8");
console.log("merged. body lines:", body.length, "refs:", REFS.length,
  "| markers:", body.filter(l => /^@@T/.test(l)).length);
