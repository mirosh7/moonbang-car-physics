/* Заменяет список источников в vkr_text.txt на расширенный (19 шт.,
 * порядок — по первому упоминанию в тексте, формат — по образцу с ISBN). */
const fs = require("fs");
const path = require("path");

const FILE = path.join(__dirname, "..", "vkr_text.txt");
const HEAD = "СПИСОК ИСПОЛЬЗОВАННЫХ ИСТОЧНИКОВ";

const REFS = [
  "Pacejka, H. B. Tire and Vehicle Dynamics / H. B. Pacejka. — 3rd ed. — Oxford : Butterworth-Heinemann, 2012. — 672 p. — ISBN 978-0-08-097016-5.",
  "Gillespie, T. D. Fundamentals of Vehicle Dynamics / T. D. Gillespie. — Warrendale : SAE International, 1992. — 519 p. — ISBN 978-1-56091-199-9.",
  "Milliken, W. F. Race Car Vehicle Dynamics / W. F. Milliken, D. L. Milliken. — Warrendale : SAE International, 1995. — 890 p. — ISBN 978-1-56091-526-3.",
  "Wong, J. Y. Theory of Ground Vehicles / J. Y. Wong. — 4th ed. — Hoboken : John Wiley & Sons, 2008. — 592 p. — ISBN 978-0-470-17038-0.",
  "Besselink, I. J. M. An improved Magic Formula/Swift tyre model that can handle inflation pressure changes / I. J. M. Besselink, A. J. C. Schmeitz, H. B. Pacejka // Vehicle System Dynamics. — 2010. — Vol. 48, suppl. 1. — P. 337–352.",
  "Fiala, E. Seitenkräfte am rollenden Luftreifen / E. Fiala // VDI-Zeitschrift. — 1954. — Bd. 96, № 29. — S. 973–979.",
  "Blundell, M. The Multibody Systems Approach to Vehicle Dynamics / M. Blundell, D. Harty. — 2nd ed. — Oxford : Butterworth-Heinemann, 2015. — 768 p. — ISBN 978-0-08-099425-3.",
  "Dugoff, H. An analysis of tire traction properties and their influence on vehicle dynamic performance / H. Dugoff, P. Fancher, L. Segel // SAE Technical Paper 700377. — Warrendale : SAE International, 1970. — 18 p.",
  "Vehicle Physics Pro: advanced vehicle simulation kit // Vehicle Physics Pro : [официальный сайт]. — URL: https://vehiclephysics.com/ (дата обращения: 09.06.2026). — Текст : электронный.",
  "NVIDIA PhysX SDK Documentation // NVIDIA Developer : [официальный сайт]. — URL: https://nvidia-omniverse.github.io/PhysX/ (дата обращения: 09.06.2026). — Текст : электронный.",
  "Tasora, A. Chrono: An Open Source Multi-physics Dynamics Engine / A. Tasora [et al.] // Lecture Notes in Computer Science. — 2016. — Vol. 9611. — P. 19–49. — ISBN 978-3-319-40360-1.",
  "Project Chrono: An Open Source Multi-physics Simulation Engine // Project Chrono : [официальный сайт]. — URL: https://projectchrono.org/ (дата обращения: 09.06.2026). — Текст : электронный.",
  "Jazar, R. N. Vehicle Dynamics: Theory and Application / R. N. Jazar. — 3rd ed. — Cham : Springer, 2017. — 1068 p. — ISBN 978-3-319-53440-4.",
  "Genta, G. Motor Vehicle Dynamics: Modeling and Simulation / G. Genta. — Singapore : World Scientific, 1997. — 539 p. — ISBN 978-981-02-2911-5.",
  "Rajamani, R. Vehicle Dynamics and Control / R. Rajamani. — 2nd ed. — New York : Springer, 2012. — 498 p. — ISBN 978-1-4614-1432-2.",
  "Kiencke, U. Automotive Control Systems: For Engine, Driveline and Vehicle / U. Kiencke, L. Nielsen. — 2nd ed. — Berlin : Springer, 2005. — 512 p. — ISBN 978-3-540-23139-4.",
  "Platform Invoke (P/Invoke) // Microsoft Learn : [официальный сайт]. — URL: https://learn.microsoft.com/dotnet/standard/native-interop/pinvoke (дата обращения: 09.06.2026). — Текст : электронный.",
  "ISO/IEC 14882:2020. Programming languages — C++. — Geneva : International Organization for Standardization, 2020. — 1853 p.",
  "Unity User Manual // Unity Technologies : [официальный сайт]. — URL: https://docs.unity3d.com/Manual/ (дата обращения: 09.06.2026). — Текст : электронный.",
];

let raw = fs.readFileSync(FILE, "utf8").replace(/^﻿/, "");
const lines = raw.split(/\r?\n/);
const idx = lines.findIndex(l => l.trim() === HEAD);
if (idx < 0) { console.error("HEAD not found"); process.exit(1); }
const head = lines.slice(0, idx + 1);
const out = head.concat(REFS).join("\r\n") + "\r\n";
fs.writeFileSync(FILE, out, "utf8");
console.log("Bibliography replaced:", REFS.length, "entries; head lines:", head.length);
