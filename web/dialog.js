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

  // ===== ГЛИТЧ окна при открытии / закрытии (18.11, Ваня: «чтоб окна переключались с глитчами», «без выезда снизу и оба варианта мне очень нравятся»).
  // Образцы — айдентика Unbound (knowledge/stil-unbound.md «Айдентика»; демо outputs/nfs-unbound/glitch-okon/demo-glitch.html). Два рецепта, переходы берут
  // их по очереди: xp — окно XP из концепта UI (копия окна съехала, полосы, RGB, потом одна строка дергается), swap — лого из интро
  // (окно на кадр-два другое: обрез заголовка, синий/лайм, серый, растяжка, призрак). Кадр 1/30 с. «Куски кадра» — копии окна .dlg-gh,
  // обрезанные clip-path полосой и сдвинутые; RGB и синий/лайм — SVG-фильтры; линии и квадраты — canvas .dlg-gl поверх.
  // Яркий (серый) кадр — один на переход (правило против эпилепсии). prefers-reduced-motion — без глитча.
  // 18.11 ~01:30 Ваня: «туда (разделы слева, редактор песни) тоже добавить; 2 типа как в прототипе, чтоб чередовались» — по очереди xp / swap,
  // свой счет у окон (dlg), разделов (grid), редактора (rpan) — отдельно для открытия и закрытия. Яркие кадры (серый, синий/лайм) — не чаще раза в секунду.
  const GL = window.GL = { variant: null, slow: 1, manual: false, n: {}, brightAt: -1e9, brightRun: null };   // variant: null — по очереди; manual — шаг кадров вручную GL.next() (скрипты проверки)
  const reduced = () => !!(window.matchMedia && matchMedia("(prefers-reduced-motion: reduce)").matches);
  const PAL = ["#b6ff1a", "#ff2e88", "#2fe6ff", "#f4f4f0", "#2b2bff", "#ffe600"];
  let svgDone = false;
  function ensureSvg() {
    if (svgDone) return; svgDone = true;
    const s = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    s.setAttribute("width", "0"); s.setAttribute("height", "0"); s.setAttribute("aria-hidden", "true");
    s.style.cssText = "position:absolute;width:0;height:0";
    s.innerHTML =
      '<filter id="gl-rgb" x="-3%" y="0" width="106%" height="100%" color-interpolation-filters="sRGB">' +
      '<feColorMatrix in="SourceGraphic" type="matrix" values="1 0 0 0 0  0 0 0 0 0  0 0 0 0 0  0 0 0 1 0" result="r"/><feOffset in="r" dx="-8" result="r2"/>' +
      '<feColorMatrix in="SourceGraphic" type="matrix" values="0 0 0 0 0  0 1 0 0 0  0 0 1 0 0  0 0 0 1 0" result="gb"/><feOffset in="gb" dx="6" result="gb2"/>' +
      '<feBlend in="r2" in2="gb2" mode="screen"/></filter>' +
      '<filter id="gl-duo" color-interpolation-filters="sRGB">' +   // темное — синий, светлое — лайм (концепт UI)
      '<feColorMatrix type="matrix" values=".3 .55 .15 0 0  .3 .55 .15 0 0  .3 .55 .15 0 0  0 0 0 1 0"/>' +
      '<feComponentTransfer><feFuncR type="table" tableValues="0.09 0.06 0.71"/><feFuncG type="table" tableValues="0.1 0.12 1"/><feFuncB type="table" tableValues="0.85 0.95 0.1"/></feComponentTransfer></filter>';
    document.body.appendChild(s);
  }
  // копия без хвостов длинных списков (окно «+» на сотни песен): у каждого узла — не больше 60 детей
  const cloneLite = el => {
    const c = el.cloneNode(false); let n = 0;
    for (const ch of el.childNodes) { if (++n > 60) break; c.appendChild(ch.nodeType === 1 ? cloneLite(ch) : ch.cloneNode(false)); }
    return c;
  };
  function makeGhost(dlg) {
    const g = document.createElement("div");
    g.className = "dlg-gh"; g.setAttribute("aria-hidden", "true");
    for (const ch of dlg.children) if (!ch.classList.contains("dlg-gh") && !ch.classList.contains("dlg-gl")) g.appendChild(cloneLite(ch));
    g.querySelectorAll("[id]").forEach(e => e.removeAttribute("id"));
    g.querySelectorAll("button, a, input").forEach(e => e.tabIndex = -1);
    const src = dlg.querySelector(".dlg-fx"), dst = g.querySelector(".dlg-fx");
    if (src && dst && src.width) { dst.width = src.width; dst.height = src.height; dst.getContext("2d").drawImage(src, 0, 0); }
    const rs = dlg.querySelector(".dlg-ribbon span"), rd = g.querySelector(".dlg-ribbon span");
    if (rs && rd) { rd.style.animation = "none"; rd.style.transform = getComputedStyle(rs).transform; }
    dlg.appendChild(g);
    return g;
  }
  const FX = {
    xp: {
      in: () => [].concat(
        Array.from({ length: 5 }, (_, k) => A => {
          A.filter("rgb");
          const y0 = A.H * (.25 + A.R() * .2);
          A.band(0, y0, y0 + A.H * (.3 + A.R() * .2), -A.W * (.06 + A.R() * .12), { dy: -y0 * (.7 + A.R() * .3) });
          A.rb(1, 3, A.W * .12); A.lines(4 + k);
        }),
        Array.from({ length: 6 }, (_, k) => A => {
          if (k % 2 === 0) A.filter("rgb");
          const y = A.H * (.28 + A.R() * .45), h = 12 + A.R() * 22;
          A.band(1, y, y + h, (A.R() - .5) * 26, { dy: h * (.5 + A.R() * .5) });
          A.lines(2);
        })),
      out: () => [
        A => { A.filter("rgb"); A.rb(0, 3, A.W * .15); A.lines(5); },
        A => { A.filter("rgb"); A.band(0, A.H * .3, A.H * .8, -A.W * .12, { dy: -A.H * .25 }); A.rb(1, 2, A.W * .2); A.lines(7); },
        A => A.hide()]
    },
    swap: {
      in: () => [
        A => { A.ghost(0, { transform: "scale(3.3)", transformOrigin: "5% 36%" }); A.filter("rgb"); },
        A => { A.ghost(0, { transform: "scale(2.4) translateX(-6%)", transformOrigin: "22% 36%" }); A.filter("rgb"); A.lines(3); },
        A => { A.filter("duo"); A.rb(1, 2, A.W * .1); },
        A => { A.filter("duo"); A.rb(1, 3, A.W * .06); A.blocks(3); },
        A => { A.paper(); A.lines(5, ["#0a0a0a", "#f4f4f0"]); },
        A => { A.stretch(1, 3); A.filter("rgb"); },
        A => { A.stretch(1, 2); A.lines(3); },
        A => { A.behind(.5); A.rb(1, 1, 30); },
        A => A.behind(.36),
        A => A.behind(.22),
        A => A.behind(.1)],
      out: () => [
        A => { A.filter("duo"); A.rb(0, 2, A.W * .1); },
        A => { A.stretch(0, 3); A.filter("rgb"); },
        A => A.hide()]
    }
  };
  const glOn = () => !reduced();
  // сцена: окно (копии внутри .dlg) или область страницы (сетка, редактор): слой .gl-ov поверх по месту на экране,
  // в копиях — только видимые дети, поставленные туда же, где они на экране (прокрутка и сотни плиток не мешают)
  function stageDlg(dlg) { return { W: dlg.clientWidth, H: dlg.clientHeight, fx: [dlg], host: dlg, ghost: () => makeGhost(dlg), done() {} }; }
  // видимые дети области и их место на экране меряются ОДИН раз на переход, до вставки слоя (иначе на сотнях плиток каждая копия
  // заставляет страницу пересчитать раскладку: 563 плитки — +0,12 с до первого кадра); прокрутка внутри детей — тоже заранее
  function stageArea(el) {
    const r0 = el.getBoundingClientRect(), vis = [];
    for (const ch of el.children) {
      const r = ch.getBoundingClientRect();
      if (!r.width || !r.height || r.bottom <= r0.top || r.top >= r0.bottom || r.right <= r0.left || r.left >= r0.right) continue;
      const sc = []; [ch, ...ch.querySelectorAll("*")].forEach((e, i) => { if (e.scrollTop) sc.push([i, e.scrollTop]); });
      vis.push({ ch, r, sc });
    }
    const z = (parseInt(getComputedStyle(el).zIndex, 10) || 4) + 1, ov = document.createElement("div");
    ov.className = "gl-ov"; ov.setAttribute("aria-hidden", "true");
    Object.assign(ov.style, { left: r0.left + "px", top: r0.top + "px", width: r0.width + "px", height: r0.height + "px", zIndex: String(z) });
    document.body.appendChild(ov);
    return { W: Math.round(r0.width), H: Math.round(r0.height), fx: [el, ov], host: ov, ghost: () => areaGhost(el, ov, r0, vis), done() { ov.remove(); } };
  }
  function areaGhost(el, ov, r0, vis) {
    const g = document.createElement("div"), pairs = [];
    g.className = el.className + " gl-agh"; g.setAttribute("aria-hidden", "true");
    for (const v of vis) {
      const c = v.ch.cloneNode(true), r = v.r;
      Object.assign(c.style, { position: "absolute", left: (r.left - r0.left) + "px", top: (r.top - r0.top) + "px", width: r.width + "px", height: r.height + "px",
        margin: "0", boxSizing: "border-box", transform: "none" });
      g.appendChild(c); pairs.push([v, c]);
    }
    g.querySelectorAll("[id]").forEach(e => e.removeAttribute("id"));
    g.querySelectorAll("button, a, input, select, textarea").forEach(e => e.tabIndex = -1);
    ov.appendChild(g);
    for (const [v, c] of pairs) {   // canvas (волна, обложка) — копия пикселей; прокрутка внутри (редактор) — как у оригинала
      const s = v.ch, sc = s.matches("canvas") ? [s] : [...s.querySelectorAll("canvas")], cc = c.matches("canvas") ? [c] : [...c.querySelectorAll("canvas")];
      sc.forEach((x, i) => { const y = cc[i]; if (y && x.width && x.height) { y.width = x.width; y.height = x.height; y.getContext("2d").drawImage(x, 0, 0); } });
      if (v.sc.length) { const co = [c, ...c.querySelectorAll("*")]; v.sc.forEach(([i, top]) => { if (co[i]) co[i].scrollTop = top; }); }
    }
    return g;
  }
  // o.area — область страницы (не окно), o.key — чей счет очереди xp / swap
  function glitch(el, kind, cb, o = {}) {
    if (el._gl) el._gl.stop();
    if (!glOn() || !el.isConnected) { cb && cb(); return; }
    const key = (o.key || "dlg") + "-" + kind, nx = GL.n[key] = (GL.n[key] || 0) + 1;
    const rec = FX[GL.variant] || FX[nx % 2 ? "xp" : "swap"];
    ensureSvg();
    const S = o.area ? stageArea(el) : stageDlg(el), W = S.W, H = S.H, R = rnd(Date.now() ^ (W * 7 + H * 13));
    if (!W || !H) { S.done(); cb && cb(); return; }
    const gh = [], G = i => gh[i] || (gh[i] = S.ghost());
    const cv = document.createElement("canvas"); cv.className = "dlg-gl"; cv.width = W; cv.height = H; S.host.appendChild(cv);
    const c2 = cv.getContext("2d"), run = {};
    // яркий кадр (серый / синий-лайм): в своем переходе — сколько задумано; новый переход раньше чем через 1 с после яркого — без них (против вспышек)
    const bright = () => { const now = performance.now(); if (GL.brightRun === run || now - GL.brightAt > 1000) { GL.brightRun = run; GL.brightAt = now; return true; } return false; };
    const show = (g, css) => { g.style.visibility = "visible"; Object.assign(g.style, css); };
    const A = {
      W, H, R,
      band(i, y0, y1, dx, o = {}) {   // полоса y0..y1 сдвинута на dx (dy), sx — растянута вбок от ox
        show(G(i), { clipPath: `inset(${Math.max(0, y0)}px 0 ${Math.max(0, H - y1)}px 0)`,
          transform: `translate(${dx}px, ${o.dy || 0}px)` + (o.sx ? ` scaleX(${o.sx})` : ""), transformOrigin: (o.ox || 0) + "px 0" });
      },
      rb(from, n, max) { for (let k = 0; k < n; k++) { const y = R() * H * .92, h = 6 + R() * H * .16, d = (R() < .5 ? -1 : 1) * (8 + R() * max); A.band(from + k, y, y + h, d); } },
      stretch(from, n) { for (let k = 0; k < n; k++) { const y = R() * H * .9, h = 8 + R() * 50; A.band(from + k, y, y + h, 0, { sx: 2 + R() * 5, ox: R() * W * .5 }); } },
      ghost(i, css) { show(G(i), css); },
      behind(op) { show(G(0), { background: "transparent", opacity: op, mixBlendMode: "screen", transform: "translate(2.5%, -9%) scale(1.16)", transformOrigin: "0 50%" }); },
      filter(f) {
        if (f === "duo" && !bright()) f = "rgb";
        const v = f === "rgb" ? "url(#gl-rgb)" : f === "duo" ? "url(#gl-duo)" : f;
        S.fx.forEach(e => e.style.filter = v);
      },
      paper() { if (bright()) S.fx.forEach(e => e.classList.add("gl-paper")); else A.filter("rgb"); },
      hide() { el.style.opacity = "0"; },
      lines(n, pal = PAL) {
        for (let k = 0; k < n; k++) {
          const y = R() * H, h = 1 + (R() * 3 | 0), full = R() < .45, x = full ? 0 : R() * W * .7, w = full ? W : W * (.08 + R() * .45);
          c2.globalAlpha = .5 + R() * .45; c2.fillStyle = pal[R() * pal.length | 0]; c2.fillRect(x, y, w, h);
        }
        c2.globalAlpha = 1;
      },
      blocks(n) {   // пиксельные квадраты (лаймовая сетка из ролика Frostbite)
        for (let k = 0; k < n; k++) {
          const y = R() * H, s = 5 + (R() * 9 | 0), x0 = R() * W * .8;
          for (let j = 0, m = 6 + R() * 16; j < m; j++) { c2.fillStyle = R() < .75 ? "#b6ff1a" : "#f4f4f0"; c2.fillRect(x0 + j * s * 1.5 + R() * s, y + (R() - .5) * s * 3, s, s); }
        }
      }
    };
    const frames = rec[kind]();
    let i = 0, t = 0;
    const reset = () => { gh.forEach(g => g.removeAttribute("style")); S.fx.forEach(e => { e.style.filter = ""; e.classList.remove("gl-paper"); }); el.style.opacity = ""; c2.clearRect(0, 0, W, H); };
    const stop = () => { clearTimeout(t); reset(); gh.forEach(g => g.remove()); cv.remove(); S.done(); el._gl = null; if (GL.next === step) GL.next = null; };
    const t0 = performance.now();   // 18.11: кадры по общим часам — медленная отрисовка не растягивает глитч (как в Heat); ≥ 10 мс — кадр успевает нарисоваться
    const step = () => {
      reset();
      if (i >= frames.length || !el.isConnected) { stop(); cb && cb(); return; }
      frames[i++](A);
      if (GL.manual) GL.next = step; else t = setTimeout(step, Math.max(10, t0 + i * 1000 / 30 * GL.slow - performance.now()));
    };
    el._gl = { stop };
    step();
  }
  // область страницы: сетка после смены раздела, редактор песни (index.html)
  window.glitchArea = (el, kind, cb, key) => glitch(el, kind, cb, { area: true, key });
  // открыть: окно встает сразу (без въезда), глитч поверх; intro — кнопка в фокусе разгорается после глитча (dialog.css)
  function openFx(back) {
    back.classList.add("show", "intro");
    setTimeout(() => back.classList.remove("intro"), 800);
    glitch(back.querySelector(".dlg"), "in");
  }
  // закрыть: глитч, потом окно спрятано (не проявляется снова, пока гаснет фон) → cb
  function closeFx(back, cb) {
    back.style.pointerEvents = "none";
    const dlg = back.querySelector(".dlg"), on = glOn();
    glitch(dlg, "out", () => { if (on) dlg.style.opacity = "0"; cb(); });
  }

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
        closeFx(back, () => { back.classList.remove("show"); setTimeout(() => back.remove(), 200); });
        resolve(v);   // сразу: следующее окно открывается поверх, пока это уходит глитчем — «переключение»
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
      requestAnimationFrame(() => { repaint(); focus(o.start > 0 ? o.start : 0); openFx(back); });   // открытие — глитч (openFx); кнопка разгорается только при открытии
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
      const b = busy; requestAnimationFrame(() => { paintFx(b.querySelector(".dlg-fx")); openFx(b); });
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
    const b = busy; busy = null;
    closeFx(b, () => { b.classList.remove("show"); setTimeout(() => b.remove(), 200); });
  };
})();
