// Witch Knight bestiary rig: the Greek reskins of Hallownest's creatures.
// Each creature draws itself facing right from a pose object, like the Hex Moth rig,
// so the same code feeds the preview page and export_bestiary.py.
(function (root) {
  const C = {
    ink: '#0d0f17', pale: '#e9e6f2', paleShade: '#c3bdd6', bone: '#e8e0cc', boneShade: '#b9ac90',
    bronzeLight: '#e0b56a', bronze: '#b07a3a', bronzeDark: '#6e4520', wood: '#3a2c20',
    crimson: '#a3283a', crimsonDark: '#5a1622', ember: '#ff8a3d', emberCore: '#ffe0a0',
    husk: '#6a6158', huskLight: '#8d8376', huskDark: '#2f2a26', hide: '#3a2a22', hideDark: '#1e1612',
    plum: '#3b2340', plumLight: '#6b4577', plumDark: '#1e1022', feather: '#4c3a66', featherTip: '#9b86b8',
    grub: '#ece4c8', grubShade: '#c4b68a',
  };
  const TAU = Math.PI * 2;

  function bronzeFill(ctx, x0, y0, x1, y1) {
    const g = ctx.createLinearGradient(x0, y0, x1, y1);
    g.addColorStop(0, C.bronzeLight); g.addColorStop(0.55, C.bronze); g.addColorStop(1, C.bronzeDark);
    return g;
  }
  function grad(ctx, x0, y0, x1, y1, a, b) {
    const g = ctx.createLinearGradient(x0, y0, x1, y1); g.addColorStop(0, a); g.addColorStop(1, b); return g;
  }
  function outline(ctx, w) { ctx.lineWidth = w || 2; ctx.strokeStyle = C.ink; ctx.lineJoin = 'round'; ctx.stroke(); }
  function disc(ctx, x, y, r, fill, line) {
    ctx.beginPath(); ctx.arc(x, y, r, 0, TAU); ctx.fillStyle = fill; ctx.fill(); if (line) outline(ctx, line);
  }
  function glow(ctx, x, y, r, col, blur) {
    ctx.save(); ctx.shadowColor = col; ctx.shadowBlur = blur; disc(ctx, x, y, r, col); ctx.restore();
  }

  // A jointed limb drawn as an outlined stroke through points.
  function limb(ctx, pts, w, col) {
    ctx.lineCap = 'round'; ctx.lineJoin = 'round';
    for (const [lw, c] of [[w + 2.5, C.ink], [w, col]]) {
      ctx.strokeStyle = c; ctx.lineWidth = lw; ctx.beginPath(); ctx.moveTo(...pts[0]);
      for (const p of pts.slice(1)) ctx.lineTo(...p);
      ctx.stroke();
    }
  }

  // Thin insect leg from the hip: thigh forward, shin back, small claw.
  function bugLeg(ctx, x, y, angle, len, w, col) {
    ctx.save(); ctx.translate(x, y); ctx.rotate(angle);
    const knee = [len * 0.22, len * 0.5], ankle = [-len * 0.04, len], toe = [len * 0.3, len * 1.08];
    limb(ctx, [[0, 0], knee, ankle, toe], w, col);
    ctx.restore();
  }

  function toothedGear(ctx, x, y, r, a, fill) {
    ctx.save(); ctx.translate(x, y); ctx.rotate(a);
    ctx.beginPath();
    const n = 8;
    for (let i = 0; i < n * 2; i++) {
      const rr = i % 2 ? r : r * 1.28, t0 = i / (n * 2) * TAU, t1 = (i + 1) / (n * 2) * TAU;
      ctx.arc(0, 0, rr, t0, t1);
    }
    ctx.closePath(); ctx.fillStyle = fill || bronzeFill(ctx, -r, -r, r, r); ctx.fill(); outline(ctx, 1);
    disc(ctx, 0, 0, r * 0.35, C.bronzeDark);
    ctx.restore();
  }

  function cycle(n, fn) { return Array.from({ length: n }, (_, i) => fn(i / n * TAU, i)); }
  function keys(base, n, ks) {
    return Array.from({ length: n }, (_, i) => {
      const t = n === 1 ? 0 : i / (n - 1);
      let j = 0; while (j < ks.length - 2 && t > ks[j + 1][0]) j++;
      const [t0, a] = ks[j], [t1, b] = ks[j + 1], u = Math.min(1, Math.max(0, (t - t0) / (t1 - t0 || 1)));
      const e = u * u * (3 - 2 * u), out = {};
      for (const k of new Set([...Object.keys(a), ...Object.keys(b)])) {
        const va = k in a ? a[k] : base[k], vb = k in b ? b[k] : base[k];
        out[k] = va + (vb - va) * e;
      }
      return out;
    });
  }

  // Shared frame wrapper: origin, body transform, then a white hit flash over the figure.
  function frame(FRAME, BASE, body) {
    return function draw(ctx, pose) {
      const p = Object.assign({}, BASE, pose);
      ctx.save();
      ctx.beginPath(); ctx.rect(0, 0, FRAME.w, FRAME.h); ctx.clip();
      ctx.save();
      ctx.globalAlpha = p.alpha;
      ctx.translate(FRAME.originX + p.dx, FRAME.originY + p.bob);
      ctx.rotate(p.rot);
      ctx.scale(1 + p.squash * 0.08, 1 - p.squash * 0.1);
      body(ctx, p);
      ctx.restore();
      if (p.flash > 0) {
        ctx.globalCompositeOperation = 'source-atop'; ctx.fillStyle = `rgba(255,255,255,${p.flash})`;
        ctx.fillRect(0, 0, FRAME.w, FRAME.h);
      }
      ctx.restore();
    };
  }

  // ---------------------------------------------------------------------------------------
  // Hoplite Husk: a hollow beetle-soldier in a crested bronze helmet, aspis and spear.
  // ---------------------------------------------------------------------------------------
  const hoplite = (() => {
    const FRAME = { w: 300, h: 260, originX: 140, originY: 232 };
    const BASE = { bob: 0, tilt: 0, legBack: 0, legFront: 0, spear: 0, thrust: 0, shieldUp: 0, head: 0,
      crest: 0, squash: 0, rot: 0, dx: 0, alpha: 1, flash: 0, helmOff: 0, eye: 1 };

    function crestedHelmet(ctx, p) {
      // horsehair crest on a stand, front to back
      ctx.save(); ctx.rotate(p.crest);
      ctx.beginPath(); ctx.moveTo(10, -26);
      ctx.bezierCurveTo(14, -58, -30, -66, -46, -30);
      ctx.bezierCurveTo(-40, -22, -36, -22, -30, -26);
      ctx.bezierCurveTo(-24, -46, 0, -48, 2, -26); ctx.closePath();
      ctx.fillStyle = grad(ctx, 0, -64, 0, -24, C.crimson, C.crimsonDark); ctx.fill(); outline(ctx, 1.6);
      ctx.strokeStyle = '#c84a5a'; ctx.lineWidth = 1;
      for (const [x0, y0, x1, y1] of [[6, -34, 2, -52], [-4, -36, -12, -54], [-14, -34, -26, -48], [-24, -30, -38, -36]]) {
        ctx.beginPath(); ctx.moveTo(x0, y0); ctx.lineTo(x1, y1); ctx.stroke();
      }
      ctx.restore();
      // Attic helmet: dome, short brim, hinged cheek guard
      ctx.beginPath();
      ctx.moveTo(-26, 10); ctx.bezierCurveTo(-30, -14, -18, -28, 2, -28);
      ctx.bezierCurveTo(22, -28, 30, -12, 28, 2); ctx.lineTo(32, 4); ctx.lineTo(26, 8);
      ctx.lineTo(-20, 12); ctx.closePath();
      ctx.fillStyle = bronzeFill(ctx, -20, -28, 24, 12); ctx.fill(); outline(ctx, 1.6);
      ctx.beginPath(); ctx.moveTo(-12, 6); ctx.lineTo(4, 8); ctx.lineTo(2, 24); ctx.quadraticCurveTo(-6, 28, -12, 22); ctx.closePath();
      ctx.fillStyle = bronzeFill(ctx, -12, 6, 4, 26); ctx.fill(); outline(ctx, 1.3);
      ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 1.2;
      ctx.beginPath(); ctx.moveTo(-24, -4); ctx.bezierCurveTo(-6, -10, 12, -10, 28, -2); ctx.stroke();
    }

    function body(ctx, p) {
      ctx.rotate(p.tilt);
      // spear, held overhand by the far arm, behind the body
      ctx.save();
      ctx.translate(-4 + p.thrust, -92);
      ctx.rotate(0.12 + p.spear);
      limb(ctx, [[-14, 6], [-2, -4]], 5, C.huskDark);
      ctx.strokeStyle = C.wood; ctx.lineWidth = 3.4; ctx.lineCap = 'round';
      ctx.beginPath(); ctx.moveTo(-70, 0); ctx.lineTo(70, 0); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(70, -4); ctx.bezierCurveTo(80, -5, 90, -3, 100, 0); ctx.bezierCurveTo(90, 3, 80, 5, 70, 4); ctx.closePath();
      ctx.fillStyle = bronzeFill(ctx, 70, -5, 100, 5); ctx.fill(); outline(ctx, 1.1);
      ctx.beginPath(); ctx.moveTo(-70, -2); ctx.lineTo(-79, 0); ctx.lineTo(-70, 2); ctx.closePath(); ctx.fillStyle = C.bronze; ctx.fill();
      disc(ctx, -2, -3, 4.5, C.huskDark, 1.1);
      ctx.restore();

      // legs
      bugLeg(ctx, -12, -34, p.legBack, 34, 5, C.huskDark);
      // shell body
      ctx.beginPath(); ctx.ellipse(0, -66, 32, 38, 0.12, 0, TAU);
      ctx.fillStyle = grad(ctx, -20, -104, 20, -30, C.huskLight, C.huskDark); ctx.fill(); outline(ctx, 2);
      ctx.strokeStyle = C.husk; ctx.lineWidth = 1.4;
      for (const y of [-80, -64, -48]) { ctx.beginPath(); ctx.ellipse(-4, y, 26, 6, 0.1, 0.3, Math.PI - 0.3); ctx.stroke(); }
      // leather pteruges skirt
      for (let i = 0; i < 6; i++) {
        const x = -22 + i * 8.5;
        ctx.beginPath(); ctx.rect(x, -40, 7, 14 - Math.abs(i - 2.5) * 1.5);
        ctx.fillStyle = i % 2 ? C.crimsonDark : C.crimson; ctx.fill(); outline(ctx, 1);
        disc(ctx, x + 3.5, -28 - Math.abs(i - 2.5) * 1.5, 1.4, C.bronzeLight);
      }
      ctx.fillStyle = C.bronzeDark; ctx.fillRect(-24, -45, 50, 6); ctx.strokeStyle = C.ink; ctx.lineWidth = 1; ctx.strokeRect(-24, -45, 50, 6);
      bugLeg(ctx, 10, -34, p.legFront, 34, 5.5, C.husk);

      // head
      ctx.save();
      ctx.translate(16, -104); ctx.rotate(p.head);
      ctx.beginPath(); ctx.ellipse(6, 4, 19, 16, 0.2, 0, TAU); ctx.fillStyle = C.pale; ctx.fill(); outline(ctx, 1.5);
      // mandibles
      ctx.strokeStyle = C.huskDark; ctx.lineWidth = 3; ctx.lineCap = 'round';
      ctx.beginPath(); ctx.moveTo(18, 14); ctx.quadraticCurveTo(28, 16, 26, 24); ctx.stroke();
      // eye
      ctx.beginPath(); ctx.ellipse(15, 5, 6, 6.5, 0.2, 0, TAU); ctx.fillStyle = C.ink; ctx.fill();
      if (p.eye > 0) glow(ctx, 16, 4, 1.8 * p.eye, C.ember, 6);
      // helmet, which comes off when it dies
      ctx.save();
      ctx.translate(-4 - p.helmOff * 40, -11 - p.helmOff * 26 + p.helmOff * p.helmOff * 60); ctx.rotate(-p.helmOff * 2.2);
      crestedHelmet(ctx, p);
      ctx.restore();
      ctx.restore();

      // aspis on the near arm, across the front of the body
      ctx.save();
      ctx.translate(24 + p.shieldUp * 8, -66 - p.shieldUp * 10); ctx.rotate(p.shieldUp * 0.15);
      ctx.beginPath(); ctx.ellipse(0, 0, 15, 30, 0, 0, TAU);
      ctx.fillStyle = bronzeFill(ctx, -15, -30, 15, 30); ctx.fill(); outline(ctx, 1.8);
      ctx.beginPath(); ctx.ellipse(0, 0, 12, 26, 0, 0, TAU); ctx.strokeStyle = C.crimson; ctx.lineWidth = 2.6; ctx.stroke();
      // painted lambda
      ctx.strokeStyle = C.ink; ctx.lineWidth = 3; ctx.lineCap = 'round';
      ctx.beginPath(); ctx.moveTo(-5, 12); ctx.lineTo(1, -12); ctx.lineTo(6, 12); ctx.stroke();
      ctx.restore();
    }

    const draw = frame(FRAME, BASE, body);
    const K = (n, ks) => keys(BASE, n, ks);
    const ANIMS = {
      idle: { fps: 7, loop: true, frames: cycle(6, t => ({ bob: Math.sin(t) * 1.2, head: Math.sin(t) * 0.03, crest: Math.sin(t + 1) * 0.04, spear: Math.sin(t) * 0.02 })) },
      walk: { fps: 10, loop: true, frames: cycle(8, t => ({
        bob: -Math.abs(Math.sin(t)) * 3 + 1, legFront: Math.sin(t) * 0.55, legBack: -Math.sin(t) * 0.55,
        tilt: 0.04, crest: Math.sin(t * 2) * 0.05, spear: Math.sin(t * 2) * 0.03, shieldUp: 0.1 + Math.sin(t * 2) * 0.05 })) },
      run: { fps: 14, loop: true, frames: cycle(8, t => ({
        bob: -Math.abs(Math.sin(t)) * 5 + 2, legFront: Math.sin(t) * 0.8, legBack: -Math.sin(t) * 0.8,
        tilt: 0.14, crest: -0.12 + Math.sin(t * 2) * 0.05, spear: 0.08, thrust: 6, shieldUp: 0.3 })) },
      attackAntic: { fps: 10, loop: false, frames: K(4, [
        [0, {}],
        [1, { tilt: -0.14, thrust: -22, spear: -0.1, shieldUp: 0.7, legFront: 0.35, legBack: -0.35, crest: 0.12, squash: 0.25 }],
      ]) },
      attack: { fps: 16, loop: false, frames: K(5, [
        [0, { tilt: -0.14, thrust: -22, spear: -0.1, shieldUp: 0.7, legFront: 0.35, legBack: -0.35, squash: 0.25 }],
        [0.35, { tilt: 0.24, thrust: 40, spear: 0.05, dx: 10, shieldUp: 0.2, legFront: 0.75, legBack: -0.7, crest: -0.2 }],
        [1, { tilt: 0.18, thrust: 34, spear: 0.04, dx: 8, shieldUp: 0.2, legFront: 0.6, legBack: -0.6, crest: -0.15 }],
      ]) },
      hurt: { fps: 14, loop: false, frames: K(3, [
        [0, { flash: 0.85, tilt: -0.15, dx: -4, crest: 0.2, head: 0.1 }], [1, { flash: 0, tilt: -0.04, dx: -2 }]]) },
      death: { fps: 12, loop: false, frames: K(10, [
        [0, { flash: 0.8, tilt: -0.2, head: 0.2, eye: 0.6 }],
        [0.35, { rot: -0.7, dx: 8, bob: -6, helmOff: 0.5, legFront: -0.6, legBack: 0.4, eye: 0.3, spear: -0.4 }],
        [0.7, { rot: -1.5, dx: 50, bob: -30, helmOff: 1, legFront: -0.9, legBack: 0.6, eye: 0, spear: -0.6, shieldUp: -0.3 }],
        [1, { rot: -1.5, dx: 50, bob: -30, helmOff: 1, legFront: -1.1, legBack: 0.9, eye: 0, spear: -0.6, shieldUp: -0.3 }],
      ]) },
    };
    return { name: 'Hoplite Husk', key: 'hopliteHusk', draw, FRAME, ANIMS, scale: 2, figure: 190, anchor: 'feet' };
  })();

  // ---------------------------------------------------------------------------------------
  // Harpy Fly: a plum-bodied fly with a hooked bronze beak, feather mane and talons.
  // ---------------------------------------------------------------------------------------
  const harpy = (() => {
    const FRAME = { w: 220, h: 200, originX: 104, originY: 104 };
    const BASE = { bob: 0, tilt: 0, flap: 0, mane: 0, talon: 0, beak: 0, squash: 0, rot: 0, dx: 0, alpha: 1, flash: 0, eye: 1, limp: 0 };

    function wing(ctx, angle, dark) {
      ctx.save(); ctx.rotate(angle);
      const feathers = [[0, 62, 13], [0.28, 58, 12], [0.55, 50, 11], [0.82, 40, 10], [1.08, 30, 9]];
      for (const [a, len, w] of feathers) {
        ctx.save(); ctx.rotate(-1.1 - a);
        ctx.beginPath(); ctx.ellipse(len / 2, 0, len / 2, w / 2, 0, 0, TAU);
        ctx.fillStyle = grad(ctx, 0, 0, len, 0, dark ? C.plumDark : C.feather, dark ? C.feather : C.featherTip);
        ctx.fill(); outline(ctx, 1.2);
        ctx.beginPath(); ctx.moveTo(len * 0.82, 0); ctx.lineTo(len, 0); ctx.strokeStyle = C.bronzeLight; ctx.lineWidth = 2.2; ctx.stroke();
        ctx.restore();
      }
      ctx.restore();
    }

    function body(ctx, p) {
      ctx.rotate(p.tilt);
      // back wing
      ctx.save(); ctx.translate(-4, -12); wing(ctx, -0.2 + p.flap * 0.9 + p.limp, true); ctx.restore();
      // tail feathers
      for (const [a, l] of [[0.35, 30], [0.6, 34], [0.85, 28]]) {
        ctx.save(); ctx.translate(-26, 12); ctx.rotate(Math.PI - a + p.limp * 0.3);
        ctx.beginPath(); ctx.ellipse(l / 2, 0, l / 2, 4.5, 0, 0, TAU); ctx.fillStyle = C.feather; ctx.fill(); outline(ctx, 1);
        ctx.restore();
      }
      // talons
      for (const [x, s] of [[-2, 0.8], [8, 1]]) {
        ctx.save(); ctx.translate(x, 14); ctx.rotate(-0.2 - p.talon * 0.8);
        limb(ctx, [[0, 0], [2, 12], [6 + p.talon * 6, 20]], 2.6 * s, C.bronzeDark);
        ctx.strokeStyle = C.ink; ctx.lineWidth = 1.6;
        for (const a of [-0.6, 0, 0.6]) { ctx.beginPath(); ctx.moveTo(6 + p.talon * 6, 20); ctx.lineTo(6 + p.talon * 6 + Math.cos(a) * 6, 20 + Math.sin(a) * 4 + 3); ctx.stroke(); }
        ctx.restore();
      }
      // abdomen with bronze bands, then thorax
      ctx.save(); ctx.translate(-18, 8); ctx.rotate(0.45);
      ctx.beginPath(); ctx.ellipse(0, 0, 17, 12, 0, 0, TAU); ctx.fillStyle = grad(ctx, 0, -12, 0, 12, C.plumLight, C.plumDark); ctx.fill(); outline(ctx, 1.6);
      ctx.strokeStyle = C.bronze; ctx.lineWidth = 2;
      for (const x of [-7, 0, 7]) { ctx.beginPath(); ctx.ellipse(x, 0, 2.5, 11, 0, -1.3, 1.3); ctx.stroke(); }
      ctx.restore();
      ctx.beginPath(); ctx.ellipse(2, 0, 21, 18, 0, 0, TAU); ctx.fillStyle = grad(ctx, 0, -18, 0, 18, C.plumLight, C.plumDark); ctx.fill(); outline(ctx, 1.8);
      // mane of feathers streaming back
      for (let i = 0; i < 5; i++) {
        ctx.save(); ctx.translate(16, -22); ctx.rotate(Math.PI + 0.25 - i * 0.22 + p.mane * (1 + i * 0.3));
        const l = 26 + i * 3;
        ctx.beginPath(); ctx.ellipse(l / 2, 0, l / 2, 5, 0, 0, TAU);
        ctx.fillStyle = grad(ctx, 0, 0, l, 0, C.feather, C.featherTip); ctx.fill(); outline(ctx, 1.1);
        ctx.restore();
      }
      // head: pale mask, scowling eye, hooked beak
      ctx.save(); ctx.translate(22, -14);
      disc(ctx, 0, 0, 14, C.pale, 1.5);
      ctx.beginPath(); ctx.moveTo(10, -2); ctx.bezierCurveTo(22, -6, 30, 0, 28, 12 + p.beak * 3);
      ctx.bezierCurveTo(24, 6, 18, 6, 10, 7); ctx.closePath();
      ctx.fillStyle = bronzeFill(ctx, 10, -4, 28, 12); ctx.fill(); outline(ctx, 1.3);
      ctx.beginPath(); ctx.moveTo(-1, -9); ctx.lineTo(12, -4); ctx.lineTo(10, 2); ctx.quadraticCurveTo(2, 2, -1, -3); ctx.closePath();
      ctx.fillStyle = C.ink; ctx.fill();
      if (p.eye > 0) glow(ctx, 7, -3, 1.8 * p.eye, C.ember, 6);
      ctx.restore();
      // front wing
      ctx.save(); ctx.translate(0, -14); wing(ctx, p.flap + p.limp, false); ctx.restore();
    }

    const draw = frame(FRAME, BASE, body);
    const K = (n, ks) => keys(BASE, n, ks);
    const ANIMS = {
      fly: { fps: 14, loop: true, frames: cycle(6, t => ({
        bob: Math.sin(t) * 4, flap: Math.sin(t) * 0.75, mane: Math.sin(t - 1) * 0.08, talon: 0.1 + Math.sin(t) * 0.05, tilt: 0.04 })) },
      attack: { fps: 16, loop: true, frames: cycle(4, t => ({
        tilt: 0.45, flap: -0.7 + Math.sin(t) * 0.25, mane: 0.25, talon: 1, beak: 1, bob: Math.sin(t) * 1.5 })) },
      hurt: { fps: 14, loop: false, frames: K(3, [[0, { flash: 0.85, tilt: -0.3, dx: -5, flap: 0.5, mane: -0.2 }], [1, { tilt: -0.05, dx: -2 }]]) },
      death: { fps: 12, loop: false, frames: K(9, [
        [0, { flash: 0.8, tilt: -0.3, flap: 0.6, eye: 0.6 }],
        [0.5, { rot: 1.6, bob: 30, flap: -0.3, limp: 0.6, eye: 0.2, talon: 0.5 }],
        [1, { rot: 2.8, bob: 60, dx: 4, flap: -0.5, limp: 1.1, eye: 0, talon: 0.6, mane: 0.3 }],
      ]) },
    };
    return { name: 'Harpy Fly', key: 'harpyFly', draw, FRAME, ANIMS, scale: 2, figure: 120, anchor: 'center' };
  })();

  // ---------------------------------------------------------------------------------------
  // Bronze Crawler: a tiny wind-up automaton beetle with a turning key and piston legs.
  // ---------------------------------------------------------------------------------------
  const crawler = (() => {
    const FRAME = { w: 200, h: 150, originX: 96, originY: 128 };
    const BASE = { bob: 0, step: 0, key: 0, squash: 0, rot: 0, dx: 0, alpha: 1, flash: 0, eye: 1, burst: 0, tilt: 0 };

    function body(ctx, p) {
      ctx.rotate(p.tilt);
      const b = p.burst;
      // far legs
      for (let i = 0; i < 3; i++) {
        const ph = p.step + i * 2.1, x = -22 + i * 20;
        ctx.save(); ctx.translate(x, -14);
        if (b > 0) ctx.translate(-20 + i * 14 * b, -b * 10);
        limb(ctx, [[0, 0], [6 + Math.cos(ph) * 4, 6 - Math.max(0, Math.sin(ph)) * 4], [2 + Math.cos(ph) * 7, 14 - Math.max(0, Math.sin(ph)) * 4]], 2.4, C.bronzeDark);
        ctx.restore();
      }
      // spring and gears that burst out on death
      if (b > 0) {
        const gears = [[-46, -30, 7, 1], [-20, -58, 6, -1], [30, -52, 8, 1], [52, -18, 5, -1], [6, -70, 4, 1]];
        gears.forEach(([gx, gy, r, s], i) => {
          const fall = Math.max(0, b - 0.5) * 2;
          toothedGear(ctx, gx * b, gy * b * (1 - fall) + (-r) * fall, r, s * b * 6 + i);
        });
        ctx.strokeStyle = C.bronzeLight; ctx.lineWidth = 1.6; ctx.beginPath();
        for (let i = 0; i <= 30; i++) { const a = i / 30 * TAU * 3; ctx.lineTo(Math.cos(a) * 5 + i * 0.6 * b, -20 - i * 1.1 * b * (1.2 - b) + Math.sin(a) * 5); }
        ctx.stroke();
      }
      // shell, which pops up and flips onto its back on death
      ctx.save();
      ctx.translate(-b * 10, -b * (1 - b) * 120 + b * 10);
      ctx.translate(0, -30); ctx.rotate(b * Math.PI); ctx.translate(0, 30);
      ctx.beginPath(); ctx.moveTo(-42, -12); ctx.bezierCurveTo(-42, -50, 36, -56, 42, -14); ctx.closePath();
      ctx.fillStyle = bronzeFill(ctx, -30, -50, 30, -12); ctx.fill(); outline(ctx, 1.8);
      ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 1.3;
      for (const x of [-18, 2, 22]) { ctx.beginPath(); ctx.moveTo(x, -13); ctx.quadraticCurveTo(x + 2, -36, x - 4, -46); ctx.stroke(); }
      for (const [x, y] of [[-30, -18], [-10, -18], [10, -18], [30, -18], [-24, -36], [16, -40]]) disc(ctx, x, y, 1.6, C.bronzeLight);
      ctx.fillStyle = C.wood; ctx.fillRect(-42, -14, 84, 5); ctx.strokeStyle = C.ink; ctx.lineWidth = 1; ctx.strokeRect(-42, -14, 84, 5);
      // wind-up key
      if (b < 0.3) {
      ctx.save(); ctx.translate(-6, -46);
      ctx.fillStyle = C.bronzeDark; ctx.fillRect(-2, -12, 4, 12);
      ctx.scale(Math.cos(p.key), 1);
      for (const s of [-1, 1]) { ctx.beginPath(); ctx.ellipse(s * 9, -16, 8, 6, 0, 0, TAU); ctx.fillStyle = bronzeFill(ctx, -16, -22, 16, -10); ctx.fill(); outline(ctx, 1.3); disc(ctx, s * 9, -16, 2.4, C.wood); }
      ctx.restore();
      }
      ctx.restore();
      if (b > 0) return;
      // head with a glowing lens and pincers
      ctx.beginPath(); ctx.moveTo(38, -12); ctx.lineTo(38, -30); ctx.quadraticCurveTo(54, -30, 56, -16); ctx.lineTo(54, -10); ctx.closePath();
      ctx.fillStyle = C.bronzeDark; ctx.fill(); outline(ctx, 1.5);
      disc(ctx, 47, -21, 6, C.ink, 1);
      if (p.eye > 0) glow(ctx, 48, -21, 3.2 * p.eye, C.ember, 8);
      ctx.strokeStyle = C.ink; ctx.lineWidth = 2.4; ctx.lineCap = 'round';
      ctx.beginPath(); ctx.moveTo(54, -12); ctx.quadraticCurveTo(64, -10, 60, -4); ctx.stroke();
      // near legs, piston-style
      for (let i = 0; i < 3; i++) {
        const ph = p.step + i * 2.1 + Math.PI, x = -26 + i * 22;
        limb(ctx, [[x, -10], [x + 7 + Math.cos(ph) * 5, -4 - Math.max(0, Math.sin(ph)) * 5], [x + 3 + Math.cos(ph) * 8, 0 - Math.max(0, Math.sin(ph)) * 5]], 3, C.bronze);
        disc(ctx, x, -10, 2.4, C.bronzeLight, 0.8);
      }
    }

    const draw = frame(FRAME, BASE, body);
    const K = (n, ks) => keys(BASE, n, ks);
    const ANIMS = {
      walk: { fps: 12, loop: true, frames: cycle(8, t => ({ step: t, key: t, bob: Math.sin(t * 3) * 0.8 })) },
      hurt: { fps: 14, loop: false, frames: K(3, [[0, { flash: 0.85, squash: 0.4, dx: -4 }], [1, { dx: -1 }]]) },
      death: { fps: 14, loop: false, frames: K(10, [
        [0, { flash: 0.8, squash: 0.5, eye: 0.5 }],
        [0.15, { burst: 0.15, eye: 0, key: 2 }],
        [1, { burst: 1, eye: 0, key: 6 }],
      ]) },
    };
    return { name: 'Bronze Crawler', key: 'bronzeCrawler', draw, FRAME, ANIMS, scale: 2, figure: 74, anchor: 'feet' };
  })();

  // ---------------------------------------------------------------------------------------
  // The Minotaur: a maggot inside a hulking bronze-plated shell, wearing a bull skull, with a labrys.
  // ---------------------------------------------------------------------------------------
  const minotaur = (() => {
    const FRAME = { w: 460, h: 440, originX: 210, originY: 410 };
    const BASE = { bob: 0, tilt: 0, legBack: 0, legFront: 0, axe: 0, axeLift: 0, head: 0, skullOff: 0, skullDrop: 0, maggot: 0,
      jaw: 0, squash: 0, rot: 0, dx: 0, alpha: 1, flash: 0, eye: 1, kneel: 0 };

    function labrys(ctx, p) {
      // haft
      ctx.strokeStyle = C.wood; ctx.lineWidth = 8; ctx.lineCap = 'round';
      ctx.beginPath(); ctx.moveTo(0, 50); ctx.lineTo(0, -170); ctx.stroke();
      ctx.lineWidth = 1.5; ctx.strokeStyle = C.ink; ctx.beginPath(); ctx.moveTo(4, 50); ctx.lineTo(4, -170); ctx.stroke();
      ctx.fillStyle = C.bronzeDark; for (const y of [20, -40]) ctx.fillRect(-5, y, 10, 6);
      // double crescent head
      ctx.save(); ctx.translate(0, -150);
      for (const s of [-1, 1]) {
        ctx.beginPath(); ctx.moveTo(s * 6, -16);
        ctx.bezierCurveTo(s * 40, -30, s * 70, -46, s * 72, -50);
        ctx.bezierCurveTo(s * 58, -10, s * 58, 10, s * 72, 50);
        ctx.bezierCurveTo(s * 70, 46, s * 40, 30, s * 6, 16); ctx.closePath();
        ctx.fillStyle = bronzeFill(ctx, s * 6, -50, s * 72, 50); ctx.fill(); outline(ctx, 2.2);
        ctx.beginPath(); ctx.moveTo(s * 70, -44); ctx.bezierCurveTo(s * 58, -10, s * 58, 10, s * 70, 44);
        ctx.strokeStyle = C.bronzeLight; ctx.lineWidth = 2; ctx.stroke();
        ctx.beginPath(); ctx.arc(s * 34, 0, 7, 0, TAU); ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 2; ctx.stroke();
      }
      ctx.beginPath(); ctx.rect(-8, -20, 16, 40); ctx.fillStyle = C.bronzeDark; ctx.fill(); outline(ctx, 1.5);
      ctx.restore();
    }

    function skull(ctx, p) {
      // horns: a bull's sweep, out to the side and then forward and up
      for (const [ox, oy, dark] of [[-10, -4, true], [0, 0, false]]) {
        ctx.save(); ctx.translate(ox, oy);
        ctx.beginPath(); ctx.moveTo(-22, -34);
        ctx.bezierCurveTo(-70, -50, -66, -96, -10, -112);
        ctx.bezierCurveTo(-40, -92, -40, -62, 2, -44); ctx.closePath();
        ctx.fillStyle = grad(ctx, -20, -40, -10, -112, dark ? C.boneShade : C.bone, '#2a2420'); ctx.fill(); outline(ctx, 1.8);
        ctx.strokeStyle = dark ? '#8f8470' : C.boneShade; ctx.lineWidth = 1.2;
        for (const [x, y] of [[-40, -48], [-50, -64], [-50, -80]]) { ctx.beginPath(); ctx.moveTo(x, y); ctx.lineTo(x + 12, y - 4); ctx.stroke(); }
        ctx.restore();
      }
      // broad cranium and a short heavy snout pointing forward-down
      ctx.beginPath();
      ctx.moveTo(-34, -20); ctx.bezierCurveTo(-32, -52, 22, -54, 32, -28);
      ctx.bezierCurveTo(44, -14, 56, 4, 58, 22); ctx.bezierCurveTo(58, 36, 44, 42, 32, 36 + p.jaw * 6);
      ctx.bezierCurveTo(16, 32, 0, 26, -12, 22); ctx.bezierCurveTo(-32, 16, -38, 0, -34, -20); ctx.closePath();
      ctx.fillStyle = grad(ctx, 0, -50, 30, 40, C.bone, C.boneShade); ctx.fill(); outline(ctx, 2);
      // cracks, nostrils, eye socket
      ctx.strokeStyle = C.boneShade; ctx.lineWidth = 1.4;
      ctx.beginPath(); ctx.moveTo(-6, -44); ctx.lineTo(0, -32); ctx.lineTo(-6, -22); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(30, -2); ctx.bezierCurveTo(40, 6, 46, 12, 52, 22); ctx.stroke();
      for (const [x, y] of [[52, 28], [44, 32]]) { ctx.beginPath(); ctx.ellipse(x, y, 3.5, 2.4, 0.4, 0, TAU); ctx.fillStyle = C.ink; ctx.fill(); }
      ctx.beginPath(); ctx.ellipse(14, -12, 12, 10, 0.35, 0, TAU); ctx.fillStyle = C.ink; ctx.fill();
      if (p.eye > 0) glow(ctx, 16, -11, 4 * p.eye, C.ember, 14);
      // bronze ring through the snout
      ctx.beginPath(); ctx.arc(48, 38, 7, -0.4, Math.PI * 1.1); ctx.strokeStyle = C.bronzeLight; ctx.lineWidth = 3; ctx.stroke();
    }

    function maggot(ctx, p) {
      // the pale grub inside, seen when the skull lifts
      ctx.save(); ctx.translate(0, 10 - p.maggot * 40);
      ctx.beginPath(); ctx.ellipse(0, 0, 22, 26, 0.2, 0, TAU); ctx.fillStyle = grad(ctx, 0, -26, 0, 26, C.grub, C.grubShade); ctx.fill(); outline(ctx, 1.8);
      ctx.strokeStyle = C.grubShade; ctx.lineWidth = 1.4;
      for (const y of [6, 14]) { ctx.beginPath(); ctx.ellipse(0, y, 20, 5, 0.2, 0.2, Math.PI - 0.2); ctx.stroke(); }
      disc(ctx, 6, -8, 5, C.ink); disc(ctx, 16, -6, 4, C.ink);
      disc(ctx, 7, -9, 1.4, C.pale); disc(ctx, 17, -7, 1.2, C.pale);
      ctx.beginPath(); ctx.arc(13, 4, 4, 0.2, Math.PI - 0.2); ctx.strokeStyle = C.ink; ctx.lineWidth = 1.6; ctx.stroke();
      ctx.restore();
    }

    function leg(ctx, x, angle, dark) {
      ctx.save(); ctx.translate(x, -64 + 0); ctx.rotate(angle);
      ctx.beginPath(); ctx.roundRect(-18, -10, 36, 48, 12); ctx.fillStyle = dark ? C.hideDark : C.hide; ctx.fill(); outline(ctx, 2);
      ctx.beginPath(); ctx.roundRect(-16, 26, 32, 30, 6); ctx.fillStyle = bronzeFill(ctx, -16, 26, 16, 56); ctx.fill(); outline(ctx, 1.8);
      ctx.beginPath(); ctx.moveTo(-18, 56); ctx.lineTo(20, 56); ctx.lineTo(24, 66); ctx.lineTo(-20, 66); ctx.closePath();
      ctx.fillStyle = C.ink; ctx.fill();
      ctx.restore();
    }

    function body(ctx, p) {
      ctx.translate(0, p.kneel * 30);
      ctx.rotate(p.tilt);
      leg(ctx, -40, p.legBack - p.kneel * 0.4, true);
      // far arm holding the labrys
      ctx.save();
      ctx.translate(-10, -200 - p.axeLift * 30);
      ctx.rotate(-0.9 + p.axe);
      labrys(ctx, p);
      limb(ctx, [[0, 20], [0, 0]], 14, C.hideDark);
      disc(ctx, 0, 6, 11, C.hideDark, 1.6);
      ctx.restore();
      // hulking body
      ctx.beginPath(); ctx.ellipse(0, -160, 96, 100, 0.08, 0, TAU);
      ctx.fillStyle = grad(ctx, -40, -260, 40, -60, '#5a4232', C.hideDark); ctx.fill(); outline(ctx, 2.4);
      // bronze cuirass plate with embossed lines and rivets
      ctx.beginPath(); ctx.moveTo(-30, -240); ctx.bezierCurveTo(60, -260, 104, -190, 92, -120);
      ctx.bezierCurveTo(70, -86, 10, -80, -24, -96); ctx.bezierCurveTo(-46, -150, -48, -210, -30, -240); ctx.closePath();
      ctx.fillStyle = bronzeFill(ctx, -30, -250, 90, -90); ctx.fill(); outline(ctx, 2);
      ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 2;
      ctx.beginPath(); ctx.moveTo(4, -220); ctx.bezierCurveTo(50, -214, 70, -180, 66, -150); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(-6, -170); ctx.bezierCurveTo(30, -164, 56, -140, 56, -116); ctx.stroke();
      for (const [x, y] of [[-20, -226], [-30, -180], [-22, -120], [80, -130], [84, -180]]) disc(ctx, x, y, 3, C.bronzeLight, 0.8);
      // meander belt and leather pteruges
      ctx.save(); ctx.translate(0, -78); ctx.rotate(0.05);
      for (let i = 0; i < 9; i++) {
        const x = -76 + i * 18, h = 30 - Math.abs(i - 4) * 2;
        ctx.beginPath(); ctx.rect(x, 0, 16, h); ctx.fillStyle = i % 2 ? C.crimsonDark : C.crimson; ctx.fill(); outline(ctx, 1.2);
        disc(ctx, x + 8, h - 5, 2.4, C.bronzeLight);
      }
      ctx.fillStyle = C.bronzeDark; ctx.fillRect(-80, -10, 166, 12); ctx.strokeStyle = C.ink; ctx.lineWidth = 1.4; ctx.strokeRect(-80, -10, 166, 12);
      ctx.strokeStyle = C.bronzeLight; ctx.lineWidth = 1.2;
      for (let x = -76; x < 80; x += 14) { ctx.beginPath(); ctx.moveTo(x, 0); ctx.lineTo(x, -7); ctx.lineTo(x + 8, -7); ctx.lineTo(x + 8, -3); ctx.lineTo(x + 4, -3); ctx.stroke(); }
      ctx.restore();
      leg(ctx, 40, p.legFront + p.kneel * 0.5, false);
      // neck hole, maggot and the bull skull riding on top
      ctx.save(); ctx.translate(58, -246); ctx.rotate(p.head);
      ctx.beginPath(); ctx.ellipse(0, 18, 34, 14, 0.2, 0, TAU); ctx.fillStyle = C.hideDark; ctx.fill(); outline(ctx, 1.8);
      maggot(ctx, p);
      ctx.save(); ctx.translate(-p.skullOff * 46 + p.skullDrop * 80, -p.skullOff * 70 + p.skullDrop * 190); ctx.rotate(-p.skullOff * 0.7 + p.skullDrop * 1.9);
      skull(ctx, p);
      ctx.restore();
      ctx.restore();
      // pauldron on the near shoulder
      ctx.beginPath(); ctx.ellipse(-6, -214, 40, 26, -0.3, 0, TAU); ctx.fillStyle = bronzeFill(ctx, -40, -240, 30, -190); ctx.fill(); outline(ctx, 2);
      ctx.beginPath(); ctx.ellipse(-6, -214, 30, 18, -0.3, 0, TAU); ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 2; ctx.stroke();
    }

    const draw = frame(FRAME, BASE, body);
    const K = (n, ks) => keys(BASE, n, ks);
    const raised = { axe: -0.8, axeLift: 1.4, tilt: -0.12, head: -0.08, legFront: 0.15, legBack: -0.15, squash: 0.15 };
    const slam = { axe: 2.9, axeLift: -1.8, tilt: 0.16, head: 0.1, legFront: 0.35, legBack: -0.3, dx: 12, squash: -0.05 };
    const ANIMS = {
      idle: { fps: 6, loop: true, frames: cycle(6, t => ({ bob: Math.sin(t) * 2.5, squash: Math.sin(t) * 0.05, head: Math.sin(t) * 0.03, axe: Math.sin(t) * 0.03, eye: 0.8 + Math.sin(t) * 0.2 })) },
      run: { fps: 12, loop: true, frames: cycle(8, t => ({
        bob: -Math.abs(Math.sin(t)) * 8 + 3, legFront: Math.sin(t) * 0.5, legBack: -Math.sin(t) * 0.5,
        tilt: 0.12, head: 0.06, axe: 0.15 + Math.sin(t * 2) * 0.05 })) },
      jumpAntic: { fps: 12, loop: false, frames: K(3, [[0, {}], [1, { squash: 0.7, bob: 8, legFront: 0.4, legBack: -0.4, axe: -0.2, head: 0.1 }]]) },
      jump: { fps: 10, loop: false, frames: K(3, [[0, { squash: -0.4, legFront: -0.3, legBack: 0.4, axe: -0.4, axeLift: 0.5 }], [1, { squash: -0.15, legFront: -0.2, legBack: 0.3, axe: -0.3, axeLift: 0.4 }]]) },
      land: { fps: 14, loop: false, frames: K(3, [[0, { squash: 0.8, bob: 6, legFront: 0.4, legBack: -0.4, axe: 0.3 }], [1, {}]]) },
      attackAntic: { fps: 10, loop: false, frames: K(5, [[0, {}], [1, raised]]) },
      attack: { fps: 18, loop: false, frames: K(5, [[0, raised], [0.4, slam], [1, Object.assign({}, slam, { axe: 2.85 })]]) },
      attackRecover: { fps: 10, loop: false, frames: K(4, [[0, slam], [1, {}]]) },
      roar: { fps: 12, loop: true, frames: cycle(4, t => ({ head: -0.22 + Math.sin(t * 2) * 0.03, jaw: 1, tilt: -0.06, axe: -0.3, axeLift: 0.4, bob: Math.sin(t * 2) * 1.5, eye: 1.4 })) },
      stun: { fps: 8, loop: true, frames: cycle(6, t => ({
        kneel: 1, tilt: 0.18, head: 0.15, skullOff: 0.9 + Math.sin(t) * 0.05, maggot: 0.8 + Math.sin(t) * 0.12, eye: 0, axe: 2.7, axeLift: -1.8,
        legFront: 0.2, legBack: -0.2, squash: 0.1 })) },
      hurt: { fps: 14, loop: false, frames: K(3, [[0, { flash: 0.8, tilt: -0.08, dx: -6 }], [1, { dx: -2 }]]) },
      death: { fps: 10, loop: false, frames: K(12, [
        [0, { flash: 0.8, head: -0.2, jaw: 1, eye: 1.3 }],
        [0.3, { kneel: 0.6, tilt: 0.1, head: 0.2, skullOff: 0.7, maggot: 0.5, eye: 0.6, axe: 1.2, axeLift: -1 }],
        [0.7, { kneel: 1, tilt: 0.3, head: 0.4, skullOff: 0.4, skullDrop: 0.8, maggot: 0.9, eye: 0, axe: 2.6, axeLift: -1.8, squash: 0.2 }],
        [1, { kneel: 1, tilt: 0.34, head: 0.5, skullOff: 0.2, skullDrop: 1, maggot: 0.7, eye: 0, axe: 2.9, axeLift: -1.9, squash: 0.25 }],
      ]) },
    };
    return { name: 'The Minotaur', key: 'minotaur', draw, FRAME, ANIMS, scale: 1.5, figure: 380, anchor: 'feet' };
  })();

  root.Bestiary = { creatures: [hoplite, harpy, crawler, minotaur], COLORS: C };
})(typeof window !== 'undefined' ? window : globalThis);
