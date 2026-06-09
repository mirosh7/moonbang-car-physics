/* Генератор ВКР по ГОСТ 7.32-2017.
 * Читает извлечённый текст (../vkr_text.txt) и исходный код проекта,
 * собирает итоговый .docx с титулом, содержанием, телом и приложениями. */
const fs = require("fs");
const path = require("path");
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  AlignmentType, LevelFormat, HeadingLevel, BorderStyle,
  WidthType, PageNumber, TabStopType, LeaderType,
  VerticalAlign, Header, Footer,
} = require("docx");

const ROOT = path.join(__dirname, "..");
const FONT = "Times New Roman";
const MONO = "Courier New";
const SZ = 28;            // 14 pt
const SZ_TABLE = 24;      // 12 pt
const SZ_CODE = 18;       // 9 pt
const INDENT = 709;       // 1.25 cm
const LINE = 360;         // 1.5 интервал
const CW = 9355;          // ширина набора (A4 портрет, поля 30/15)

/* ---------- лёгкая литературная правка исходного текста ---------- */
const REPLACEMENTS = [
  ["Моделирование динамики автомобиля является одной из ключевых задач в широком спектре прикладных областей",
   "Поведение автомобиля на дороге, привычное и интуитивно понятное любому водителю, в действительности рождается из сложного взаимодействия множества физических процессов, сосредоточенных в небольшом пятне контакта шины с дорожным полотном. Моделирование динамики автомобиля является одной из центральных задач в широком спектре прикладных областей"],
  ["процессора Ryzen 7 700", "процессора AMD Ryzen 7 7700"],
  ["приведено в таблице 1.2", "приведено в таблице 1.1"],
  ["(таблица 1.1)", "(таблица 1.2)"],
];
function improve(line) {
  let s = line;
  for (const [a, b] of REPLACEMENTS) s = s.split(a).join(b);
  return s;
}

/* ---------- помощники абзацев ---------- */
function body(text) {
  const noIndent = text.startsWith("где ") || text.startsWith("где,");
  return new Paragraph({
    alignment: AlignmentType.JUSTIFIED,
    spacing: { line: LINE, after: 0 },
    indent: { firstLine: noIndent ? 0 : INDENT },
    children: [new TextRun({ text, font: FONT, size: SZ })],
  });
}

function formula(expr, num) {
  return new Paragraph({
    spacing: { line: LINE, before: 120, after: 120 },
    tabStops: [
      { type: TabStopType.CENTER, position: Math.round(CW / 2) },
      { type: TabStopType.RIGHT, position: CW },
    ],
    children: [
      new TextRun({ text: "\t" + expr + "\t" + num, font: FONT, size: SZ }),
    ],
  });
}

function h1struct(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_1,
    alignment: AlignmentType.CENTER,
    pageBreakBefore: true,
    spacing: { line: LINE, before: 0, after: 240 },
    children: [new TextRun({ text: text.toUpperCase(), font: FONT, size: SZ, bold: true })],
  });
}

function h1chapter(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_1,
    alignment: AlignmentType.LEFT,
    pageBreakBefore: true,
    spacing: { line: LINE, before: 0, after: 240 },
    indent: { firstLine: INDENT },
    children: [new TextRun({ text, font: FONT, size: SZ, bold: true })],
  });
}

function h2(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_2,
    alignment: AlignmentType.LEFT,
    spacing: { line: LINE, before: 240, after: 120 },
    indent: { firstLine: INDENT },
    children: [new TextRun({ text, font: FONT, size: SZ, bold: true })],
  });
}

function center(text, opts = {}) {
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    spacing: { line: LINE, after: opts.after ?? 0, before: opts.before ?? 0 },
    children: [new TextRun({ text, font: FONT, size: opts.size ?? SZ, bold: !!opts.bold, italics: !!opts.italics })],
  });
}

function dashItem(text) {
  return new Paragraph({
    alignment: AlignmentType.JUSTIFIED,
    spacing: { line: LINE, after: 0 },
    numbering: { reference: "dashes", level: 0 },
    children: [new TextRun({ text, font: FONT, size: SZ })],
  });
}

function refItem(text) {
  return new Paragraph({
    alignment: AlignmentType.JUSTIFIED,
    spacing: { line: LINE, after: 0 },
    numbering: { reference: "refs", level: 0 },
    children: [new TextRun({ text, font: FONT, size: SZ })],
  });
}

/* ---------- таблицы ---------- */
const BORDER = { style: BorderStyle.SINGLE, size: 4, color: "000000" };
const BORDERS = { top: BORDER, bottom: BORDER, left: BORDER, right: BORDER };

function cell(text, w, opts = {}) {
  return new TableCell({
    width: { size: w, type: WidthType.DXA },
    borders: BORDERS,
    verticalAlign: VerticalAlign.CENTER,
    margins: { top: 40, bottom: 40, left: 100, right: 100 },
    children: [new Paragraph({
      alignment: opts.left ? AlignmentType.LEFT : AlignmentType.CENTER,
      spacing: { line: 240, after: 0 },
      children: [new TextRun({ text, font: FONT, size: SZ_TABLE, bold: !!opts.bold })],
    })],
  });
}

function gostTable(caption, headers, rows, widths) {
  const head = new TableRow({
    tableHeader: true,
    children: headers.map((h, i) => cell(h, widths[i], { bold: true })),
  });
  const bodyRows = rows.map(r => new TableRow({
    children: r.map((c, i) => cell(c, widths[i], { left: i === 0 })),
  }));
  return [
    new Paragraph({
      alignment: AlignmentType.LEFT,
      spacing: { line: LINE, before: 120, after: 60 },
      children: [new TextRun({ text: caption, font: FONT, size: SZ })],
    }),
    new Table({
      width: { size: CW, type: WidthType.DXA },
      columnWidths: widths,
      rows: [head, ...bodyRows],
    }),
    new Paragraph({ spacing: { after: 120 }, children: [] }),
  ];
}

const TABLES = {
  "Признак": gostTable(
    "Таблица 1.1 — Сопоставление программных решений автомобильной физики",
    ["Признак", "VPP", "PhysX", "Chrono", "Модуль"],
    [
      ["Открытый код", "нет", "частично", "да", "да"],
      ["Модель Пасейки", "да", "нет", "да", "да"],
      ["Независимость от движка", "нет", "частично", "да", "да"],
      ["Кроссплатформенность", "огранич.", "да", "да", "да"],
      ["Реальное время", "да", "да", "нет", "да"],
    ],
    [3355, 1500, 1500, 1500, 1500]),
  "Критерий": gostTable(
    "Таблица 1.2 — Сравнение математических моделей покрышки",
    ["Критерий", "Пасейка", "Щёточная", "Фиала", "Дугофф"],
    [
      ["Точность", "высокая", "средняя", "средняя", "средняя"],
      ["Вычисл. сложность", "низкая", "средняя", "низкая", "низкая"],
      ["Комбинир. нагружение", "да", "да", "частично", "да"],
      ["Физ. интерпретируемость", "нет", "высокая", "средняя", "средняя"],
      ["Требования к данным", "высокие", "низкие", "низкие", "низкие"],
    ],
    [2755, 1650, 1650, 1650, 1650]),
  "Частота, Гц": gostTable(
    "Таблица 4.1 — Затраты времени на расчёт физики при различных частотах обновления",
    ["Частота, Гц", "Шаг, мс", "Время расчёта, мс", "Доля кадра, %"],
    [
      ["100", "10", "≈ 0,005", "0,03"],
      ["1 000", "1", "≈ 0,018", "0,11"],
      ["10 000", "0,1", "≈ 0,22", "1,3"],
      ["20 000", "0,05", "≈ 0,56", "3,5"],
    ],
    [2339, 2339, 2339, 2338]),
};
const TABLE_SKIP_UNTIL = {
  "Признак": "Таблица 1.2",
  "Критерий": "Таблица 1.1",
  "Частота, Гц": "Таблица 4.1",
};

/* ---------- титульный лист ---------- */
function tline(text, opts = {}) {
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    spacing: { line: 240, after: opts.after ?? 0, before: opts.before ?? 0 },
    children: [new TextRun({ text, font: FONT, size: opts.size ?? SZ, bold: !!opts.bold, allCaps: !!opts.caps })],
  });
}
function sigLine(label, name) {
  return new Paragraph({
    spacing: { line: 240, before: 160 },
    tabStops: [{ type: TabStopType.RIGHT, position: CW }],
    children: [new TextRun({ text: label + "\t" + name, font: FONT, size: SZ })],
  });
}
function titlePage() {
  return [
    tline("Министерство науки и высшего образования Российской Федерации"),
    tline("Федеральное государственное бюджетное образовательное учреждение"),
    tline("высшего образования"),
    tline("«Кубанский государственный университет»", { bold: true, after: 120 }),
    tline("Факультет компьютерных технологий и прикладной математики"),
    tline("Кафедра прикладной математики", { after: 480 }),
    tline("ВЫПУСКНАЯ КВАЛИФИКАЦИОННАЯ РАБОТА", { bold: true }),
    tline("(МАГИСТЕРСКАЯ ДИССЕРТАЦИЯ)", { bold: true, after: 360 }),
    tline("РАЗРАБОТКА КРОСС-ПЛАТФОРМЕННОГО МОДУЛЯ", { bold: true }),
    tline("АВТОМОБИЛЬНОЙ СИМУЛЯЦИИ НА ОСНОВЕ", { bold: true }),
    tline("ДИНАМИЧЕСКОЙ БИБЛИОТЕКИ C++", { bold: true, after: 480 }),
    sigLine("Работу выполнил _________________________", "А.А. Мирошниченко"),
    new Paragraph({ spacing: { line: 240 }, children: [new TextRun({ text: "Направление подготовки 01.04.02 Прикладная математика и информатика", font: FONT, size: SZ })] }),
    sigLine("Научный руководитель", ""),
    new Paragraph({ spacing: { line: 240 }, children: [new TextRun({ text: "_________________________________________________", font: FONT, size: SZ })] }),
    sigLine("Нормоконтролёр", ""),
    new Paragraph({ spacing: { line: 240, after: 720 }, children: [new TextRun({ text: "_________________________________________________", font: FONT, size: SZ })] }),
    tline("Краснодар", { before: 480 }),
    tline("2025"),
  ];
}

/* ---------- содержание (статическое, с реальными номерами страниц) ---------- */
const TOC_ENTRIES = require("./toc_entries");

function loadTocPages() {
  const p = path.join(__dirname, "toc_pages.json");
  return fs.existsSync(p) ? JSON.parse(fs.readFileSync(p, "utf8")) : {};
}

function tocBlock() {
  const pages = loadTocPages();
  const out = [
    new Paragraph({
      alignment: AlignmentType.CENTER,
      pageBreakBefore: true,
      spacing: { line: LINE, after: 240 },
      children: [new TextRun({ text: "СОДЕРЖАНИЕ", font: FONT, size: SZ, bold: true })],
    }),
  ];
  for (const [display, level, key] of TOC_ENTRIES) {
    const num = pages[key] != null ? String(pages[key]) : "";
    out.push(new Paragraph({
      spacing: { line: LINE, after: 0 },
      indent: { left: level === 2 ? 425 : 0 },
      tabStops: [{ type: TabStopType.RIGHT, position: CW, leader: LeaderType.DOT }],
      children: [new TextRun({ text: display + "\t" + num, font: FONT, size: SZ })],
    }));
  }
  return out;
}

/* ---------- обработка тела ---------- */
function parseBody() {
  const raw = fs.readFileSync(path.join(ROOT, "vkr_text.txt"), "utf8").replace(/^﻿/, "");
  const lines = raw.split(/\r?\n/);
  const out = [];
  let state = "preface"; // preface -> skipToc -> body
  let refsMode = false;
  let expectTasks = false;
  let skipUntilPrefix = null;

  for (let i = 0; i < lines.length; i++) {
    let line = lines[i].trim();
    if (line === "") continue;

    if (skipUntilPrefix) {
      if (line.startsWith(skipUntilPrefix)) skipUntilPrefix = null;
      continue;
    }
    if (state === "preface") {
      if (line === "СОДЕРЖАНИЕ") { out.push(...tocBlock()); state = "skipToc"; }
      continue;
    }
    if (state === "skipToc") {
      if (line === "ВВЕДЕНИЕ") { out.push(h1struct("Введение")); state = "body"; }
      continue;
    }

    // --- тело ---
    if (TABLES[line]) {
      out.push(...TABLES[line]);
      skipUntilPrefix = TABLE_SKIP_UNTIL[line];
      continue;
    }
    if (line === "ВВЕДЕНИЕ") { out.push(h1struct("Введение")); continue; }
    if (line === "ЗАКЛЮЧЕНИЕ") { out.push(h1struct("Заключение")); continue; }
    if (line === "СПИСОК ИСПОЛЬЗОВАННЫХ ИСТОЧНИКОВ") {
      out.push(h1struct("Список использованных источников"));
      refsMode = true;
      continue;
    }
    if (refsMode) { out.push(refItem(line)); continue; }

    if (/^\d+\.\d+\s+\S/.test(line)) { out.push(h2(line)); expectTasks = false; continue; }
    if (/^[1-4]\s+[А-ЯЁ]/.test(line)) { out.push(h1chapter(line)); expectTasks = false; continue; }

    const fm = line.match(/^(.*?)\s*(\([0-9]+\.[0-9]+\))\s*$/);
    if (fm && /[=←]/.test(fm[1])) { out.push(formula(fm[1].trim(), fm[2])); continue; }

    line = improve(line);

    if (expectTasks && /^[а-яё]/.test(line)) { out.push(dashItem(line)); continue; }
    expectTasks = false;

    out.push(body(line));
    if (/задачи:\s*$/.test(line)) expectTasks = true;
  }
  return out;
}

/* ---------- приложения с кодом ---------- */
function codeParagraphs(file) {
  const text = fs.readFileSync(path.join(ROOT, file), "utf8").replace(/^﻿/, "");
  const lines = text.replace(/\t/g, "    ").split(/\r?\n/);
  // убрать пустые хвостовые строки
  while (lines.length && lines[lines.length - 1].trim() === "") lines.pop();
  return lines.map(l => new Paragraph({
    alignment: AlignmentType.LEFT,
    spacing: { line: 240, before: 0, after: 0 },
    children: [new TextRun({ text: l === "" ? " " : l, font: MONO, size: SZ_CODE })],
  }));
}
function listingCaption(text) {
  return new Paragraph({
    alignment: AlignmentType.LEFT,
    spacing: { line: LINE, before: 200, after: 80 },
    keepNext: true,
    children: [new TextRun({ text, font: FONT, size: SZ })],
  });
}
function appendix(letter, title, note, files) {
  const out = [
    new Paragraph({
      heading: HeadingLevel.HEADING_1,
      alignment: AlignmentType.CENTER,
      pageBreakBefore: true,
      spacing: { line: LINE, after: 60 },
      children: [new TextRun({ text: "ПРИЛОЖЕНИЕ " + letter, font: FONT, size: SZ, bold: true })],
    }),
    center("(" + note + ")", { after: 120 }),
    center(title, { bold: true, after: 240 }),
  ];
  files.forEach((f, idx) => {
    out.push(listingCaption(`Листинг ${letter}.${idx + 1} — ${path.basename(f)}`));
    out.push(...codeParagraphs(f));
  });
  return out;
}

function appendices() {
  return [
    ...appendix("А", "Программный интерфейс библиотеки на основе C-ABI", "обязательное",
      ["Native/CarPhysics/include/car_physics.h"]),
    ...appendix("Б", "Реализация математических моделей узлов автомобиля", "обязательное",
      ["Native/CarPhysics/src/models.h", "Native/CarPhysics/src/models.cpp"]),
    ...appendix("В", "Ядро симуляции и точка входа динамической библиотеки", "обязательное",
      ["Native/CarPhysics/src/math_util.h", "Native/CarPhysics/src/car_sim.h",
       "Native/CarPhysics/src/car_sim.cpp", "Native/CarPhysics/src/api.cpp"]),
    ...appendix("Г", "Интеграция модуля в игровой движок Unity", "обязательное",
      ["Assets/Scripts/Car/CarPhysicsNative.cs"]),
  ];
}

/* ---------- сборка документа ---------- */
const doc = new Document({
  styles: {
    default: { document: { run: { font: FONT, size: SZ }, paragraph: { spacing: { line: LINE } } } },
    paragraphStyles: [
      {
        id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { font: FONT, size: SZ, bold: true },
        paragraph: { spacing: { line: LINE, before: 0, after: 240 }, outlineLevel: 0, keepNext: true },
      },
      {
        id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { font: FONT, size: SZ, bold: true },
        paragraph: { spacing: { line: LINE, before: 240, after: 120 }, outlineLevel: 1, keepNext: true },
      },
    ],
  },
  numbering: {
    config: [
      {
        reference: "dashes",
        levels: [{
          level: 0, format: LevelFormat.BULLET, text: "–", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 1069, hanging: 360 } } },
        }],
      },
      {
        reference: "refs",
        levels: [{
          level: 0, format: LevelFormat.DECIMAL, text: "%1.", alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 709, hanging: 360 } } },
        }],
      },
    ],
  },
  sections: [{
    properties: {
      page: {
        size: { width: 11906, height: 16838 },
        margin: { top: 1134, right: 850, bottom: 1134, left: 1701, header: 720, footer: 567 },
      },
      titlePage: true,
    },
    headers: { default: new Header({ children: [] }), first: new Header({ children: [] }) },
    footers: {
      first: new Footer({ children: [] }),
      default: new Footer({
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          children: [new TextRun({ children: [PageNumber.CURRENT], font: FONT, size: SZ })],
        })],
      }),
    },
    children: [
      ...titlePage(),
      ...parseBody(),
      ...appendices(),
    ],
  }],
});

Packer.toBuffer(doc).then(buf => {
  const out = path.join(ROOT, "МирошниченкоВКРМагистратура_ГОСТ.docx");
  fs.writeFileSync(out, buf);
  console.log("WROTE", out, buf.length, "bytes");
});
