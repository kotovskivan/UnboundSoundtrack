// Окно да/нет для всех уведомлений приложения.
// askYesNo({title, html, yes, no, ribbon}) -> Promise<true|false>.  no: null — одна кнопка.
// Клавиши: ←/→ (Tab) — выбор, Enter — нажать, Esc — «нет».
(function () {
  const RIBBON = "UNBOUND SOUNDTRACK / FAN-MADE / NOT AFFILIATED WITH EA / KITIVSKI + CLAUDE / ";

  function rnd(seed) { let s = seed >>> 0; return () => ((s = (s * 1664525 + 1013904223) >>> 0) / 4294967296); }

  // фон полосы: шум, зеленые брызги по краям, крестики справа внизу
  function paintFx(cv) {
    const r = cv.getBoundingClientRect(), dpr = window.devicePixelRatio || 1;
    const W = Math.max(1, Math.round(r.width)), H = Math.max(1, Math.round(r.height));
    cv.width = W * dpr; cv.height = H * dpr;
    const g = cv.getContext("2d"); g.scale(dpr, dpr);
    const R = rnd(W * 31 + H);
    // шум
    for (let i = 0; i < W * H / 260; i++) {
      g.fillStyle = `rgba(255,255,255,${0.02 + R() * 0.05})`;
      g.fillRect(R() * W, R() * H, 1, 1);
    }
    // мазок кистью слева от заголовка
    g.strokeStyle = "rgba(255,255,255,.05)"; g.lineWidth = 14; g.lineCap = "round";
    g.beginPath(); g.moveTo(-20, H * 0.42); g.quadraticCurveTo(W * 0.12, H * 0.36, W * 0.22, H * 0.3); g.stroke();
    // брызги: потеки вниз, гуще у края
    const splat = (x0, x1, edgeLeft) => {
      for (let i = 0; i < (x1 - x0) * H / 80; i++) {
        const t = R(), x = edgeLeft ? x0 + (x1 - x0) * Math.pow(t, 1.8) : x1 - (x1 - x0) * Math.pow(t, 1.8);
        const y = R() * H, len = 2 + R() * R() * 26, a = 0.25 + R() * 0.6;
        g.fillStyle = `rgba(${70 + R() * 40 | 0},${215 + R() * 40 | 0},${40 + R() * 30 | 0},${a})`;
        g.fillRect(x, y, 1 + (R() < 0.3 ? 1 : 0), len * (R() < 0.6 ? 0.25 : 1));
      }
    };
    splat(0, Math.min(90, W * 0.05), true);
    splat(W - Math.min(230, W * 0.13), W, false);
    // крестики: два ряда, потертые
    const cx = W - Math.min(560, W * 0.3), cy = H - 70, step = 26;
    for (let row = 0; row < 2; row++)
      for (let k = 0; k < 15; k++) {
        if (R() < 0.12) continue;
        const x = cx + k * step + (row ? step / 2 : 0) + (R() - .5) * 4, y = cy + row * 22 + (R() - .5) * 6, s = 8 + R() * 3;
        g.fillStyle = `rgba(150,148,165,${0.35 + R() * 0.25})`;
        g.fillRect(x - s, y - 2.5, s * 2, 5); g.fillRect(x - 2.5, y - s, 5, s * 2);
        g.fillStyle = "#141518";                                   // потертости
        for (let j = 0; j < 6; j++) g.fillRect(x - s + R() * s * 2, y - s + R() * s * 2, 1 + R() * 2, 1 + R() * 2);
      }
  }

  const SWOOSH = '<svg class="swoosh" viewBox="0 0 100 9" preserveAspectRatio="none" aria-hidden="true">' +
    '<path d="M2 6 C 25 3, 60 2.5, 98 3.5 L 97 6 C 62 5, 30 6, 3 8.5 Z" fill="#0d0d0f"/></svg>';

  window.askYesNo = function (o) {
    return new Promise(resolve => {
      const back = document.createElement("div");
      back.className = "dlg-back" + (o.tall ? " dlg-tall" : "") + (o.cls ? " " + o.cls : "");
      // o.buttons: [{label, val}] — свои кнопки (выбор языка), o.start — какая подсвечена сначала; Esc — false
      const btns = o.buttons || [{ label: o.yes || "YES", val: true }];
      if (!o.buttons && o.no !== null) btns.push({ label: o.no || "NO", val: false });
      const rib = (o.ribbon || RIBBON).repeat(12);
      back.innerHTML =
        '<div class="dlg" role="alertdialog" aria-modal="true" aria-labelledby="dlg-t">' +
        `<div class="dlg-ribbon"><span>${rib}${rib}</span></div>` +
        '<canvas class="dlg-fx"></canvas>' +
        '<div class="dlg-body">' +
        `<h1 class="dlg-title" id="dlg-t"></h1><div class="dlg-text">${o.html || ""}</div>` +
        '<div class="dlg-btns">' + btns.map((b, i) =>
          `<button class="dlg-btn${i === (o.start > 0 ? o.start : 0) ? " focus" : ""}" data-i="${i}"><span class="key">↵</span><span class="lbl"></span>${SWOOSH}</button>`).join("") +
        "</div></div></div>";
      back.querySelector(".dlg-title").textContent = o.title || "";
      back.querySelectorAll(".dlg-btn .lbl").forEach((el, i) => el.textContent = btns[i].label);
      document.body.appendChild(back);
      const els = [...back.querySelectorAll(".dlg-btn")];
      let cur = 0;
      const focus = i => { cur = (i + els.length) % els.length; els.forEach((e, k) => e.classList.toggle("focus", k === cur)); els[cur].focus({ preventScroll: true }); };
      const done = v => {
        document.removeEventListener("keydown", key, true); window.removeEventListener("resize", repaint);
        back.classList.remove("show"); setTimeout(() => back.remove(), 200); resolve(v);
      };
      const key = e => {
        if (e.target && e.target.tagName === "INPUT" && e.key !== "Escape") return;   // поле ввода внутри окна — клавиши его
        // переключатель языка поверх первых окон (R1-05): Esc закрывает список языков, Enter открывает его — не «Вийти» / не кнопка окна
        if (document.querySelector(".lsel.open") || (e.target && e.target.closest && e.target.closest(".lang"))) return;
        if (e.key === "ArrowRight" || e.key === "ArrowLeft" || e.key === "Tab") { e.preventDefault(); focus(cur + (e.key === "ArrowLeft" || e.shiftKey ? -1 : 1)); }
        else if (e.key === "Enter" || e.key === " ") { e.preventDefault(); done(btns[cur].val); }
        else if (e.key === "Escape" && btns.length > 1) { e.preventDefault(); done(false); }
      };
      els.forEach((e, i) => { e.addEventListener("mouseenter", () => focus(i)); e.addEventListener("click", () => done(btns[i].val)); });
      document.addEventListener("keydown", key, true);
      const cv = back.querySelector(".dlg-fx"), repaint = () => paintFx(cv);
      window.addEventListener("resize", repaint);
      requestAnimationFrame(() => { repaint(); back.classList.add("show"); focus(o.start > 0 ? o.start : 0); });
      // o.re() — те же {title, html, yes, no} на новом языке (язык сменили, пока окно открыто); живое содержимое (onOpen) не трогаем — для него o.onRelang
      if (o.re || o.onRelang) back._re = () => {
        if (o.re) {
          const x = o.re();
          back.querySelector(".dlg-title").textContent = x.title || "";
          if (x.html != null && !o.onOpen) back.querySelector(".dlg-text").innerHTML = x.html;
          const labels = x.buttons ? x.buttons.map(b => b.label) : [x.yes || "YES"].concat(x.no !== null ? [x.no || "NO"] : []);
          back.querySelectorAll(".dlg-btn .lbl").forEach((el, i) => { if (labels[i] != null) el.textContent = labels[i]; });
        }
        if (o.onRelang) o.onRelang(back);
      };
      if (o.onOpen) o.onOpen(back.querySelector(".dlg-text"), done);   // живое содержимое (выбор папки и т.п.)
    });
  };
  window.relangDialogs = () => document.querySelectorAll(".dlg-back").forEach(b => b._re && b._re());

  // «идет работа»: та же полоса, без кнопок — заголовок, шаг, полоска прогресса (pct < 0 — бегущая)
  let busy = null;
  window.showBusy = function (title, text, stop) {   // stop = {label, fn}: кнопка «Стоп» под полосой, Esc — то же
    if (!busy) {
      busy = document.createElement("div");
      busy.className = "dlg-back";
      const rib = RIBBON.repeat(12);
      busy.innerHTML = '<div class="dlg" role="status" aria-live="polite">' +
        `<div class="dlg-ribbon"><span>${rib}${rib}</span></div><canvas class="dlg-fx"></canvas>` +
        '<div class="dlg-body"><h1 class="dlg-title"></h1><div class="dlg-text"><p class="busy-step"></p></div>' +
        '<div class="busy-bar"><i></i></div><div class="busy-log"></div></div></div>';
      document.body.appendChild(busy);
      requestAnimationFrame(() => { paintFx(busy.querySelector(".dlg-fx")); busy.classList.add("show"); });
    }
    busy.querySelector(".dlg-title").textContent = title || "";
    setStop(stop);
    updateBusy(text, -1);
  };
  function setStop(o) {
    let row = busy.querySelector(".dlg-btns");
    busy.onStop = o ? o.fn : null;
    if (!o) { if (row) row.remove(); return; }
    if (!row) {
      row = document.createElement("div"); row.className = "dlg-btns";
      row.innerHTML = `<button class="dlg-btn focus"><span class="key">Esc</span><span class="lbl"></span>${SWOOSH}</button>`;
      busy.querySelector(".dlg-body").appendChild(row);
    }
    row.querySelector(".lbl").textContent = o.label;
    row.querySelector("button").onclick = () => busy && busy.onStop && busy.onStop();
  }
  document.addEventListener("keydown", e => { if (busy && busy.onStop && e.key === "Escape") { e.preventDefault(); busy.onStop(); } }, true);
  // строка под полосой (ошибка / замена / повтор) — видно сразу, по ходу работы; kind: err | warn
  window.busyLog = function (text, kind) {
    if (!busy) return;
    const box = busy.querySelector(".busy-log"), d = document.createElement("div");
    d.className = kind || ""; d.textContent = text; box.appendChild(d); box.scrollTop = box.scrollHeight;
  };
  window.updateBusy = function (text, pct) {
    if (!busy) return;
    if (text != null) busy.querySelector(".busy-step").textContent = text;
    const bar = busy.querySelector(".busy-bar");
    bar.classList.toggle("run", !(pct >= 0));
    bar.querySelector("i").style.width = pct >= 0 ? Math.min(100, pct) + "%" : "";
  };
  window.hideBusy = function () {
    if (!busy) return;
    const b = busy; busy = null; b.classList.remove("show"); setTimeout(() => b.remove(), 200);
  };
})();
