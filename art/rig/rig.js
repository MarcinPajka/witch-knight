// Hex Moth side-view rig. Draws the heroine facing right from a pose object,
// so the same code drives the preview page and the sprite-sheet export.
(function (root) {
  const C = {
    pale: '#eeeaf6', paleShade: '#b9add6', ink: '#0c0914', body: '#241634',
    cloakTop: '#57357a', cloakBottom: '#170d24', cloakLining: '#2e1b45', moon: '#c8e07a', moonCore: '#f4ffd0',
    silverLight: '#f4f3fa', silver: '#b8b6c8', silverDark: '#5d5a72',
    bronzeLight: '#e0b56a', bronze: '#b07a3a', bronzeDark: '#6e4520', wood: '#3a2c20',
  };

  // Frame size in pixels; the origin sits between the feet.
  const FRAME = { w: 320, h: 320, originX: 150, originY: 230 };

  const BASE = {
    bob: 0, tilt: 0, legBack: 0, legFront: 0, wingBack: 0, wingFront: 0,
    wingOpen: 0, staff: 0, staffReach: 0, staffLift: 0, antenna: 0, head: 0, orb: 1, squash: 0,
    rot: 0, dx: 0, alpha: 1, shieldUp: 0, shieldBack: 0, flash: 0, hex: 0, spin: 0,
  };

  function bronzeFill(ctx, x0, y0, x1, y1) {
    const g = ctx.createLinearGradient(x0, y0, x1, y1);
    g.addColorStop(0, C.bronzeLight); g.addColorStop(0.55, C.bronze); g.addColorStop(1, C.bronzeDark);
    return g;
  }

  function silverFill(ctx, x0, y0, x1, y1) {
    const g = ctx.createLinearGradient(x0, y0, x1, y1);
    g.addColorStop(0, C.silverLight); g.addColorStop(0.5, C.silver); g.addColorStop(1, C.silverDark);
    return g;
  }

  // Witch's mantle hanging from the shoulders behind her, with a ragged hem that trails
  // as she moves (sway follows the back wing).
  function mantle(ctx, p) {
    const sway = p.wingBack * 18 - p.wingOpen * 6, lift = p.wingOpen * 10;
    ctx.save();
    ctx.translate(-4, -94);
    ctx.beginPath();
    ctx.moveTo(14, 0);
    ctx.bezierCurveTo(-6, 4, -26 + sway * 0.3, 30, -34 + sway, 74 - lift);
    // ragged hem, left to right
    const hem = [[-34, 74], [-26, 66], [-20, 78], [-12, 68], [-6, 80], [2, 70], [6, 76]];
    for (const [hx, hy] of hem) ctx.lineTo(hx + sway * (1 - (hx + 34) / 50), hy - lift);
    ctx.bezierCurveTo(8, 50, 12, 20, 14, 0);
    ctx.closePath();
    const g = ctx.createLinearGradient(0, 0, 0, 80);
    g.addColorStop(0, C.cloakTop); g.addColorStop(1, C.cloakBottom);
    ctx.fillStyle = g; ctx.fill(); outline(ctx, 1.6);
    // inner lining fold
    ctx.beginPath(); ctx.moveTo(8, 6); ctx.bezierCurveTo(-4, 20, -12 + sway * 0.4, 44, -14 + sway * 0.6, 66 - lift);
    ctx.strokeStyle = C.cloakLining; ctx.lineWidth = 2; ctx.stroke();
    // silver trim along the hem
    ctx.beginPath(); ctx.moveTo(-34 + sway, 74 - lift);
    for (const [hx, hy] of hem.slice(1)) ctx.lineTo(hx + sway * (1 - (hx + 34) / 50), hy - lift);
    ctx.strokeStyle = C.silver; ctx.lineWidth = 1.2; ctx.globalAlpha = 0.8; ctx.stroke(); ctx.globalAlpha = 1;
    ctx.restore();
  }

  // Silver crescent diadem worn at the helmet's brow, horns up, with a moonstone.
  function crown(ctx, p) {
    ctx.save();
    ctx.translate(8, -26);
    ctx.rotate(-0.05);
    ctx.scale(0.62, 0.55);
    ctx.beginPath();
    ctx.moveTo(-22, 4);
    ctx.bezierCurveTo(-30, -8, -24, -26, -14, -32);
    ctx.bezierCurveTo(-18, -20, -14, -8, 0, -6);
    ctx.bezierCurveTo(14, -8, 18, -20, 14, -32);
    ctx.bezierCurveTo(24, -26, 30, -8, 22, 4);
    ctx.bezierCurveTo(10, 0, -10, 0, -22, 4);
    ctx.closePath();
    ctx.fillStyle = silverFill(ctx, -20, -32, 20, 4); ctx.fill(); outline(ctx, 1.4);
    ctx.save(); ctx.shadowColor = C.moon; ctx.shadowBlur = 6 + 4 * p.orb;
    ctx.beginPath(); ctx.ellipse(0, -2, 3.2, 4.2, 0, 0, Math.PI * 2); ctx.fillStyle = C.moon; ctx.fill();
    ctx.restore();
    ctx.beginPath(); ctx.arc(-0.8, -3.4, 1.1, 0, Math.PI * 2); ctx.fillStyle = C.moonCore; ctx.fill();
    ctx.restore();
  }

  function outline(ctx, w) { ctx.lineWidth = w || 2; ctx.strokeStyle = C.ink; ctx.lineJoin = 'round'; ctx.stroke(); }

  function wing(ctx, angle, open, shade) {
    ctx.save();
    ctx.rotate(angle);
    const len = 70 + open * 18, spread = 26 + open * 30;
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.bezierCurveTo(-spread, 8, -spread - 8, len * 0.7, -spread * 0.6, len);
    ctx.bezierCurveTo(-spread * 0.3, len * 0.85, -6, len * 0.8, 6, len * 0.62);
    ctx.closePath();
    const g = ctx.createLinearGradient(0, 0, 0, len);
    g.addColorStop(0, C.cloakTop); g.addColorStop(1, C.cloakBottom);
    ctx.fillStyle = g; ctx.globalAlpha = shade; ctx.fill(); ctx.globalAlpha = 1; outline(ctx, 1.5);
    // eyespot
    ctx.beginPath(); ctx.arc(-spread * 0.55, len * 0.62, 7 + open * 2, 0, Math.PI * 2);
    ctx.fillStyle = C.moon; ctx.globalAlpha = 0.85 * shade + 0.1; ctx.fill(); ctx.globalAlpha = 1;
    ctx.beginPath(); ctx.arc(-spread * 0.55, len * 0.62, 3, 0, Math.PI * 2); ctx.fillStyle = C.ink; ctx.fill();
    ctx.restore();
  }

  // Insect leg: thigh angled forward, a knee joint, a thin shin angled back and a hooked foot claw.
  function leg(ctx, x, angle, squash) {
    const k = 1 - squash * 0.25;
    ctx.save();
    ctx.translate(x, -38 + squash * 6);
    ctx.rotate(angle);
    ctx.lineCap = 'round'; ctx.lineJoin = 'round';
    const knee = [6, 15 * k], ankle = [-1, 31 * k], toe = [9, 36 * k];
    // outline pass, then fill pass, so segments read as one limb
    for (const [w, col] of [[7.5, C.ink], [5, C.body]]) {
      ctx.strokeStyle = col;
      ctx.lineWidth = w; ctx.beginPath(); ctx.moveTo(0, 0); ctx.lineTo(...knee); ctx.stroke();
      ctx.lineWidth = w - 1.5; ctx.beginPath(); ctx.moveTo(...knee); ctx.lineTo(...ankle); ctx.stroke();
      ctx.lineWidth = w - 2.5; ctx.beginPath(); ctx.moveTo(...ankle); ctx.lineTo(...toe); ctx.stroke();
    }
    // claw tips
    ctx.strokeStyle = C.ink; ctx.lineWidth = 1.6;
    ctx.beginPath(); ctx.moveTo(toe[0], toe[1]); ctx.lineTo(toe[0] + 3, toe[1] + 2); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(ankle[0], ankle[1]); ctx.lineTo(ankle[0] - 4, ankle[1] + 4); ctx.stroke();
    // bronze knee guard
    ctx.beginPath(); ctx.arc(knee[0], knee[1], 3.6, 0, Math.PI * 2);
    ctx.fillStyle = silverFill(ctx, knee[0] - 3, knee[1] - 3, knee[0] + 3, knee[1] + 3); ctx.fill(); outline(ctx, 1);
    ctx.restore();
  }

  function antenna(ctx, sway, mirror) {
    ctx.save();
    ctx.rotate(sway * mirror);
    ctx.strokeStyle = C.pale; ctx.lineWidth = 2.4; ctx.lineCap = 'round';
    const pts = [[0, 0], [-10, -18], [-26, -30], [-40, -34]];
    ctx.beginPath(); ctx.moveTo(0, 0);
    ctx.bezierCurveTo(-10, -18, -26, -30, -40, -34); ctx.stroke();
    ctx.lineWidth = 1.3;
    for (let i = 1; i < pts.length; i++) {
      const [px, py] = pts[i];
      ctx.beginPath(); ctx.moveTo(px, py); ctx.lineTo(px - 2, py - 9); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(px, py); ctx.lineTo(px - 9, py + 1); ctx.stroke();
    }
    ctx.restore();
  }

  function head(ctx, p) {
    ctx.save();
    ctx.translate(6, -120);
    ctx.rotate(p.head);
    // mask, mostly hidden by the helmet; shows through the eye slot and face gap
    ctx.beginPath(); ctx.ellipse(4, 2, 21, 22, 0, 0, Math.PI * 2);
    ctx.fillStyle = C.pale; ctx.fill();
    // crest of moth fur, running front to back
    ctx.beginPath();
    ctx.moveTo(16, -27);
    ctx.bezierCurveTo(14, -54, -26, -60, -44, -18);
    ctx.lineTo(-31, -15);
    ctx.bezierCurveTo(-22, -38, 2, -42, 8, -29);
    ctx.closePath();
    ctx.fillStyle = C.pale; ctx.fill(); outline(ctx, 1.6);
    ctx.strokeStyle = C.paleShade; ctx.lineWidth = 1.2;
    for (const [x0, y0, x1, y1] of [[10, -34, 4, -48], [0, -36, -8, -50], [-10, -34, -20, -46], [-20, -28, -32, -36]]) {
      ctx.beginPath(); ctx.moveTo(x0, y0); ctx.lineTo(x1, y1); ctx.stroke();
    }
    // Corinthian helmet: dome, neck flare, nose guard and cheek guard
    ctx.beginPath();
    ctx.moveTo(-31, 22);
    ctx.bezierCurveTo(-36, 0, -30, -30, 0, -30);
    ctx.bezierCurveTo(22, -30, 31, -14, 30, -2);
    ctx.lineTo(29, 15); ctx.lineTo(24, 15); ctx.lineTo(23, 5);
    ctx.lineTo(16, 7); ctx.lineTo(17, 23);
    ctx.bezierCurveTo(10, 29, -14, 29, -22, 25);
    ctx.closePath();
    ctx.fillStyle = silverFill(ctx, -24, -30, 24, 26); ctx.fill(); outline(ctx, 1.8);
    // brow ridge and cheek line
    ctx.strokeStyle = C.silverDark; ctx.lineWidth = 1.4;
    ctx.beginPath(); ctx.moveTo(-26, -6); ctx.bezierCurveTo(-8, -12, 14, -12, 29, -6); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(-20, 8); ctx.bezierCurveTo(-6, 4, 6, 6, 14, 18); ctx.stroke();
    // almond eye slot with her eye inside
    ctx.beginPath(); ctx.moveTo(1, 0); ctx.quadraticCurveTo(14, -14, 27, -2); ctx.quadraticCurveTo(14, 10, 1, 0); ctx.closePath();
    ctx.fillStyle = C.ink; ctx.fill();
    ctx.beginPath(); ctx.arc(17, -2.5, 2.2, 0, Math.PI * 2); ctx.fillStyle = C.moon; ctx.fill();
    crown(ctx, p);
    // antennae sweep forward from the helmet's brow
    ctx.save(); ctx.translate(14, -27); ctx.scale(-1, 1); ctx.globalAlpha = 0.6; antenna(ctx, p.antenna * 0.8 - 0.2, -1); ctx.restore();
    ctx.save(); ctx.translate(18, -25); ctx.scale(-1, 1); antenna(ctx, p.antenna - 0.05, -1); ctx.restore();
    ctx.restore();
  }

  function mane(ctx) {
    const puffs = [[-14, -92, 11], [-4, -88, 12], [8, -90, 12], [18, -94, 10], [-8, -100, 10], [12, -100, 10], [24, -88, 8]];
    for (const [x, y, r] of puffs) {
      ctx.beginPath(); ctx.arc(x, y, r, 0, Math.PI * 2);
      ctx.fillStyle = C.pale; ctx.fill(); outline(ctx, 1.2);
    }
    for (const [x, y, r] of puffs) { ctx.beginPath(); ctx.arc(x, y + r * 0.35, r * 0.6, 0, Math.PI * 2); ctx.fillStyle = C.paleShade; ctx.globalAlpha = 0.35; ctx.fill(); ctx.globalAlpha = 1; }
  }

  function torso(ctx) {
    ctx.beginPath();
    ctx.moveTo(-16, -92); ctx.bezierCurveTo(-22, -70, -20, -46, -16, -34);
    ctx.lineTo(18, -34); ctx.bezierCurveTo(22, -50, 22, -76, 14, -92); ctx.closePath();
    ctx.fillStyle = C.body; ctx.fill(); outline(ctx, 1.8);
    // breastplate
    ctx.beginPath(); ctx.moveTo(-4, -84); ctx.bezierCurveTo(16, -86, 22, -70, 18, -54); ctx.lineTo(0, -52); ctx.closePath();
    ctx.fillStyle = silverFill(ctx, 0, -86, 20, -52); ctx.fill(); outline(ctx, 1.2);
    // meander band
    ctx.fillStyle = C.cloakTop; ctx.fillRect(-17, -42, 35, 7);
    ctx.strokeStyle = C.silverLight; ctx.lineWidth = 1;
    for (let x = -15; x < 16; x += 8) { ctx.beginPath(); ctx.moveTo(x, -36); ctx.lineTo(x, -41); ctx.lineTo(x + 5, -41); ctx.lineTo(x + 5, -38); ctx.lineTo(x + 3, -38); ctx.stroke(); }
  }

  // Hoplite aspis on the far arm, held across the body.
  function shield(ctx, p) {
    ctx.save();
    ctx.translate(4 + p.shieldUp * 8 - p.shieldBack * 22, -62 - p.shieldUp * 14 + p.shieldBack * 6);
    ctx.rotate(p.shieldUp * 0.2 - p.shieldBack * 0.5);
    ctx.beginPath(); ctx.arc(0, 0, 24, 0, Math.PI * 2);
    ctx.fillStyle = bronzeFill(ctx, -24, -24, 24, 24); ctx.fill(); outline(ctx, 1.8);
    ctx.beginPath(); ctx.arc(0, 0, 19, 0, Math.PI * 2); ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 2; ctx.stroke();
    ctx.beginPath(); ctx.arc(0, 0, 21.5, 0, Math.PI * 2); ctx.strokeStyle = C.bronzeLight; ctx.lineWidth = 0.8; ctx.stroke();
    // glowing crescent emblem
    ctx.save(); ctx.shadowColor = C.moon; ctx.shadowBlur = 6 + 4 * p.orb;
    ctx.beginPath(); ctx.arc(-2, 0, 10, 0, Math.PI * 2); ctx.arc(2.5, -2.5, 8, 0, Math.PI * 2, true);
    ctx.fillStyle = C.moon; ctx.fill('evenodd'); ctx.restore();
    ctx.restore();
  }

  // Free hand thrust forward with a moonlight hex sigil, used by the cast.
  function hexHand(ctx, p) {
    if (p.hex <= 0.01) return;
    const k = p.hex, hx = 14 + 30 * k, hy = -78 - 6 * k;
    ctx.save();
    ctx.strokeStyle = C.body; ctx.lineWidth = 6; ctx.lineCap = 'round';
    ctx.beginPath(); ctx.moveTo(6, -80); ctx.quadraticCurveTo(hx - 10, hy + 8, hx, hy); ctx.stroke();
    ctx.lineWidth = 1.2; ctx.strokeStyle = C.ink; ctx.stroke();
    // open palm with spread fingers
    ctx.fillStyle = C.body;
    ctx.beginPath(); ctx.arc(hx, hy, 4.5, 0, Math.PI * 2); ctx.fill(); outline(ctx, 1.1);
    ctx.strokeStyle = C.body; ctx.lineWidth = 2.2;
    for (const a of [-0.9, -0.3, 0.3, 0.9]) { ctx.beginPath(); ctx.moveTo(hx, hy); ctx.lineTo(hx + Math.cos(a) * 8, hy + Math.sin(a) * 8); ctx.stroke(); }
    // sigil: outer ring with rune ticks, inner crescent
    const sx = hx + 20 + 10 * k, r = 8 + 18 * k;
    ctx.translate(sx, hy);
    ctx.shadowColor = C.moon; ctx.shadowBlur = 8 + 10 * k;
    ctx.globalAlpha = Math.min(1, k * 1.4);
    ctx.strokeStyle = C.moon; ctx.lineWidth = 1.8;
    ctx.beginPath(); ctx.ellipse(0, 0, r * 0.45, r, 0, 0, Math.PI * 2); ctx.stroke();
    for (let i = 0; i < 6; i++) {
      const a = p.spin + i * Math.PI / 3, cx = Math.cos(a) * r * 0.45, cy = Math.sin(a) * r;
      ctx.beginPath(); ctx.arc(cx, cy, 1.6, 0, Math.PI * 2); ctx.fillStyle = C.moonCore; ctx.fill();
    }
    ctx.beginPath(); ctx.ellipse(-1, 0, r * 0.28, r * 0.62, 0, 0, Math.PI * 2); ctx.ellipse(1.5, -1.5, r * 0.22, r * 0.5, 0, 0, Math.PI * 2, true);
    ctx.fillStyle = C.moon; ctx.fill('evenodd');
    ctx.restore();
  }

  function spear(ctx, p) {
    ctx.save();
    ctx.translate(46 + p.staffReach * 10, -54 - p.staffLift * 10);
    ctx.rotate(p.staff);
    // shaft with a bronze butt-spike
    ctx.strokeStyle = C.wood; ctx.lineWidth = 3.5; ctx.lineCap = 'round';
    ctx.beginPath(); ctx.moveTo(0, -52); ctx.lineTo(0, 58); ctx.stroke();
    ctx.lineWidth = 0.8; ctx.strokeStyle = C.ink; ctx.beginPath(); ctx.moveTo(2, -52); ctx.lineTo(2, 58); ctx.stroke();
    ctx.beginPath(); ctx.moveTo(-2.5, 56); ctx.lineTo(0, 66); ctx.lineTo(2.5, 56); ctx.closePath();
    ctx.fillStyle = C.bronze; ctx.fill();
    // leaf-shaped bronze blade with a faint moonlight edge
    ctx.save(); ctx.shadowColor = C.moon; ctx.shadowBlur = 4 * p.orb;
    ctx.beginPath(); ctx.moveTo(0, -50); ctx.bezierCurveTo(7, -58, 6, -70, 0, -82); ctx.bezierCurveTo(-6, -70, -7, -58, 0, -50);
    ctx.fillStyle = bronzeFill(ctx, -6, -82, 6, -50); ctx.fill(); outline(ctx, 1.2);
    ctx.restore();
    ctx.beginPath(); ctx.moveTo(0, -52); ctx.lineTo(0, -78); ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 1; ctx.stroke();
    // hand
    ctx.beginPath(); ctx.arc(0, 4, 4.5, 0, Math.PI * 2); ctx.fillStyle = C.body; ctx.fill(); outline(ctx, 1.2);
    ctx.restore();
  }

  function draw(ctx, pose) {
    const p = Object.assign({}, BASE, pose);
    ctx.save();
    ctx.globalAlpha = p.alpha;
    ctx.translate(FRAME.originX + p.dx, FRAME.originY + p.bob);
    ctx.rotate(p.rot);
    ctx.scale(1 + p.squash * 0.08, 1 - p.squash * 0.1);
    ctx.save();
    ctx.translate(0, 0);
    ctx.rotate(p.tilt);
    // back layer
    mantle(ctx, p);
    ctx.save(); ctx.translate(-6, -88); wing(ctx, 0.25 + p.wingBack, p.wingOpen, 0.75); ctx.restore();
    ctx.restore();
    leg(ctx, -5, p.legBack, p.squash);
    ctx.save(); ctx.rotate(p.tilt);
    torso(ctx);
    ctx.restore();
    leg(ctx, 6, p.legFront, p.squash);
    ctx.save(); ctx.rotate(p.tilt);
    ctx.save(); ctx.translate(-2, -88); wing(ctx, 0.1 + p.wingFront, p.wingOpen, 1); ctx.restore();
    shield(ctx, p);
    mane(ctx);
    head(ctx, p);
    hexHand(ctx, p);
    spear(ctx, p);
    ctx.restore();
    ctx.restore();
    if (p.flash > 0) {
      // white hit flash over the whole figure
      ctx.save(); ctx.globalCompositeOperation = 'source-atop'; ctx.fillStyle = `rgba(255,255,255,${p.flash})`;
      ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height); ctx.restore();
    }
  }

  // Animations: name -> { fps, loop, frames: pose[] }
  const TAU = Math.PI * 2;
  function cycle(n, fn) { return Array.from({ length: n }, (_, i) => fn(i / n * TAU, i)); }

  // Linear keyframes: keys = [[time 0..1, pose], ...] sampled into n frames.
  function keys(n, ks) {
    return Array.from({ length: n }, (_, i) => {
      const t = n === 1 ? 0 : i / (n - 1);
      let j = 0; while (j < ks.length - 2 && t > ks[j + 1][0]) j++;
      const [t0, a] = ks[j], [t1, b] = ks[j + 1], u = Math.min(1, Math.max(0, (t - t0) / (t1 - t0 || 1)));
      const out = {};
      for (const k of new Set([...Object.keys(a), ...Object.keys(b)])) {
        const va = k in a ? a[k] : BASE[k], vb = k in b ? b[k] : BASE[k];
        out[k] = va + (vb - va) * u;
      }
      return out;
    });
  }

  const ANIMS = {
    idle: { fps: 8, loop: true, frames: cycle(8, t => ({
      bob: Math.sin(t) * 1.5, antenna: Math.sin(t + 1) * 0.08, wingBack: Math.sin(t) * 0.04,
      wingFront: Math.sin(t + 0.5) * 0.04, head: Math.sin(t) * 0.02, orb: 0.75 + 0.25 * Math.sin(t), staff: 0.02,
    })) },
    run: { fps: 14, loop: true, frames: cycle(8, t => ({
      bob: -Math.abs(Math.sin(t)) * 5 + 2, tilt: 0.1, legFront: Math.sin(t) * 0.7, legBack: -Math.sin(t) * 0.7,
      wingBack: 0.35 + Math.sin(t * 2) * 0.06, wingFront: 0.3 + Math.sin(t * 2 + 1) * 0.06,
      antenna: -0.25 + Math.sin(t * 2) * 0.06, staff: 0.35, staffReach: 0.2, head: -0.04, orb: 0.8,
    })) },
    jump: { fps: 12, loop: false, frames: keys(4, [
      [0, { squash: 0.6, legFront: 0.3, legBack: -0.3, wingOpen: 0.2 }],
      [0.4, { squash: -0.4, bob: -6, legFront: -0.5, legBack: 0.6, wingOpen: 0.6, wingBack: -0.2, wingFront: -0.25, antenna: 0.2, staff: -0.15 }],
      [1, { squash: -0.2, bob: -6, legFront: -0.4, legBack: 0.5, wingOpen: 0.4, wingBack: -0.1, wingFront: -0.15, antenna: 0.25, staff: -0.1 }],
    ]) },
    fall: { fps: 10, loop: true, frames: cycle(4, t => ({
      bob: -4, legFront: 0.35 + Math.sin(t) * 0.05, legBack: -0.2, wingOpen: 0.9 + Math.sin(t) * 0.1,
      wingBack: -0.5 + Math.sin(t) * 0.08, wingFront: -0.45 + Math.sin(t + 0.6) * 0.08, antenna: -0.3, staff: 0.15, tilt: -0.04,
    })) },
    land: { fps: 16, loop: false, frames: keys(3, [
      [0, { squash: 0.9, legFront: 0.2, legBack: -0.2, wingOpen: 0.5, wingBack: -0.3, antenna: -0.3 }],
      [1, { squash: 0, antenna: 0.05 }],
    ]) },
    dash: { fps: 18, loop: false, frames: keys(5, [
      [0, { tilt: 0.35, bob: -4, wingOpen: 1, wingBack: 0.9, wingFront: 0.9, legFront: -0.9, legBack: -1.1, staff: 1.2, staffReach: 0.4, antenna: -0.6, squash: -0.3 }],
      [0.6, { tilt: 0.4, bob: -4, wingOpen: 1, wingBack: 1.1, wingFront: 1.05, legFront: -1, legBack: -1.2, staff: 1.3, staffReach: 0.5, antenna: -0.7, squash: -0.35 }],
      [1, { tilt: 0.15, wingOpen: 0.3, wingBack: 0.3, wingFront: 0.3, legFront: -0.3, legBack: -0.3, staff: 0.6, antenna: -0.2 }],
    ]) },
    attack: { fps: 20, loop: false, frames: keys(6, [
      [0, { tilt: -0.05, staff: 1.4, staffReach: -1.2, legFront: 0.3, legBack: -0.3, shieldUp: 0.3 }],
      [0.35, { tilt: 0.15, staff: Math.PI / 2, staffReach: 3.2, legFront: 0.6, legBack: -0.5, wingBack: 0.2, antenna: -0.3, shieldUp: 0.1 }],
      [0.6, { tilt: 0.12, staff: Math.PI / 2, staffReach: 3, legFront: 0.5, legBack: -0.4, wingBack: 0.2, antenna: -0.25 }],
      [1, { staff: 0.3, staffReach: 0.2 }],
    ]) },
    attackUp: { fps: 20, loop: false, frames: keys(6, [
      [0, { staff: -0.1, staffLift: -1.5, squash: 0.3 }],
      [0.35, { staff: -0.05, staffLift: 6, staffReach: -0.6, bob: -3, squash: -0.3, head: -0.12, antenna: 0.35, wingOpen: 0.3 }],
      [0.6, { staff: -0.05, staffLift: 5.6, staffReach: -0.6, bob: -2, head: -0.1, antenna: 0.3 }],
      [1, { staff: 0.02 }],
    ]) },
    attackDown: { fps: 20, loop: false, frames: keys(6, [
      [0, { staff: Math.PI - 0.2, staffLift: 2, staffReach: -0.6, legFront: -0.4, legBack: 0.4, wingOpen: 0.5, wingBack: -0.4, wingFront: -0.4 }],
      [0.35, { staff: Math.PI, staffLift: -4.8, staffReach: -0.8, legFront: -0.6, legBack: 0.5, wingOpen: 0.8, wingBack: -0.6, wingFront: -0.6, head: 0.12 }],
      [0.6, { staff: Math.PI, staffLift: -4.5, staffReach: -0.8, legFront: -0.5, legBack: 0.4, wingOpen: 0.7, wingBack: -0.5, wingFront: -0.5, head: 0.1 }],
      [1, { staff: Math.PI - 0.1, staffLift: 1, staffReach: -0.6, wingOpen: 0.4 }],
    ]) },
    cast: { fps: 14, loop: false, frames: keys(8, [
      [0, { squash: 0.4, shieldBack: 0.4, wingOpen: 0.2, hex: 0.15, staff: 0.15, staffReach: 0.3 }],
      [0.3, { squash: -0.15, bob: -3, shieldBack: 1, hex: 0.85, spin: 1.2, wingOpen: 0.9, wingBack: -0.4, wingFront: -0.45, antenna: 0.35, orb: 2.2, staff: 0.3, staffReach: 0.6, tilt: 0.08 }],
      [0.65, { squash: -0.2, bob: -4, shieldBack: 1, hex: 1, spin: 2.6, wingOpen: 1.1, wingBack: -0.5, wingFront: -0.55, antenna: 0.45, orb: 3, staff: 0.3, staffReach: 0.6, tilt: 0.12 }],
      [1, { shieldBack: 0.2, hex: 0, spin: 3.4, orb: 1, staffReach: 0.2 }],
    ]) },
    block: { fps: 16, loop: false, frames: keys(5, [
      [0, { shieldUp: 0.4, tilt: -0.05, legFront: 0.3, legBack: -0.3, staff: 1.2, staffReach: -1.2 }],
      [0.3, { shieldUp: 1.3, tilt: -0.12, dx: -3, bob: 2, squash: 0.06, legFront: 0.6, legBack: -0.5, staff: 1.1, staffReach: -1.4, antenna: 0.3, wingOpen: 0.2, flash: 0.12 }],
      [1, { shieldUp: 1.15, tilt: -0.1, dx: -2, bob: 1, squash: 0.04, legFront: 0.5, legBack: -0.45, staff: 1.15, staffReach: -1.3, antenna: 0.2 }],
    ]) },
    hurt: { fps: 14, loop: false, frames: keys(4, [
      [0, { flash: 0.85, tilt: -0.3, dx: -6, bob: -3, head: 0.15, antenna: 0.6, legFront: -0.5, legBack: 0.4, staff: -0.4, wingOpen: 0.6 }],
      [0.4, { flash: 0.2, tilt: -0.25, dx: -8, bob: -2, head: 0.12, antenna: 0.5, legFront: -0.4, legBack: 0.3, staff: -0.35, wingOpen: 0.5 }],
      [1, { tilt: -0.05, dx: -2 }],
    ]) },
    death: { fps: 10, loop: false, frames: keys(10, [
      [0, { flash: 0.8, tilt: -0.2, head: 0.2, antenna: 0.6, staff: -0.4 }],
      [0.3, { rot: -0.6, dx: 10, bob: -2, antenna: 0.8, staff: -0.9, wingOpen: 0.7, legFront: -0.6, legBack: 0.5 }],
      [0.6, { rot: -1.3, dx: 40, bob: -6, antenna: 1, staff: -1.4, wingOpen: 1, wingBack: -0.6, wingFront: -0.6, legFront: -0.3, legBack: 0.3, orb: 0.3 }],
      [1, { rot: -1.35, dx: 40, bob: -6, antenna: 1.1, staff: -1.45, wingOpen: 1.1, wingBack: -0.7, wingFront: -0.7, orb: 0, alpha: 0.35 }],
    ]) },
  };

  root.HexMoth = { draw, FRAME, ANIMS, COLORS: C };
})(typeof window !== 'undefined' ? window : globalThis);
