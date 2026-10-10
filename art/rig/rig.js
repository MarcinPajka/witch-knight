// Hex Moth side-view rig. Draws the heroine facing right from a pose object,
// so the same code drives the preview page and the sprite-sheet export.
(function (root) {
  const C = {
    pale: '#e9e6f2', paleShade: '#c3bdd6', ink: '#0d0f17', body: '#1d1828',
    cloakTop: '#4a4258', cloakBottom: '#16131d', moon: '#c8e07a', moonCore: '#f4ffd0',
    bronzeLight: '#e0b56a', bronze: '#b07a3a', bronzeDark: '#6e4520', wood: '#3a2c20',
  };

  // Frame size in pixels; the origin sits between the feet.
  const FRAME = { w: 160, h: 200, originX: 76, originY: 190 };

  const BASE = {
    bob: 0, tilt: 0, legBack: 0, legFront: 0, wingBack: 0, wingFront: 0,
    wingOpen: 0, staff: 0, staffReach: 0, antenna: 0, head: 0, orb: 1, squash: 0,
  };

  function bronzeFill(ctx, x0, y0, x1, y1) {
    const g = ctx.createLinearGradient(x0, y0, x1, y1);
    g.addColorStop(0, C.bronzeLight); g.addColorStop(0.55, C.bronze); g.addColorStop(1, C.bronzeDark);
    return g;
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

  function leg(ctx, x, angle, squash) {
    ctx.save();
    ctx.translate(x, -38 + squash * 6);
    ctx.rotate(angle);
    ctx.beginPath();
    ctx.moveTo(-5, 0); ctx.lineTo(5, 0); ctx.lineTo(4, 30 - squash * 6); ctx.quadraticCurveTo(0, 36 - squash * 6, -4, 30 - squash * 6);
    ctx.closePath();
    ctx.fillStyle = C.body; ctx.fill(); outline(ctx, 1.5);
    // bronze greave
    ctx.beginPath(); ctx.rect(-4.5, 12, 9, 12 - squash * 4);
    ctx.fillStyle = bronzeFill(ctx, -4, 12, 4, 24); ctx.fill(); outline(ctx, 1);
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
    // antennae (far one first, dimmer)
    ctx.save(); ctx.translate(-4, -26); ctx.globalAlpha = 0.6; antenna(ctx, p.antenna * 0.8 - 0.15, 1); ctx.restore();
    ctx.save(); ctx.translate(2, -27); antenna(ctx, p.antenna, 1); ctx.restore();
    // mask
    ctx.beginPath(); ctx.ellipse(0, 0, 25, 27, 0, 0, Math.PI * 2);
    ctx.fillStyle = C.pale; ctx.fill(); outline(ctx, 2);
    // helmet cap with nose guard
    ctx.beginPath();
    ctx.moveTo(-27, 2);
    ctx.bezierCurveTo(-30, -30, 24, -36, 27, -6);
    ctx.lineTo(24, -4); ctx.lineTo(24, 12); ctx.lineTo(19, 12); ctx.lineTo(19, -6);
    ctx.bezierCurveTo(10, -16, -14, -16, -20, 6);
    ctx.closePath();
    ctx.fillStyle = bronzeFill(ctx, -20, -30, 20, 10); ctx.fill(); outline(ctx, 1.6);
    // eye
    ctx.beginPath(); ctx.ellipse(10, 4, 7, 9, -0.1, 0, Math.PI * 2); ctx.fillStyle = C.ink; ctx.fill();
    ctx.beginPath(); ctx.arc(12, 1, 1.8, 0, Math.PI * 2); ctx.fillStyle = C.moon; ctx.fill();
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
    ctx.fillStyle = bronzeFill(ctx, 0, -86, 20, -52); ctx.fill(); outline(ctx, 1.2);
    // meander band
    ctx.fillStyle = C.bronzeDark; ctx.fillRect(-17, -42, 35, 7);
    ctx.strokeStyle = C.bronzeLight; ctx.lineWidth = 1;
    for (let x = -15; x < 16; x += 8) { ctx.beginPath(); ctx.moveTo(x, -36); ctx.lineTo(x, -41); ctx.lineTo(x + 5, -41); ctx.lineTo(x + 5, -38); ctx.lineTo(x + 3, -38); ctx.stroke(); }
  }

  function buckler(ctx) {
    ctx.beginPath(); ctx.arc(-14, -64, 13, 0, Math.PI * 2);
    ctx.fillStyle = bronzeFill(ctx, -26, -76, -2, -52); ctx.fill(); outline(ctx, 1.5);
    ctx.beginPath(); ctx.arc(-14, -64, 9, 0, Math.PI * 2); ctx.strokeStyle = C.bronzeDark; ctx.lineWidth = 1.4; ctx.stroke();
    ctx.beginPath(); ctx.arc(-14, -64, 3.5, 0, Math.PI * 2); ctx.fillStyle = C.moon; ctx.fill();
  }

  function staff(ctx, p) {
    ctx.save();
    ctx.translate(32 + p.staffReach * 10, -48);
    ctx.rotate(p.staff);
    ctx.strokeStyle = C.wood; ctx.lineWidth = 4; ctx.lineCap = 'round';
    ctx.beginPath(); ctx.moveTo(0, -30); ctx.lineTo(0, 54); ctx.stroke();
    ctx.lineWidth = 1; ctx.strokeStyle = C.ink; ctx.beginPath(); ctx.moveTo(2.4, -30); ctx.lineTo(2.4, 54); ctx.stroke();
    // bronze cradle and orb
    ctx.beginPath(); ctx.moveTo(-6, -32); ctx.quadraticCurveTo(0, -24, 6, -32); ctx.strokeStyle = C.bronze; ctx.lineWidth = 2.5; ctx.stroke();
    const r = 7 + 2 * p.orb;
    const g = ctx.createRadialGradient(0, -40, 0, 0, -40, r * 2.2);
    g.addColorStop(0, C.moonCore); g.addColorStop(0.4, C.moon); g.addColorStop(1, 'rgba(200,224,122,0)');
    ctx.fillStyle = g; ctx.beginPath(); ctx.arc(0, -40, r * 2.2, 0, Math.PI * 2); ctx.fill();
    // hand
    ctx.beginPath(); ctx.arc(0, 4, 4.5, 0, Math.PI * 2); ctx.fillStyle = C.body; ctx.fill(); outline(ctx, 1.2);
    ctx.restore();
  }

  function draw(ctx, pose) {
    const p = Object.assign({}, BASE, pose);
    ctx.save();
    ctx.translate(FRAME.originX, FRAME.originY + p.bob);
    ctx.scale(1 + p.squash * 0.08, 1 - p.squash * 0.1);
    ctx.save();
    ctx.translate(0, 0);
    ctx.rotate(p.tilt);
    // back layer
    ctx.save(); ctx.translate(-6, -88); wing(ctx, 0.25 + p.wingBack, p.wingOpen, 0.75); ctx.restore();
    ctx.restore();
    leg(ctx, -5, p.legBack, p.squash);
    ctx.save(); ctx.rotate(p.tilt);
    buckler(ctx);
    torso(ctx);
    ctx.restore();
    leg(ctx, 6, p.legFront, p.squash);
    ctx.save(); ctx.rotate(p.tilt);
    ctx.save(); ctx.translate(-2, -88); wing(ctx, 0.1 + p.wingFront, p.wingOpen, 1); ctx.restore();
    mane(ctx);
    head(ctx, p);
    staff(ctx, p);
    ctx.restore();
    ctx.restore();
  }

  // Animations: name -> { fps, loop, frames: pose[] }
  const TAU = Math.PI * 2;
  function cycle(n, fn) { return Array.from({ length: n }, (_, i) => fn(i / n * TAU, i)); }

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
  };

  root.HexMoth = { draw, FRAME, ANIMS, COLORS: C };
})(typeof window !== 'undefined' ? window : globalThis);
