/* Сравнивает тело моей текущей версии (vkr_text.txt) с версией научного
 * руководителя (nauch_text.txt) и печатает правки. */
const fs = require("fs");
const path = require("path");

function load(file, startKey, endKey) {
  const raw = fs.readFileSync(path.join(__dirname, file), "utf8").replace(/^﻿/, "");
  const all = raw.split(/\r?\n/).map(s => s.trim()).filter(s => s !== "");
  const a = all.findIndex(l => l === startKey);
  const b = all.findIndex(l => l === endKey);
  return all.slice(a, b < 0 ? undefined : b);
}

const A = load("../vkr_text.txt", "ВВЕДЕНИЕ", "СПИСОК ИСПОЛЬЗОВАННЫХ ИСТОЧНИКОВ");
const B = load("nauch_text.txt", "ВВЕДЕНИЕ", "СПИСОК ИСПОЛЬЗОВАННЫХ ИСТОЧНИКОВ");

// нормализация для отделения существенных правок от «тире/пробелы»
const norm = s => s.replace(/[‒–—―-]/g, "-").replace(/\s+/g, " ").trim();

// LCS по нормализованным строкам
const n = A.length, m = B.length;
const dp = Array.from({ length: n + 1 }, () => new Int32Array(m + 1));
for (let i = n - 1; i >= 0; i--)
  for (let j = m - 1; j >= 0; j--)
    dp[i][j] = norm(A[i]) === norm(B[j]) ? dp[i + 1][j + 1] + 1
      : Math.max(dp[i + 1][j], dp[i][j + 1]);

const out = [];
let i = 0, j = 0;
while (i < n && j < m) {
  if (norm(A[i]) === norm(B[j])) {
    if (A[i] !== B[j]) out.push("~ СТИЛЬ (тире/пробел):\n  было: " + A[i] + "\n  стало: " + B[j]);
    i++; j++;
  } else if (dp[i + 1][j] >= dp[i][j + 1]) {
    out.push("- УДАЛЕНО (моё, нет у науч.):\n  " + A[i]); i++;
  } else {
    out.push("+ ДОБАВЛЕНО/ИЗМЕНЕНО науч.:\n  " + B[j]); j++;
  }
}
while (i < n) { out.push("- УДАЛЕНО (моё):\n  " + A[i]); i++; }
while (j < m) { out.push("+ ДОБАВЛЕНО науч.:\n  " + B[j]); j++; }

fs.writeFileSync(path.join(__dirname, "diff_report.txt"), out.join("\n\n"), "utf8");
const styleOnly = out.filter(x => x.startsWith("~")).length;
const subst = out.length - styleOnly;
console.log("A(my):", n, "B(nauch):", m, "| строк-различий:", out.length,
  "| стиль(тире):", styleOnly, "| существенных:", subst);
