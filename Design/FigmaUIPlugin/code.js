/**
 * The Lord of the Bow — Elven Forge Codex UI Builder
 * Builds foundations, components, and all game screens at 1080×1920.
 */
(async () => {
  const W = 1080;
  const H = 1920;

  const C = {
    stone: hex("1A1612"),
    stoneMid: hex("2C241C"),
    stoneLight: hex("3D3228"),
    parchment: hex("E8D9B8"),
    parchmentDeep: hex("D4C19A"),
    forest: hex("2D5A3D"),
    forestBright: hex("3E7A52"),
    teal: hex("5C9FAE"),
    tealDeep: hex("3A6F7C"),
    ember: hex("EAB770"),
    emberDeep: hex("C8893A"),
    moss: hex("648255"),
    mossDeep: hex("445A38"),
    health: hex("A9FF8D"),
    mithril: hex("C5D4E0"),
    gold: hex("D4A84B"),
    danger: hex("A63D3D"),
    dangerBright: hex("C94C4C"),
    textLight: hex("F5EFE0"),
    textMuted: hex("B8A990"),
    textDark: hex("2A241C"),
    ink: hex("0E0C0A"),
    reward: hex("C9A227"),
    white: hex("FFFFFF"),
    black: hex("000000"),
  };

  function hex(h) {
    const n = parseInt(h, 16);
    return { r: ((n >> 16) & 255) / 255, g: ((n >> 8) & 255) / 255, b: (n & 255) / 255 };
  }
  function solid(color, opacity = 1) {
    return [{ type: "SOLID", color, opacity }];
  }
  function rgb(c) {
    return { r: c.r, g: c.g, b: c.b };
  }

  // —— Fonts ——
  let displayFont = { family: "Inter", style: "Bold" };
  let bodyReg = { family: "Inter", style: "Regular" };
  let bodyMed = { family: "Inter", style: "Medium" };
  let bodySemi = { family: "Inter", style: "Semi Bold" };
  let bodyBold = { family: "Inter", style: "Bold" };
  let fontNote = "";

  async function tryLoad(family, style) {
    try {
      await figma.loadFontAsync({ family, style });
      return { family, style };
    } catch (_) {
      return null;
    }
  }

  async function resolveFonts() {
    const d =
      (await tryLoad("Climate Crisis", "Regular")) ||
      (await tryLoad("ClimateCrisis", "Regular")) ||
      (await tryLoad("Climate Crisis Regular", "Regular"));
    const jr =
      (await tryLoad("Jura", "Regular")) ||
      (await tryLoad("Jura", "Light"));
    const jm =
      (await tryLoad("Jura", "Medium")) ||
      (await tryLoad("Jura", "Regular"));
    const js =
      (await tryLoad("Jura", "SemiBold")) ||
      (await tryLoad("Jura", "Semi Bold")) ||
      (await tryLoad("Jura", "Medium")) ||
      (await tryLoad("Jura", "Bold"));
    const jb =
      (await tryLoad("Jura", "Bold")) ||
      (await tryLoad("Jura", "SemiBold"));

    await figma.loadFontAsync({ family: "Inter", style: "Regular" });
    await figma.loadFontAsync({ family: "Inter", style: "Medium" });
    await figma.loadFontAsync({ family: "Inter", style: "Semi Bold" });
    await figma.loadFontAsync({ family: "Inter", style: "Bold" });

    if (d) displayFont = d;
    if (jr) bodyReg = jr;
    if (jm) bodyMed = jm;
    if (js) bodySemi = js;
    if (jb) bodyBold = jb;

    const missing = [];
    if (!d) missing.push("Climate Crisis");
    if (!jr) missing.push("Jura");
    fontNote = missing.length
      ? "Missing fonts (install then re-run): " + missing.join(", ") + ". Using Inter fallback for missing."
      : "Fonts OK: Climate Crisis + Jura";
  }

  function txt(chars, opts = {}) {
    const t = figma.createText();
    t.fontName = opts.font || bodyReg;
    t.fontSize = opts.size || 28;
    t.characters = chars;
    t.fills = solid(opts.color || C.textLight);
    if (opts.align) t.textAlignHorizontal = opts.align;
    if (opts.lineHeight) t.lineHeight = opts.lineHeight;
    if (opts.letterSpacing) t.letterSpacing = opts.letterSpacing;
    if (opts.width) {
      t.resize(opts.width, t.height);
      t.textAutoResize = "HEIGHT";
    }
    if (opts.name) t.name = opts.name;
    return t;
  }

  function frame(name, w, h, opts = {}) {
    const f = figma.createFrame();
    f.name = name;
    f.resize(w, h);
    f.fills = opts.fills || solid(C.stone);
    if (opts.radius != null) f.cornerRadius = opts.radius;
    if (opts.stroke) {
      f.strokes = solid(opts.stroke, opts.strokeOpacity ?? 1);
      f.strokeWeight = opts.strokeWeight || 2;
    }
    if (opts.clips != null) f.clipsContent = opts.clips;
    if (opts.opacity != null) f.opacity = opts.opacity;
    return f;
  }

  function auto(name, dir, opts = {}) {
    const f = figma.createFrame();
    f.name = name;
    f.layoutMode = dir;
    f.primaryAxisSizingMode = opts.primary || "AUTO";
    f.counterAxisSizingMode = opts.counter || "AUTO";
    f.itemSpacing = opts.gap ?? 16;
    f.paddingTop = opts.pt ?? opts.pad ?? 0;
    f.paddingBottom = opts.pb ?? opts.pad ?? 0;
    f.paddingLeft = opts.pl ?? opts.pad ?? 0;
    f.paddingRight = opts.pr ?? opts.pad ?? 0;
    f.fills = opts.fills || [];
    if (opts.radius != null) f.cornerRadius = opts.radius;
    if (opts.stroke) {
      f.strokes = solid(opts.stroke, opts.strokeOpacity ?? 1);
      f.strokeWeight = opts.strokeWeight || 2;
    }
    if (opts.primaryAlign) f.primaryAxisAlignItems = opts.primaryAlign;
    if (opts.counterAlign) f.counterAxisAlignItems = opts.counterAlign;
    if (opts.w || opts.h) f.resize(opts.w || 100, opts.h || 100);
    if (opts.primary === "FIXED" && opts.w) f.primaryAxisSizingMode = "FIXED";
    if (opts.counter === "FIXED" && (opts.h || opts.w)) f.counterAxisSizingMode = "FIXED";
    return f;
  }

  function shadow(node, y = 8, blur = 24, opacity = 0.45) {
    node.effects = [
      {
        type: "DROP_SHADOW",
        color: { r: 0, g: 0, b: 0, a: opacity },
        offset: { x: 0, y },
        radius: blur,
        spread: 0,
        visible: true,
        blendMode: "NORMAL",
      },
    ];
  }

  function glow(node, color, blur = 18, opacity = 0.55) {
    node.effects = [
      {
        type: "DROP_SHADOW",
        color: { r: color.r, g: color.g, b: color.b, a: opacity },
        offset: { x: 0, y: 0 },
        radius: blur,
        spread: 0,
        visible: true,
        blendMode: "NORMAL",
      },
    ];
  }

  function ornamentBar(w, accent) {
    const bar = auto("Ornament", "HORIZONTAL", {
      gap: 10,
      counterAlign: "CENTER",
      primaryAlign: "CENTER",
      w,
      primary: "FIXED",
    });
    const lineL = figma.createRectangle();
    lineL.resize(Math.floor((w - 40) / 2), 2);
    lineL.fills = solid(accent, 0.7);
    const gem = figma.createEllipse();
    gem.resize(10, 10);
    gem.fills = solid(accent);
    glow(gem, accent, 10, 0.7);
    const lineR = figma.createRectangle();
    lineR.resize(Math.floor((w - 40) / 2), 2);
    lineR.fills = solid(accent, 0.7);
    bar.appendChild(lineL);
    bar.appendChild(gem);
    bar.appendChild(lineR);
    lineL.layoutGrow = 1;
    lineR.layoutGrow = 1;
    return bar;
  }

  // —— Pages ——
  async function ensurePages() {
    const names = ["00 Foundations", "01 Components", "02 Screens", "03 States"];
    const map = {};
    for (const n of names) {
      let p = figma.root.children.find((x) => x.name === n);
      if (p) {
        await p.loadAsync();
        for (const c of [...p.children]) c.remove();
      } else {
        p = figma.createPage();
        p.name = n;
        await p.loadAsync();
      }
      map[n] = p;
    }
    // remove default empty Page 1 if unused
    for (const p of [...figma.root.children]) {
      if (names.includes(p.name)) continue;
      await p.loadAsync();
      if (p.children.length === 0 && figma.root.children.length > 1) {
        p.remove();
      }
    }
    return map;
  }

  // —— Variables ——
  async function buildVariables(page) {
    await figma.setCurrentPageAsync(page);
    const existing = await figma.variables.getLocalVariableCollectionsAsync();
    for (const col of existing) {
      if (col.name === "ElvenForge/Primitives" || col.name === "ElvenForge/Semantic") {
        // leave; recreate by new names with timestamp if needed — clean by removing vars
        for (const id of [...col.variableIds]) {
          const v = await figma.variables.getVariableByIdAsync(id);
          if (v) v.remove();
        }
        col.remove();
      }
    }

    const prim = figma.variables.createVariableCollection("ElvenForge/Primitives");
    const mode = prim.modes[0].modeId;
    prim.renameMode(mode, "Default");

    const colorDefs = [
      ["stone", C.stone, ["FRAME_FILL", "SHAPE_FILL"]],
      ["stone-mid", C.stoneMid, ["FRAME_FILL", "SHAPE_FILL"]],
      ["stone-light", C.stoneLight, ["FRAME_FILL", "SHAPE_FILL", "STROKE_COLOR"]],
      ["parchment", C.parchment, ["FRAME_FILL", "SHAPE_FILL", "TEXT_FILL"]],
      ["forest", C.forest, ["FRAME_FILL", "SHAPE_FILL"]],
      ["teal", C.teal, ["FRAME_FILL", "SHAPE_FILL", "STROKE_COLOR", "TEXT_FILL"]],
      ["ember", C.ember, ["FRAME_FILL", "SHAPE_FILL", "TEXT_FILL"]],
      ["moss", C.moss, ["FRAME_FILL", "SHAPE_FILL"]],
      ["health", C.health, ["FRAME_FILL", "SHAPE_FILL", "TEXT_FILL"]],
      ["mithril", C.mithril, ["FRAME_FILL", "SHAPE_FILL", "STROKE_COLOR", "TEXT_FILL"]],
      ["gold", C.gold, ["FRAME_FILL", "SHAPE_FILL", "TEXT_FILL"]],
      ["danger", C.danger, ["FRAME_FILL", "SHAPE_FILL"]],
      ["text-light", C.textLight, ["TEXT_FILL"]],
      ["text-muted", C.textMuted, ["TEXT_FILL"]],
      ["text-dark", C.textDark, ["TEXT_FILL"]],
    ];

    const created = {};
    for (const [name, color, scopes] of colorDefs) {
      const v = figma.variables.createVariable(name, prim, "COLOR");
      v.scopes = scopes;
      v.setValueForMode(mode, { r: color.r, g: color.g, b: color.b, a: 1 });
      created[name] = v;
    }

    const space = figma.variables.createVariableCollection("ElvenForge/Spacing");
    const sm = space.modes[0].modeId;
    space.renameMode(sm, "Default");
    const spaces = [
      ["space-xs", 8],
      ["space-sm", 16],
      ["space-md", 24],
      ["space-lg", 32],
      ["space-xl", 48],
      ["space-2xl", 64],
      ["radius-sm", 12],
      ["radius-md", 20],
      ["radius-lg", 28],
      ["radius-card", 32],
      ["touch-min", 88],
    ];
    for (const [name, val] of spaces) {
      const v = figma.variables.createVariable(name, space, "FLOAT");
      v.scopes = name.startsWith("radius")
        ? ["CORNER_RADIUS"]
        : name === "touch-min"
          ? ["WIDTH_HEIGHT"]
          : ["GAP", "WIDTH_HEIGHT"];
      v.setValueForMode(sm, val);
    }

    // swatch board
    const board = auto("Color Swatches", "VERTICAL", { gap: 16, pad: 32, fills: solid(C.ink) });
    board.x = 80;
    board.y = 80;
    board.appendChild(txt("Elven Forge Codex — Foundations", { font: displayFont, size: 42, color: C.gold }));
    board.appendChild(txt(fontNote, { font: bodyReg, size: 22, color: C.textMuted, width: 900 }));
    const row = auto("Swatches", "HORIZONTAL", { gap: 16 });
    for (const [name, color] of colorDefs) {
      const cell = auto(name, "VERTICAL", { gap: 8, counterAlign: "CENTER" });
      const sw = frame(name, 96, 96, { fills: solid(color), radius: 16, stroke: C.mithril, strokeOpacity: 0.35 });
      cell.appendChild(sw);
      cell.appendChild(txt(name, { font: bodyMed, size: 14, color: C.textMuted, align: "CENTER" }));
      row.appendChild(cell);
    }
    board.appendChild(row);

    const typeSample = auto("Type Samples", "VERTICAL", { gap: 20, pad: 24, fills: solid(C.stoneMid), radius: 20, stroke: C.gold, strokeOpacity: 0.4 });
    typeSample.appendChild(txt("ПОБЕДА!", { font: displayFont, size: 72, color: C.gold }));
    typeSample.appendChild(txt("Climate Crisis — Display / Titles", { font: bodyReg, size: 24, color: C.textMuted }));
    typeSample.appendChild(txt("Нажми чтоб играть · Волна 3/10 · Купить", { font: bodyBold, size: 36, color: C.textLight }));
    typeSample.appendChild(txt("Jura — Body / UI / Buttons", { font: bodyReg, size: 24, color: C.textMuted }));
    board.appendChild(typeSample);
    page.appendChild(board);

    // text styles (TextStyle has no `fills` in Plugin API)
    async function makeTextStyle(name, font, size) {
      const styles = await figma.getLocalTextStylesAsync();
      let s = styles.find((x) => x.name === name);
      if (!s) s = figma.createTextStyle();
      s.name = name;
      s.fontName = { family: font.family, style: font.style };
      s.fontSize = size;
      return s;
    }
    await makeTextStyle("Display/Hero", displayFont, 72);
    await makeTextStyle("Display/Title", displayFont, 56);
    await makeTextStyle("UI/Button", bodyBold, 36);
    await makeTextStyle("UI/Body", bodyReg, 28);
    await makeTextStyle("UI/Caption", bodyMed, 22);
    await makeTextStyle("HUD/Label", bodySemi, 26);

    return created;
  }

  // —— Components ——
  const comps = {};

  function makeButton(name, label, bg, stroke, textColor, w = 420, h = 96) {
    const c = figma.createComponent();
    c.name = name;
    c.resize(w, h);
    c.layoutMode = "HORIZONTAL";
    c.primaryAxisAlignItems = "CENTER";
    c.counterAxisAlignItems = "CENTER";
    c.paddingLeft = 28;
    c.paddingRight = 28;
    c.itemSpacing = 12;
    c.fills = solid(bg);
    c.cornerRadius = 20;
    c.strokes = solid(stroke, 0.85);
    c.strokeWeight = 2.5;
    shadow(c, 6, 18, 0.4);
    const t = txt(label, { font: bodyBold, size: 34, color: textColor, align: "CENTER" });
    c.appendChild(t);
    c.description = "Touch target >=88px. Fantasy forged plate button.";
    return c;
  }

  function makeIconButton(name, glyph, bg, stroke, size = 96) {
    const c = figma.createComponent();
    c.name = name;
    c.resize(size, size);
    c.layoutMode = "HORIZONTAL";
    c.primaryAxisAlignItems = "CENTER";
    c.counterAxisAlignItems = "CENTER";
    c.fills = solid(bg);
    c.cornerRadius = 18;
    c.strokes = solid(stroke, 0.8);
    c.strokeWeight = 2;
    shadow(c, 4, 12, 0.35);
    c.appendChild(txt(glyph, { font: bodyBold, size: 36, color: C.textLight, align: "CENTER" }));
    return c;
  }

  function makeChip(name, label, bg) {
    const c = figma.createComponent();
    c.name = name;
    c.layoutMode = "HORIZONTAL";
    c.primaryAxisSizingMode = "AUTO";
    c.counterAxisSizingMode = "FIXED";
    c.resize(160, 64);
    c.primaryAxisAlignItems = "CENTER";
    c.counterAxisAlignItems = "CENTER";
    c.itemSpacing = 10;
    c.paddingLeft = 18;
    c.paddingRight = 18;
    c.fills = solid(bg, 0.92);
    c.cornerRadius = 999;
    c.strokes = solid(C.gold, 0.55);
    c.strokeWeight = 2;
    const coin = figma.createEllipse();
    coin.resize(28, 28);
    coin.fills = solid(C.gold);
    c.appendChild(coin);
    c.appendChild(txt(label, { font: bodyBold, size: 26, color: C.textLight }));
    return c;
  }

  function makeUpgradeCard(variant, accent, title, desc) {
    const c = figma.createComponent();
    c.name = `UpgradeCard · ${variant}`;
    c.resize(300, 520);
    c.layoutMode = "VERTICAL";
    c.itemSpacing = 16;
    c.paddingTop = 24;
    c.paddingBottom = 24;
    c.paddingLeft = 20;
    c.paddingRight = 20;
    c.primaryAxisAlignItems = "CENTER";
    c.counterAxisAlignItems = "CENTER";
    c.fills = solid(C.stoneMid);
    c.cornerRadius = 28;
    c.strokes = solid(accent, 0.9);
    c.strokeWeight = 3;
    shadow(c, 10, 28, 0.5);
    glow(c, accent, 22, 0.25);

    const gem = figma.createEllipse();
    gem.resize(88, 88);
    gem.fills = solid(accent);
    c.appendChild(gem);

    const iconSlot = frame("IconSlot", 140, 140, {
      fills: solid(C.stone, 0.85),
      radius: 24,
      stroke: accent,
      strokeOpacity: 0.6,
    });
    iconSlot.layoutMode = "HORIZONTAL";
    iconSlot.primaryAxisAlignItems = "CENTER";
    iconSlot.counterAxisAlignItems = "CENTER";
    iconSlot.appendChild(txt("◆", { font: bodyBold, size: 48, color: accent, align: "CENTER" }));
    // replace gem with icon slot - remove gem
    gem.remove();
    c.appendChild(iconSlot);

    c.appendChild(txt(title, { font: bodyBold, size: 28, color: C.textLight, align: "CENTER", width: 250 }));
    c.appendChild(txt(desc, { font: bodyReg, size: 20, color: C.textMuted, align: "CENTER", width: 250 }));

    const useBtn = frame("Use", 240, 72, { fills: solid(accent), radius: 16, stroke: C.mithril, strokeOpacity: 0.4 });
    useBtn.layoutMode = "HORIZONTAL";
    useBtn.primaryAxisAlignItems = "CENTER";
    useBtn.counterAxisAlignItems = "CENTER";
    useBtn.appendChild(txt("Использовать", { font: bodyBold, size: 24, color: C.ink, align: "CENTER" }));
    c.appendChild(useBtn);
    return c;
  }

  function makeShopCard(state, actionLabel, statusLabel) {
    const c = figma.createComponent();
    c.name = `ShopItemCard · ${state}`;
    c.resize(780, 1100);
    c.layoutMode = "VERTICAL";
    c.itemSpacing = 20;
    c.paddingTop = 36;
    c.paddingBottom = 36;
    c.paddingLeft = 32;
    c.paddingRight = 32;
    c.primaryAxisAlignItems = "CENTER";
    c.counterAxisAlignItems = "CENTER";
    c.fills = solid(C.stoneMid);
    c.cornerRadius = 32;
    c.strokes = solid(C.gold, 0.65);
    c.strokeWeight = 3;
    shadow(c, 12, 32, 0.5);

    const icon = frame("IconArea", 420, 420, {
      fills: solid(C.stone, 0.9),
      radius: 28,
      stroke: C.mithril,
      strokeOpacity: 0.35,
    });
    icon.layoutMode = "HORIZONTAL";
    icon.primaryAxisAlignItems = "CENTER";
    icon.counterAxisAlignItems = "CENTER";
    icon.appendChild(txt("⚔", { font: bodyBold, size: 96, color: C.teal, align: "CENTER" }));
    c.appendChild(icon);

    c.appendChild(txt("Скин лучника", { font: bodyBold, size: 40, color: C.textLight, align: "CENTER", width: 680 }));
    c.appendChild(txt("Плащ из листьев Лориэна. Меняет облик героя.", { font: bodyReg, size: 26, color: C.textMuted, align: "CENTER", width: 680 }));
    c.appendChild(txt(statusLabel, { font: bodySemi, size: 24, color: C.gold, align: "CENTER", width: 680 }));

    if (state === "Buy" || state === "IAP") {
      c.appendChild(txt(state === "IAP" ? "★ Premium" : "50 🪙", { font: bodyBold, size: 32, color: C.ember, align: "CENTER" }));
    }

    const action = frame("Action", 520, 96, {
      fills: solid(state === "Equipped" ? C.forest : state === "Owned" ? C.stoneLight : C.tealDeep),
      radius: 20,
      stroke: C.mithril,
      strokeOpacity: 0.5,
    });
    action.layoutMode = "HORIZONTAL";
    action.primaryAxisAlignItems = "CENTER";
    action.counterAxisAlignItems = "CENTER";
    action.appendChild(txt(actionLabel, { font: bodyBold, size: 34, color: C.textLight, align: "CENTER" }));
    c.appendChild(action);
    return c;
  }

  function makeModalPanel() {
    const c = figma.createComponent();
    c.name = "Panel · ModalFrame";
    c.resize(920, 1100);
    c.layoutMode = "VERTICAL";
    c.itemSpacing = 28;
    c.paddingTop = 48;
    c.paddingBottom = 48;
    c.paddingLeft = 40;
    c.paddingRight = 40;
    c.primaryAxisAlignItems = "CENTER";
    c.counterAxisAlignItems = "CENTER";
    c.fills = solid(C.stoneMid, 0.97);
    c.cornerRadius = 36;
    c.strokes = solid(C.gold, 0.7);
    c.strokeWeight = 3;
    shadow(c, 16, 40, 0.55);
    c.appendChild(ornamentBar(780, C.gold));
    c.appendChild(txt("Заголовок", { font: displayFont, size: 56, color: C.gold, align: "CENTER", name: "Title" }));
    c.appendChild(ornamentBar(780, C.mithril));
    const slot = frame("ContentSlot", 820, 700, { fills: solid(C.stone, 0.35), radius: 20 });
    c.appendChild(slot);
    return c;
  }

  function makeHUDWidgets(page) {
    let x = 80;
    let y = 80;
    let rowH = 0;

    function placeWidget(node) {
      page.appendChild(node);
      if (x + node.width > 2400) {
        x = 80;
        y += rowH + 48;
        rowH = 0;
      }
      node.x = x;
      node.y = y;
      x += node.width + 40;
      rowH = Math.max(rowH, node.height);
      comps[node.name] = node;
      return node;
    }

    // XP Slider
    const xp = figma.createComponent();
    xp.name = "HUD · XPBar";
    xp.resize(640, 72);
    xp.layoutMode = "VERTICAL";
    xp.itemSpacing = 6;
    xp.fills = [];
    const xpRow = auto("Labels", "HORIZONTAL", { gap: 12, primaryAlign: "SPACE_BETWEEN", w: 640, primary: "FIXED" });
    xpRow.appendChild(txt("LVL 7", { font: bodyBold, size: 26, color: C.gold }));
    xpRow.appendChild(txt("1240 / 2000", { font: bodyMed, size: 22, color: C.mithril }));
    xp.appendChild(xpRow);
    const track = frame("Track", 640, 28, { fills: solid(C.ink, 0.75), radius: 14, stroke: C.mithril, strokeOpacity: 0.35 });
    const fill = frame("Fill", 380, 28, { fills: solid(C.teal), radius: 14 });
    track.appendChild(fill);
    xp.appendChild(track);
    placeWidget(xp);

    const wave = figma.createComponent();
    wave.name = "HUD · WaveLabel";
    wave.layoutMode = "VERTICAL";
    wave.itemSpacing = 4;
    wave.fills = solid(C.stone, 0.55);
    wave.cornerRadius = 16;
    wave.paddingLeft = 20;
    wave.paddingRight = 20;
    wave.paddingTop = 12;
    wave.paddingBottom = 12;
    wave.strokes = solid(C.mithril, 0.3);
    wave.strokeWeight = 1.5;
    wave.appendChild(txt("Волна: 3/10", { font: bodySemi, size: 26, color: C.textLight }));
    wave.appendChild(txt("Врагов: 4/12 (Осталось: 8)", { font: bodyReg, size: 20, color: C.textMuted }));
    placeWidget(wave);

    function cooldown(name, accent, label) {
      const cell = figma.createComponent();
      cell.name = name;
      cell.resize(120, 120);
      cell.layoutMode = "VERTICAL";
      cell.primaryAxisAlignItems = "CENTER";
      cell.counterAxisAlignItems = "CENTER";
      cell.itemSpacing = 4;
      cell.fills = solid(C.stone, 0.7);
      cell.cornerRadius = 20;
      cell.strokes = solid(accent, 0.85);
      cell.strokeWeight = 3;
      const ring = figma.createEllipse();
      ring.resize(64, 64);
      ring.fills = solid(accent, 0.25);
      ring.strokes = solid(accent);
      ring.strokeWeight = 4;
      cell.appendChild(ring);
      cell.appendChild(txt(label, { font: bodyMed, size: 16, color: C.textMuted, align: "CENTER" }));
      return placeWidget(cell);
    }
    cooldown("HUD · CooldownArrow", C.teal, "Лук");
    cooldown("HUD · CooldownFire", C.ember, "Огонь");
    cooldown("HUD · CooldownPoison", C.moss, "Яд");

    const hp = figma.createComponent();
    hp.name = "Feedback · HealthBar";
    hp.resize(280, 40);
    hp.fills = solid(C.ink, 0.7);
    hp.cornerRadius = 10;
    hp.strokes = solid(C.mithril, 0.3);
    hp.strokeWeight = 1;
    const hpFill = frame("Fill", 200, 40, { fills: solid(C.health), radius: 10 });
    hp.appendChild(hpFill);
    const hpText = txt("120", { font: bodyBold, size: 20, color: C.ink });
    hpText.x = 110;
    hpText.y = 8;
    hp.appendChild(hpText);
    placeWidget(hp);

    const dmg = figma.createComponent();
    dmg.name = "Feedback · DamagePopup";
    dmg.layoutMode = "HORIZONTAL";
    dmg.fills = [];
    dmg.appendChild(txt("-42", { font: bodyBold, size: 40, color: C.dangerBright }));
    placeWidget(dmg);

    const joy = figma.createComponent();
    joy.name = "HUD · JoystickHint";
    joy.resize(220, 220);
    joy.fills = solid(C.stone, 0.25);
    joy.cornerRadius = 999;
    joy.strokes = solid(C.mithril, 0.35);
    joy.strokeWeight = 3;
    const knob = figma.createEllipse();
    knob.resize(88, 88);
    knob.x = 66;
    knob.y = 66;
    knob.fills = solid(C.mithril, 0.55);
    joy.appendChild(knob);
    placeWidget(joy);

    const progress = figma.createComponent();
    progress.name = "Misc · ProgressSlider";
    progress.resize(720, 28);
    progress.fills = solid(C.ink, 0.8);
    progress.cornerRadius = 14;
    progress.strokes = solid(C.gold, 0.4);
    progress.strokeWeight = 2;
    const pf = frame("Fill", 430, 28, { fills: solid(C.gold), radius: 14 });
    progress.appendChild(pf);
    placeWidget(progress);

    const hand = figma.createComponent();
    hand.name = "Misc · TutorialHand";
    hand.resize(120, 120);
    hand.fills = [];
    hand.layoutMode = "HORIZONTAL";
    hand.primaryAxisAlignItems = "CENTER";
    hand.counterAxisAlignItems = "CENTER";
    hand.appendChild(txt("👆", { font: bodyBold, size: 64, align: "CENTER" }));
    placeWidget(hand);

    const dim = figma.createComponent();
    dim.name = "Misc · DimOverlay";
    dim.resize(W, H);
    dim.fills = solid(C.black, 0.62);
    placeWidget(dim);

    const tab = figma.createComponent();
    tab.name = "Tab · Default";
    tab.resize(280, 72);
    tab.layoutMode = "HORIZONTAL";
    tab.primaryAxisAlignItems = "CENTER";
    tab.counterAxisAlignItems = "CENTER";
    tab.fills = solid(C.stoneLight);
    tab.cornerRadius = 16;
    tab.strokes = solid(C.mithril, 0.4);
    tab.strokeWeight = 2;
    tab.appendChild(txt("Скины", { font: bodyBold, size: 28, color: C.textLight }));
    placeWidget(tab);

    const tabOn = figma.createComponent();
    tabOn.name = "Tab · Active";
    tabOn.resize(280, 72);
    tabOn.layoutMode = "HORIZONTAL";
    tabOn.primaryAxisAlignItems = "CENTER";
    tabOn.counterAxisAlignItems = "CENTER";
    tabOn.fills = solid(C.tealDeep);
    tabOn.cornerRadius = 16;
    tabOn.strokes = solid(C.gold, 0.7);
    tabOn.strokeWeight = 2;
    tabOn.appendChild(txt("Скины", { font: bodyBold, size: 28, color: C.textLight }));
    placeWidget(tabOn);

    const lang = figma.createComponent();
    lang.name = "Chip · Language";
    lang.resize(96, 64);
    lang.layoutMode = "HORIZONTAL";
    lang.primaryAxisAlignItems = "CENTER";
    lang.counterAxisAlignItems = "CENTER";
    lang.fills = solid(C.stone, 0.75);
    lang.cornerRadius = 14;
    lang.strokes = solid(C.mithril, 0.45);
    lang.strokeWeight = 2;
    lang.appendChild(txt("RU", { font: bodyBold, size: 26, color: C.gold }));
    placeWidget(lang);
  }

  async function buildComponents(page) {
    await figma.setCurrentPageAsync(page);
    await page.loadAsync();

    const title = txt("01 Components — Elven Forge Codex", { font: displayFont, size: 48, color: C.gold });
    title.x = 80;
    title.y = 40;
    page.appendChild(title);

    function placeButton(node) {
      page.appendChild(node);
      node.x = x;
      node.y = y;
      x += node.width + 32;
      comps[node.name] = node;
      return node;
    }

    let x = 80;
    let y = 140;
    placeButton(makeButton("Button · Primary", "Продолжить", C.forest, C.gold, C.textLight));
    placeButton(makeButton("Button · Secondary", "Заново", C.stoneLight, C.mithril, C.textLight));
    placeButton(makeButton("Button · Danger", "Выйти", C.danger, C.ember, C.textLight));
    placeButton(makeButton("Button · Reward", "▶ +25 ОПЫТА", C.reward, C.gold, C.ink));
    placeButton(makeButton("Button · Skip", "Пропустить", C.stone, C.textMuted, C.textMuted, 360, 88));
    placeButton(makeIconButton("Button · Pause", "❚❚", C.stone, C.mithril));
    placeButton(makeIconButton("Button · Close", "✕", C.stoneMid, C.dangerBright));
    placeButton(makeIconButton("Button · MuteOn", "♪", C.stoneLight, C.teal));
    placeButton(makeIconButton("Button · MuteOff", "mute", C.stone, C.textMuted, 96));
    placeButton(makeChip("Chip · Coins", "1280", C.stone));

    y = 320;
    x = 80;
    const modal = makeModalPanel();
    page.appendChild(modal);
    modal.x = x;
    modal.y = y;
    comps[modal.name] = modal;

    x = 1100;
    const upgradeDefs = [
      ["Teal", C.teal, "Больше стрел!", "Увеличивает число стрел"],
      ["Ember", C.ember, "Открыть огонь", "Разблокирует огненные стрелы"],
      ["Moss", C.moss, "Добавить Яда", "Разблокирует яд"],
      ["Health", C.health, "Будь Здоров!", "Увеличивает макс. здоровье"],
    ];
    for (const [variant, accent, titleText, desc] of upgradeDefs) {
      const card = makeUpgradeCard(variant, accent, titleText, desc);
      page.appendChild(card);
      card.x = x;
      card.y = y;
      x += card.width + 28;
      comps[card.name] = card;
    }

    y = 920;
    x = 80;
    const shopDefs = [
      ["Buy", "Купить", "Доступно"],
      ["Equip", "Надеть", "Куплено"],
      ["Owned", "Куплено", "Куплено"],
      ["Equipped", "Надето", "Надето"],
      ["IAP", "Купить ★", "Premium"],
    ];
    for (const [state, actionLabel, statusLabel] of shopDefs) {
      const card = makeShopCard(state, actionLabel, statusLabel);
      page.appendChild(card);
      card.x = x;
      card.y = y;
      comps[card.name] = card;
      // Preview instance scaled down; keep master full-size for screens
      const preview = card.createInstance();
      preview.rescale(0.35);
      page.appendChild(preview);
      preview.x = x;
      preview.y = y;
      card.x = 3200;
      card.y = y;
      x += preview.width + 28;
    }

    makeHUDWidgets(page);

    // Settings slider block component
    const settings = figma.createComponent();
    settings.name = "Panel · SettingsBlock";
    settings.resize(760, 220);
    settings.layoutMode = "VERTICAL";
    settings.itemSpacing = 20;
    settings.paddingTop = 24;
    settings.paddingBottom = 24;
    settings.paddingLeft = 24;
    settings.paddingRight = 24;
    settings.fills = solid(C.stone, 0.6);
    settings.cornerRadius = 20;
    settings.strokes = solid(C.mithril, 0.3);
    settings.strokeWeight = 2;

    function sliderRow(label) {
      const row = auto(label, "HORIZONTAL", { gap: 16, counterAlign: "CENTER", w: 700, primary: "FIXED" });
      const icon = frame("Mute", 72, 72, { fills: solid(C.stoneLight), radius: 14, stroke: C.teal, strokeOpacity: 0.6 });
      icon.layoutMode = "HORIZONTAL";
      icon.primaryAxisAlignItems = "CENTER";
      icon.counterAxisAlignItems = "CENTER";
      icon.appendChild(txt("♪", { font: bodyBold, size: 28, color: C.textLight }));
      row.appendChild(icon);
      row.appendChild(txt(label, { font: bodySemi, size: 26, color: C.textLight }));
      const track = frame("Slider", 360, 20, { fills: solid(C.ink), radius: 10, stroke: C.mithril, strokeOpacity: 0.3 });
      const fill = frame("Fill", 220, 20, { fills: solid(C.teal), radius: 10 });
      track.appendChild(fill);
      row.appendChild(track);
      return row;
    }
    settings.appendChild(sliderRow("Музыка"));
    settings.appendChild(sliderRow("Звуки"));
    page.appendChild(settings);
    settings.x = 80;
    settings.y = 1600;
    comps[settings.name] = settings;

    return comps;
  }

  function instanceOf(name) {
    const c = comps[name];
    if (!c) throw new Error("Missing component: " + name);
    return c.createInstance();
  }

  function phoneFrame(name, bg) {
    const f = frame(name, W, H, { fills: solid(bg || C.ink), clips: true });
    // subtle forest vignette layers
    const vignette = frame("Vignette", W, H, { fills: solid(C.forest, 0.18) });
    f.appendChild(vignette);
    const mist = frame("Mist", W, 520, { fills: solid(C.stoneMid, 0.35) });
    mist.y = H - 520;
    f.appendChild(mist);
    return f;
  }

  function place(parent, node, x, y) {
    parent.appendChild(node);
    node.x = x;
    node.y = y;
    return node;
  }

  async function buildScreens(page) {
    await figma.setCurrentPageAsync(page);
    const screens = [];
    let ox = 0;

    // 1 Pre-Start
    {
      const s = phoneFrame("01 Pre-Start");
      place(s, instanceOf("Chip · Language"), 64, 64);
      place(s, instanceOf("Chip · Coins"), W - 64 - 180, 64);
      const shopBtn = instanceOf("Button · Primary");
      // shrink label via — use secondary sized
      place(s, shopBtn, 64, 160);
      shopBtn.resize(280, 88);
      // overwrite text
      const shopText = shopBtn.findOne((n) => n.type === "TEXT");
      if (shopText) {
        await figma.loadFontAsync(shopText.fontName);
        shopText.characters = "МАГАЗИН";
      }

      const level = txt("Уровень 1", { font: displayFont, size: 64, color: C.gold, align: "CENTER", width: 900 });
      place(s, level, 90, 720);
      const tap = txt("Нажми чтоб играть", { font: bodyBold, size: 40, color: C.textLight, align: "CENTER", width: 900 });
      place(s, tap, 90, 820);
      place(s, instanceOf("Misc · TutorialHand"), W / 2 - 60, 980);
      place(s, ornamentBar(720, C.mithril), 180, 1100);

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
      ox += W + 80;
    }

    // 2 Shop
    {
      const s = phoneFrame("02 Shop");
      place(s, instanceOf("Misc · DimOverlay"), 0, 0);
      const window = frame("ShopWindow", 980, 1680, {
        fills: solid(C.stoneMid, 0.98),
        radius: 36,
        stroke: C.gold,
        strokeOpacity: 0.7,
      });
      place(s, window, 50, 120);
      window.layoutMode = "VERTICAL";
      window.itemSpacing = 20;
      window.paddingTop = 28;
      window.paddingBottom = 28;
      window.paddingLeft = 28;
      window.paddingRight = 28;
      window.primaryAxisAlignItems = "CENTER";
      window.counterAxisAlignItems = "CENTER";

      const head = auto("Head", "HORIZONTAL", { gap: 16, primaryAlign: "SPACE_BETWEEN", counterAlign: "CENTER", w: 900, primary: "FIXED" });
      head.appendChild(txt("МАГАЗИН", { font: displayFont, size: 48, color: C.gold }));
      const close = instanceOf("Button · Close");
      head.appendChild(close);
      window.appendChild(head);
      window.appendChild(instanceOf("Chip · Coins"));

      const tabs = auto("Tabs", "HORIZONTAL", { gap: 12 });
      tabs.appendChild(instanceOf("Tab · Active"));
      const t2 = instanceOf("Tab · Default");
      const t2t = t2.findOne((n) => n.type === "TEXT");
      if (t2t) {
        await figma.loadFontAsync(t2t.fontName);
        t2t.characters = "Снаряды";
      }
      tabs.appendChild(t2);
      const t3 = instanceOf("Tab · Default");
      const t3t = t3.findOne((n) => n.type === "TEXT");
      if (t3t) {
        await figma.loadFontAsync(t3t.fontName);
        t3t.characters = "Шляпы";
      }
      tabs.appendChild(t3);
      window.appendChild(tabs);

      const card = instanceOf("ShopItemCard · Buy");
      card.rescale(0.85);
      window.appendChild(card);

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
      ox += W + 80;
    }

    // 3 HUD
    {
      const s = phoneFrame("03 Gameplay HUD", C.stone);
      // gameplay bg hint
      const playfield = frame("PlayfieldHint", W, H, { fills: solid(hex("1F3A2A"), 0.9) });
      s.insertChild(0, playfield);

      place(s, instanceOf("HUD · XPBar"), (W - 640) / 2, 64);
      place(s, instanceOf("Button · Pause"), W - 64 - 96, 56);
      place(s, instanceOf("HUD · WaveLabel"), 64, 160);

      const cds = auto("Cooldowns", "HORIZONTAL", { gap: 16 });
      cds.appendChild(instanceOf("HUD · CooldownArrow"));
      cds.appendChild(instanceOf("HUD · CooldownFire"));
      cds.appendChild(instanceOf("HUD · CooldownPoison"));
      place(s, cds, W - 64 - 400, 160);

      place(s, instanceOf("HUD · JoystickHint"), 80, H - 320);
      place(s, instanceOf("Feedback · HealthBar"), W / 2 - 140, 280);
      place(s, instanceOf("Feedback · DamagePopup"), W / 2 + 80, 360);

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
      ox += W + 80;
    }

    // 4 Pause
    {
      const s = phoneFrame("04 Pause");
      place(s, instanceOf("Misc · DimOverlay"), 0, 0);
      const panel = frame("PausePanel", 920, 980, {
        fills: solid(C.stoneMid, 0.97),
        radius: 36,
        stroke: C.gold,
        strokeOpacity: 0.75,
      });
      panel.layoutMode = "VERTICAL";
      panel.itemSpacing = 28;
      panel.paddingTop = 48;
      panel.paddingBottom = 48;
      panel.paddingLeft = 40;
      panel.paddingRight = 40;
      panel.primaryAxisAlignItems = "CENTER";
      panel.counterAxisAlignItems = "CENTER";
      panel.appendChild(ornamentBar(780, C.gold));
      panel.appendChild(txt("ПАУЗА", { font: displayFont, size: 64, color: C.gold, align: "CENTER" }));
      panel.appendChild(ornamentBar(780, C.mithril));
      panel.appendChild(instanceOf("Button · Primary"));
      const restart = instanceOf("Button · Secondary");
      const rt = restart.findOne((n) => n.type === "TEXT");
      if (rt) {
        await figma.loadFontAsync(rt.fontName);
        rt.characters = "Заново";
      }
      panel.appendChild(restart);
      panel.appendChild(instanceOf("Panel · SettingsBlock"));
      place(s, panel, (W - 920) / 2, 420);
      // fix primary label
      const resume = panel.children.find((n) => n.name === "Button · Primary" || (n.type === "INSTANCE" && n.mainComponent && n.mainComponent.name === "Button · Primary"));
      // find by type
      for (const ch of panel.findAll((n) => n.type === "INSTANCE")) {
        if (ch.mainComponent && ch.mainComponent.name === "Button · Primary") {
          const t = ch.findOne((n) => n.type === "TEXT");
          if (t) {
            await figma.loadFontAsync(t.fontName);
            t.characters = "Продолжить";
          }
        }
      }

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
      ox += W + 80;
    }

    // 5 Upgrade
    {
      const s = phoneFrame("05 Upgrade Choice");
      place(s, instanceOf("Misc · DimOverlay"), 0, 0);
      const title = txt("Выбери усиление", { font: displayFont, size: 48, color: C.gold, align: "CENTER", width: 960 });
      place(s, title, 60, 280);
      const row = auto("Cards", "HORIZONTAL", { gap: 24 });
      row.appendChild(instanceOf("UpgradeCard · Teal"));
      row.appendChild(instanceOf("UpgradeCard · Ember"));
      row.appendChild(instanceOf("UpgradeCard · Moss"));
      place(s, row, (W - 948) / 2, 420);
      place(s, instanceOf("Button · Skip"), (W - 360) / 2, 1040);

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
      ox += W + 80;
    }

    // 6 Win
    {
      const s = phoneFrame("06 Win");
      place(s, instanceOf("Misc · DimOverlay"), 0, 0);
      const panel = frame("WinPanel", 920, 900, {
        fills: solid(C.stoneMid, 0.97),
        radius: 36,
        stroke: C.gold,
        strokeOpacity: 0.85,
      });
      panel.layoutMode = "VERTICAL";
      panel.itemSpacing = 28;
      panel.paddingTop = 48;
      panel.paddingBottom = 48;
      panel.paddingLeft = 40;
      panel.paddingRight = 40;
      panel.primaryAxisAlignItems = "CENTER";
      panel.appendChild(ornamentBar(780, C.gold));
      panel.appendChild(txt("ПОБЕДА!", { font: displayFont, size: 72, color: C.gold, align: "CENTER" }));
      panel.appendChild(ornamentBar(780, C.ember));
      const next = instanceOf("Button · Primary");
      const nt = next.findOne((n) => n.type === "TEXT");
      if (nt) {
        await figma.loadFontAsync(nt.fontName);
        nt.characters = "Дальше";
      }
      panel.appendChild(next);
      const again = instanceOf("Button · Secondary");
      const at = again.findOne((n) => n.type === "TEXT");
      if (at) {
        await figma.loadFontAsync(at.fontName);
        at.characters = "Заново";
      }
      panel.appendChild(again);
      panel.appendChild(instanceOf("Button · Reward"));
      place(s, panel, (W - 920) / 2, 480);

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
      ox += W + 80;
    }

    // 7 Lose
    {
      const s = phoneFrame("07 Lose");
      place(s, instanceOf("Misc · DimOverlay"), 0, 0);
      const panel = frame("LosePanel", 920, 820, {
        fills: solid(C.stoneMid, 0.97),
        radius: 36,
        stroke: C.dangerBright,
        strokeOpacity: 0.8,
      });
      panel.layoutMode = "VERTICAL";
      panel.itemSpacing = 28;
      panel.paddingTop = 48;
      panel.paddingBottom = 48;
      panel.paddingLeft = 40;
      panel.paddingRight = 40;
      panel.primaryAxisAlignItems = "CENTER";
      panel.appendChild(ornamentBar(780, C.dangerBright));
      panel.appendChild(txt("ПРОИГРЫШ!", { font: displayFont, size: 64, color: C.dangerBright, align: "CENTER" }));
      panel.appendChild(ornamentBar(780, C.mithril));
      const again = instanceOf("Button · Secondary");
      const at = again.findOne((n) => n.type === "TEXT");
      if (at) {
        await figma.loadFontAsync(at.fontName);
        at.characters = "Заново";
      }
      panel.appendChild(again);
      const revive = instanceOf("Button · Reward");
      const rv = revive.findOne((n) => n.type === "TEXT");
      if (rv) {
        await figma.loadFontAsync(rv.fontName);
        rv.characters = "▶ Воскреснуть";
      }
      panel.appendChild(revive);
      place(s, panel, (W - 920) / 2, 520);

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
      ox += W + 80;
    }

    // 8 Loading
    {
      const s = phoneFrame("08 Loading");
      const title = txt("The Lord of the Bow", { font: displayFont, size: 48, color: C.gold, align: "CENTER", width: 900 });
      place(s, title, 90, 720);
      place(s, instanceOf("Misc · ProgressSlider"), (W - 720) / 2, 880);
      place(s, txt("Загрузка мира…", { font: bodyMed, size: 28, color: C.textMuted, align: "CENTER", width: 900 }), 90, 940);
      place(s, ornamentBar(640, C.mithril), 220, 1080);

      s.x = ox;
      s.y = 0;
      page.appendChild(s);
      screens.push(s);
    }

    return screens;
  }

  async function buildStates(page) {
    await figma.setCurrentPageAsync(page);
    const title = txt("03 States", { font: displayFont, size: 48, color: C.gold });
    title.x = 80;
    title.y = 40;
    page.appendChild(title);

    const items = [
      ["Shop Equipped", "ShopItemCard · Equipped"],
      ["Shop IAP", "ShopItemCard · IAP"],
      ["Mute Off", "Button · MuteOff"],
      ["Cooldown Fire", "HUD · CooldownFire"],
      ["Reward Ad", "Button · Reward"],
    ];
    let x = 80;
    let y = 140;
    for (const [label, name] of items) {
      const wrap = auto(label, "VERTICAL", { gap: 12 });
      wrap.appendChild(txt(label, { font: bodySemi, size: 22, color: C.textMuted }));
      const inst = instanceOf(name);
      if (name.startsWith("Shop")) inst.rescale(0.4);
      wrap.appendChild(inst);
      page.appendChild(wrap);
      wrap.x = x;
      wrap.y = y;
      x += wrap.width + 48;
    }
  }

  // —— Main ——
  await resolveFonts();
  const pages = await ensurePages();
  await buildVariables(pages["00 Foundations"]);
  await buildComponents(pages["01 Components"]);
  await buildScreens(pages["02 Screens"]);
  await buildStates(pages["03 States"]);
  await figma.setCurrentPageAsync(pages["02 Screens"]);

  figma.closePlugin("Elven Forge Codex UI built. " + fontNote);
})().catch((err) => {
  figma.closePlugin("Error: " + (err && err.message ? err.message : String(err)));
});
