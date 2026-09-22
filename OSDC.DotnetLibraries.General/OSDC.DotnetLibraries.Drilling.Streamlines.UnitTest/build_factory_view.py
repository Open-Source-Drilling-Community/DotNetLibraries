"""Builds the 3D view from the scene that UllriggFactoryScene.WriteFactoryScenes writes.

    dotnet test -c Release --filter "FullyQualifiedName~WriteFactoryScenes"
    python build_factory_view.py
    python -m http.server 8735 --bind 127.0.0.1     # then open ullrigg-factory-view.html

Reads factory-view-source.html beside it and the scene from %TEMP% (or from beside this file).
"""

import io, os, json

here = os.path.dirname(os.path.abspath(__file__))
src = io.open(os.path.join(here, "factory-view-source.html"), encoding="utf-8").read()

head = src[:src.index("<div class=\"shell\">")]

body = """<style>
  /* the controls move out of the picture and become a column beside it, so the view is not looked at
     through a gap between panels */
  .stage { display: flex; flex-direction: row; align-items: stretch; min-height: 520px; }
  .controls {
    position: static; left: auto; bottom: auto;
    flex: 0 0 272px; width: 272px; max-width: 272px;
    border-radius: 0; border-block: 0; border-left: 0; box-shadow: none;
    background: var(--panel); overflow-y: auto; gap: 14px;
    padding-block: 14px; padding-inline: 14px;
  }
  .viewport { position: relative; flex: 1 1 auto; min-width: 0; }
  canvas { display: block; width: 100%; height: 100%; touch-action: none; }
  .controls .group { flex-direction: column; align-items: flex-start; gap: 6px; }
  .controls .filter { width: 100%; }
  .routeline { width: 100%; display: flex; align-items: baseline; gap: 6px;
               white-space: nowrap; font-size: 11px; }
  .startmark { color: var(--accent); font-weight: 700; }
  /* a case whose target is not the one on show: clicking it swaps rather than adds */
  .cases button.othertarget { opacity: .45; font-style: italic; }
  .cases button.othertarget[aria-pressed="true"] { opacity: 1; font-style: normal; }
  .notebox { border-top: 1px solid var(--panel-edge); background: var(--panel); }
  .notebox > summary {
    cursor: pointer; list-style: none; padding-block: 9px; padding-inline: 20px;
    font-size: 10px; letter-spacing: .08em; text-transform: uppercase; color: var(--muted);
    display: flex; align-items: center; gap: 7px;
  }
  .notebox > summary::-webkit-details-marker { display: none; }
  .notebox > summary::before { content: "b8"; font-size: 11px; transition: transform .15s; }
  .notebox[open] > summary::before { transform: rotate(90deg); }
  .notebox > summary:hover { color: var(--ink); }
  .notebox > summary:focus-visible { outline: 2px solid var(--accent); outline-offset: -2px; }
  @media (max-width: 900px) {
    .stage { flex-direction: column; min-height: 0; }
    .controls { flex: none; width: auto; max-width: none; border-left: 0;
                border-top: 1px solid var(--panel-edge); }
    .viewport { min-height: 340px; }
    .controls .group { flex-direction: row; flex-wrap: wrap; align-items: center; }
    .routeline { width: auto; }
  }
</style>

<div class="shell">
  <header>
    <div class="heading">
      <h1>Ullrigg bundle factories</h1>
      <p class="sub">Each bundle of candidate paths replaced by its factory: a median path, outlines
      perpendicular to it that contain every crossing, and streamlines drawn from one density. Set how
      much room a well needs around the planned path and the corridors that cannot give it drop out.
      A case starts either at a new slot or at a window on an existing well, marked
      <span class="startmark">&#10005;</span>; a volume drawn faintly is a well that case does not
      count as a constraint. Cases that reach the same target can be shown together \u2014 click
      more than one; a case reaching a different target replaces the selection rather than joining
      it, there being nothing to compare. Shown together, corridors are coloured by case rather
      than by route.</p>
    </div>
    <div class="cases" id="cases" role="group" aria-label="Which case to show"></div>
  </header>

  <dl class="figures" id="figures"></dl>

  <div class="stage">
    <div class="controls">
      <div class="group">
        <span class="grouplabel">Room needed around the planned path</span>
        <span class="filter">
          <input type="range" id="clearance" min="0" max="12" step="0.5" value="5"
                 aria-label="Least inradius a bundle must offer, metres">
          <output id="clearanceValue">5.0 m</output>
        </span>
      </div>
      <div class="group">
        <span class="grouplabel">Hardest turn the planned path may take</span>
        <span class="filter">
          <input type="range" id="dogleg" min="2" max="60" step="0.5" value="60"
                 aria-label="Worst dogleg of the median, degrees per 30 m">
          <output id="doglegValue">any</output>
        </span>
      </div>
      <div class="group" id="bundlelist"></div>
      <div class="group">
        <span class="grouplabel">Layers</span>
        <label class="toggle"><input type="checkbox" id="showMedian" checked><span class="long">Median paths</span><span class="short">Medians</span></label>
        <label class="toggle"><input type="checkbox" id="showOutline" checked><span class="long">Cross-section outlines</span><span class="short">Outlines</span></label>
        <label class="toggle"><input type="checkbox" id="showDraws" checked><span class="long">Drawn streamlines</span><span class="short">Draws</span></label>
        <label class="toggle"><input type="checkbox" id="showVolumes" checked><span class="swatch" style="background:var(--volume)"></span><span class="long">Uncertainty volumes</span><span class="short">Volumes</span></label>
        <label class="toggle"><input type="checkbox" id="showTarget" checked><span class="swatch" style="background:var(--goal)"></span><span class="long">Target</span><span class="short">Target</span></label>
        <label class="toggle"><input type="checkbox" id="showGround" checked><span class="swatch" style="background:var(--grid)"></span><span class="long">Grid &amp; scale</span><span class="short">Grid</span></label>
      </div>
      <div class="group">
        <span class="grouplabel">View</span>
        <button class="view" data-view="iso" type="button"><span class="long">Isometric</span><span class="short">Iso</span></button>
        <button class="view" data-view="north" type="button"><span class="long">Section N</span><span class="short">Sec N</span></button>
        <button class="view" data-view="east" type="button"><span class="long">Section E</span><span class="short">Sec E</span></button>
        <button class="view" data-view="plan" type="button"><span class="long">Plan</span><span class="short">Plan</span></button>
        <label class="toggle spin"><input type="checkbox" id="spin" checked><span class="long">Rotate</span><span class="short">Spin</span></label>
      </div>
    </div>
    <div class="viewport">
      <canvas id="view"></canvas>
      <div class="readout" id="readout">drag to orbit &middot; wheel to zoom &middot; right-drag to pan</div>
    </div>
  </div>

  <details class="notebox" id="notebox">
    <summary>How to read this</summary>
    <p class="note" id="note"></p>
  </details>
</div>

<script id="scene-data" type="application/json">/*SCENE_JSON*/</script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js"></script>
<script>
(function () {
  var data = JSON.parse(document.getElementById("scene-data").textContent);
  // one set of corridors per case; the wells, the slot and the ground are shared between them
  var cases = data.cases || [{ name: "case", target: data.target, bundles: data.bundles }];
  var current = 0;                 // the case whose target decides what may be superimposed
  var picked = [0];                // every case currently drawn

  // Two cases are comparable when they reach the same target. Superimposing corridors that go to
  // different places would put two unrelated pictures in one frame, so the target is the test.
  function targetKey(one) {
    return (one.target || []).map(function (p) {
      return p.map(function (x) { return Math.round(x * 100) / 100; }).join(",");
    }).join(";");
  }
  function comparable(index) {
    return picked.length === 0 || targetKey(cases[index]) === targetKey(cases[picked[0]]);
  }
  // one hue per case, used only when more than one is on show
  function caseColour(index, into) {
    return into.setHSL((0.02 + index * 0.37) % 1, 0.68, 0.47);
  }
  var canvas = document.getElementById("view");
  var figures = document.getElementById("figures");
  var note = document.getElementById("note");
  var readout = document.getElementById("readout");

  function css(name) {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  }
  function colour(name) { return new THREE.Color(css(name)); }
  function toScene(p) { return new THREE.Vector3(p[1], -p[2], -p[0]); }

  var renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true });
  renderer.setPixelRatio(Math.min(2, window.devicePixelRatio || 1));
  var scene = new THREE.Scene();
  var camera = new THREE.PerspectiveCamera(42, 1, 1, 60000);
  var tinted = [];

  // a hue off the golden angle, so neighbouring bundles never get neighbouring colours
  function bundleColour(index, into) {
    return into.setHSL((0.55 + index * 0.618034) % 1, 0.62, 0.48);
  }

  // framed on every case at once, so switching between them does not move the camera
  var box = new THREE.Box3();
  cases.forEach(function (one) {
    one.bundles.forEach(function (bundle) {
      bundle.median.forEach(function (p) { box.expandByPoint(toScene(p)); });
    });
    one.target.forEach(function (p) { box.expandByPoint(toScene(p)); });
    if (one.start) { box.expandByPoint(toScene(one.start)); }
  });
  var focus = box.getCenter(new THREE.Vector3());
  var reach = Math.max(200, box.getSize(new THREE.Vector3()).length());

  // ---- the bundles ------------------------------------------------------------------------------
  var groups = [];
  var caseGroups = [];
  var swatch = new THREE.Color();
  cases.forEach(function (one, caseIndex) {
  var forCase = [];
  one.bundles.forEach(function (bundle, index) {
    bundleColour(bundle.group || 0, swatch);
    var group = new THREE.Group();

    var medianMaterial = new THREE.LineBasicMaterial({ color: swatch.clone(), linewidth: 2 });
    var medianGeometry = new THREE.BufferGeometry().setFromPoints(bundle.median.map(toScene));
    var median = new THREE.Line(medianGeometry, medianMaterial);
    group.add(median);

    var outlines = new THREE.Group();
    var outlineMaterial = new THREE.LineBasicMaterial({
      color: swatch.clone(), transparent: true, opacity: 0.55
    });
    bundle.outlines.forEach(function (one) {
      var points = one.ring.map(toScene);
      if (points.length > 0) { points.push(points[0]); }
      outlines.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints(points), outlineMaterial));
    });
    group.add(outlines);

    var draws = new THREE.Group();
    var drawMaterial = new THREE.LineBasicMaterial({
      color: swatch.clone(), transparent: true, opacity: 0.4
    });
    bundle.draws.forEach(function (one) {
      draws.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints(one.map(toScene)),
                               drawMaterial));
    });
    group.add(draws);

    scene.add(group);
    var entry = { group: group, median: median, outlines: outlines, draws: draws,
                  bundle: bundle, colour: swatch.clone(), caseIndex: caseIndex,
                  materials: [medianMaterial, outlineMaterial, drawMaterial] };
    forCase.push(entry);
    groups.push(entry);
  });
  caseGroups.push(forCase);
  });

  // ---- the wells, the target and the ground -----------------------------------------------------
  var volumes = new THREE.Group();
  var volumeMaterial = new THREE.LineBasicMaterial({
    color: colour("--volume"), transparent: true, opacity: 0.4
  });
  tinted.push({ material: volumeMaterial, token: "--volume" });
  // a well a case does not treat as a constraint is still drawn, faintly: it is there, it is simply
  // not being avoided, and hiding it would leave a corridor looking as though it went through nothing
  var droppedMaterial = new THREE.LineBasicMaterial({
    color: colour("--volume"), transparent: true, opacity: 0.1
  });
  tinted.push({ material: droppedMaterial, token: "--volume" });
  var wellGroups = [];
  data.wells.forEach(function (well) {
    var group = new THREE.Group();
    var lines = [];
    well.rings.forEach(function (ring) {
      var points = ring.map(toScene);
      if (points.length > 0) { points.push(points[0]); }
      var line = new THREE.Line(new THREE.BufferGeometry().setFromPoints(points), volumeMaterial);
      lines.push(line);
      group.add(line);
    });
    volumes.add(group);
    wellGroups.push({ name: well.name, group: group, lines: lines });
  });
  scene.add(volumes);

  // ---- where each case starts -------------------------------------------------------------------
  var startMarks = [];
  cases.forEach(function (one) {
    var mark = new THREE.Group();
    if (one.start) {
      var startMaterial = new THREE.LineBasicMaterial({ color: colour("--accent") });
      tinted.push({ material: startMaterial, token: "--accent" });
      [[12, 0, 0], [0, 12, 0], [0, 0, 12]].forEach(function (axis) {
        mark.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints([
          toScene([one.start[0] - axis[0], one.start[1] - axis[1], one.start[2] - axis[2]]),
          toScene([one.start[0] + axis[0], one.start[1] + axis[1], one.start[2] + axis[2]])
        ]), startMaterial));
      });
    }
    scene.add(mark);
    startMarks.push(mark);
  });

  var goals = [];
  cases.forEach(function (one) {
    var goal = new THREE.Group();
    var corners = one.target.map(toScene);
    var quad = new THREE.BufferGeometry().setFromPoints([
      corners[0], corners[1], corners[2], corners[0], corners[2], corners[3]
    ]);
    var face = new THREE.MeshBasicMaterial({
      color: colour("--goal"), transparent: true, opacity: 0.18, side: THREE.DoubleSide
    });
    var rim = new THREE.LineBasicMaterial({ color: colour("--goal") });
    tinted.push({ material: face, token: "--goal" }, { material: rim, token: "--goal" });
    goal.add(new THREE.Mesh(quad, face));
    goal.add(new THREE.LineLoop(new THREE.BufferGeometry().setFromPoints(corners), rim));
    scene.add(goal);
    goals.push(goal);
  });

  var groundGroup = new THREE.Group();
  var gridMaterial = new THREE.LineBasicMaterial({
    color: colour("--grid"), transparent: true, opacity: 0.5
  });
  tinted.push({ material: gridMaterial, token: "--grid" });
  var span = 700, step = 100;
  for (var g = -span; g <= span; g += step) {
    groundGroup.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints([
      toScene([g, -span, data.ground]), toScene([g, span, data.ground])]), gridMaterial));
    groundGroup.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints([
      toScene([-span, g, data.ground]), toScene([span, g, data.ground])]), gridMaterial));
  }
  scene.add(groundGroup);

  // ---- the clearance rule -----------------------------------------------------------------------
  var clearance = document.getElementById("clearance");
  var clearanceValue = document.getElementById("clearanceValue");
  var dogleg = document.getElementById("dogleg");
  var doglegValue = document.getElementById("doglegValue");
  var bundlelist = document.getElementById("bundlelist");

  function figure(label, value, sub, tone) {
    return '<div class="figure' + (tone ? " " + tone : "") + '"><dt>' + label + "</dt><dd>" + value
         + "</dd><dp>" + sub + "</dp></div>";
  }

  function apply() {
    var limit = parseFloat(clearance.value);
    clearanceValue.textContent = limit.toFixed(1) + " m";
    var turn = parseFloat(dogleg.value);
    var anyTurn = turn >= 60;
    doglegValue.textContent = anyTurn ? "any" : turn.toFixed(1) + " °/30 m";
    var kept = 0, keptMembers = 0, best = 0, gentlest = Infinity, nearestParent = Infinity;
    var many = picked.length > 1;
    var shown = [];
    var dropped = [];
    var parents = [];
    picked.forEach(function (index) {
      shown = shown.concat(cases[index].bundles);
      (cases[index].excluded || []).forEach(function (name) {
        if (dropped.indexOf(name) < 0) { dropped.push(name); }
      });
      if (cases[index].parent && parents.indexOf(cases[index].parent) < 0) {
        parents.push(cases[index].parent);
      }
    });
    groups.forEach(function (entry) {
      if (picked.indexOf(entry.caseIndex) < 0) {
        entry.group.visible = false;
        return;
      }
      // by route while one case is on show, by case once several are: with two sets of corridors in
      // one frame, which case a corridor belongs to is what has to be legible
      var tone = many ? caseColour(entry.caseIndex, swatch)
                      : bundleColour(entry.bundle.group || 0, swatch);
      entry.colour.copy(tone);
      entry.materials.forEach(function (material) { material.color.copy(tone); });
      var pass = entry.bundle.leastInradius >= limit
                 && (anyTurn || (entry.bundle.dogleg || 0) <= turn);
      entry.group.visible = pass;
      entry.median.visible = document.getElementById("showMedian").checked;
      entry.outlines.visible = document.getElementById("showOutline").checked;
      entry.draws.visible = document.getElementById("showDraws").checked;
      if (pass) {
        kept++;
        keptMembers += entry.bundle.members;
        if (entry.bundle.leastInradius > best) { best = entry.bundle.leastInradius; }
        if ((entry.bundle.dogleg || 0) < gentlest) { gentlest = entry.bundle.dogleg || 0; }
        if (entry.bundle.fromParent !== undefined && entry.bundle.fromParent < nearestParent) {
          nearestParent = entry.bundle.fromParent;
        }
      }
    });
    volumes.visible = document.getElementById("showVolumes").checked;
    wellGroups.forEach(function (one) {
      var out = dropped.indexOf(one.name) >= 0;
      one.lines.forEach(function (line) { line.material = out ? droppedMaterial : volumeMaterial; });
    });
    startMarks.forEach(function (mark, index) {
      mark.visible = picked.indexOf(index) >= 0;
    });
    // the cases on show share one target, so it is drawn once rather than once per case
    goals.forEach(function (one, index) {
      one.visible = index === picked[0] && document.getElementById("showTarget").checked;
    });
    groundGroup.visible = document.getElementById("showGround").checked;

    figures.innerHTML =
        figure("Corridors that qualify", kept + " / " + shown.length,
               "bundles with a factory", kept === 0 ? "flagged" : kept > 2 ? "clear" : "")
      + figure("Roomiest", best.toFixed(2) + "&#8239;m", "least room along its length",
               best >= limit ? "clear" : "flagged")
      + figure("Paths represented", String(keptMembers), "of " + shown.reduce(
               function (sum, b) { return sum + b.members; }, 0) + " in factories")
      + figure("Rule in force", limit.toFixed(1) + "&#8239;m",
               "room a driller can hold around a line")
      + figure("Routes apart", String(new Set(shown.filter(function (b) {
                 return b.leastInradius >= limit && (anyTurn || (b.dogleg || 0) <= turn);
               }).map(function (b) { return b.group || 0; })).size),
               "separated by forbidden ground")
      + figure("Gentlest plan", (gentlest === Infinity ? "—" : gentlest.toFixed(1))
               + "&#8239;°/30 m", "hardest turn on the easiest median",
               gentlest <= 10 ? "clear" : "")
      + (parents.length > 0
         ? figure(parents.length === 1 ? "Closest to " + parents[0] : "Closest to a parent",
                  (nearestParent === Infinity ? "\u2014" : nearestParent.toFixed(1)) + "&#8239;m",
                  "achieved, not asked for", nearestParent >= 20 ? "clear" : "")
         : "");

    bundlelist.innerHTML = '<span class="grouplabel">Bundles</span>'
      + groups.filter(function (entry) {
          return picked.indexOf(entry.caseIndex) >= 0;
        }).map(function (entry) {
          var pass = entry.bundle.leastInradius >= limit;
          return '<span class="routeline" style="opacity:' + (pass ? 1 : 0.35) + '">'
               + '<span class="swatch" style="background:#' + entry.colour.getHexString()
               + '"></span>'
               + (many ? cases[entry.caseIndex].name + "&#8239;&middot; " : "")
               + 'route&#8239;<b>' + (entry.bundle.group || 0) + "</b> &middot; "
               + entry.bundle.members + "&#8239;paths &middot; <b>"
               + entry.bundle.leastInradius.toFixed(2) + "</b>&#8239;m &middot; <b>"
               + (entry.bundle.dogleg || 0).toFixed(1) + "</b>/"
               + (entry.bundle.doglegLong || 0).toFixed(1) + "&#8239;°</span>";
        }).join("");

    readout.textContent = kept + " of " + shown.length + " corridors shown · "
      + (many ? picked.map(function (index) { return cases[index].name; }).join(" + ")
                + " together · " : "")
      + (data.wells.length - dropped.length) + " of " + data.wells.length
      + " volumes a constraint"
      + (many ? "" : cases[current].parent
         ? " · window on " + cases[current].parent + " at "
           + cases[current].departure.toFixed(0) + "° and "
           + cases[current].heading.toFixed(0) + "°"
         : " · new slot");
    note.innerHTML =
        "Each corridor is drawn as its factory rather than as the paths it came from: the <b>median"
      + "</b> path, the <b>outlines</b> of the cross-sections perpendicular to it, and twelve"
      + " <b>streamlines drawn</b> from the one density it holds. The outlines contain every crossing"
      + " of every original path by construction, so a draw of radius one lands on the boundary and"
      + " anything less lands inside. The rule is applied to the <b>least</b> room along a corridor,"
      + " not the average: a corridor thirty metres wide that is throttled to two somewhere is"
      + " undrillable where it is throttled. The ends are left out of that, being pinched by"
      + " construction \\u2014 the paths start together at the slot or the window and converge on"
      + " the target."
      + (many
         ? " <b>" + picked.length + " cases are shown together</b>, all reaching the same target, so"
           + " each corridor is coloured by the case it belongs to rather than by its route. The room"
           + " and the turn are comparable between them; the routes-apart count is worked out within"
           + " a case and does not carry across them."
         : "")
      + (!many && cases[current].parent
         ? " This case is a <b>sidetrack</b>: it starts on <b>" + cases[current].parent
           + "</b> and leaves along that well's own direction at the window, there being no way to"
           + " turn at a point. That well's uncertainty volume is drawn faintly and is <b>not</b> a"
           + " constraint here \\u2014 the old hole is being left behind. Every other volume still is,"
           + " and how close the plan comes to its parent is read off afterwards rather than asked"
           + " for."
         : "");
  }

  var switcher = document.getElementById("cases");
  if (switcher) { cases.forEach(function (one, index) {
    var button = document.createElement("button");
    button.type = "button";
    button.textContent = one.name;
    button.setAttribute("aria-pressed", index === 0 ? "true" : "false");
    button.addEventListener("click", function () {
      var at = picked.indexOf(index);
      if (at >= 0) {
        // never leave the frame empty: the last case on show stays on show
        if (picked.length > 1) { picked.splice(at, 1); }
      } else if (comparable(index)) {
        picked.push(index);
      } else {
        picked = [index];
      }
      current = picked[0];
      Array.prototype.forEach.call(switcher.children, function (other, k) {
        other.setAttribute("aria-pressed", picked.indexOf(k) >= 0 ? "true" : "false");
        other.classList.toggle("othertarget", !comparable(k));
        other.title = picked.indexOf(k) >= 0 ? "Shown \u2014 click to take it off"
                      : comparable(k) ? "Click to show it with the others"
                      : "A different target \u2014 clicking it shows this case on its own";
      });
      apply();
    });
    switcher.appendChild(button);
  });
  Array.prototype.forEach.call(switcher.children, function (other, k) {
    other.classList.toggle("othertarget", !comparable(k));
  }); }

  ["showMedian", "showOutline", "showDraws", "showVolumes", "showTarget", "showGround"]
    .forEach(function (id) { document.getElementById(id).addEventListener("change", apply); });
  clearance.addEventListener("input", apply);
  dogleg.addEventListener("input", apply);

  // ---- orbit ------------------------------------------------------------------------------------
  var yaw = 0.72, pitch = 0.42, distance = reach * 1.15;
  var dragging = false, panning = false, lastX = 0, lastY = 0;
  var target = focus.clone();
  var spin = document.getElementById("spin");

  function place() {
    var cp = Math.cos(pitch), sp = Math.sin(pitch);
    camera.position.set(target.x + distance * cp * Math.sin(yaw),
                        target.y + distance * sp,
                        target.z + distance * cp * Math.cos(yaw));
    camera.lookAt(target);
  }
  canvas.addEventListener("pointerdown", function (e) {
    dragging = true; panning = e.button === 2 || e.shiftKey;
    lastX = e.clientX; lastY = e.clientY;
    canvas.setPointerCapture(e.pointerId); spin.checked = false;
  });
  canvas.addEventListener("pointermove", function (e) {
    if (!dragging) { return; }
    var dx = e.clientX - lastX, dy = e.clientY - lastY;
    lastX = e.clientX; lastY = e.clientY;
    if (panning) {
      var right = new THREE.Vector3().setFromMatrixColumn(camera.matrix, 0);
      var up = new THREE.Vector3().setFromMatrixColumn(camera.matrix, 1);
      var scale = distance * 0.0016;
      target.addScaledVector(right, -dx * scale).addScaledVector(up, dy * scale);
    } else {
      yaw -= dx * 0.006;
      pitch = Math.max(-1.45, Math.min(1.45, pitch + dy * 0.006));
    }
    place();
  });
  canvas.addEventListener("pointerup", function (e) {
    dragging = false; canvas.releasePointerCapture(e.pointerId);
  });
  canvas.addEventListener("contextmenu", function (e) { e.preventDefault(); });
  canvas.addEventListener("wheel", function (e) {
    e.preventDefault();
    distance = Math.max(reach * 0.05, Math.min(reach * 4, distance * Math.exp(e.deltaY * 0.0012)));
    place();
  }, { passive: false });

  Array.prototype.forEach.call(document.querySelectorAll("button.view"), function (button) {
    button.addEventListener("click", function () {
      var which = button.getAttribute("data-view");
      spin.checked = false;
      if (which === "plan") { pitch = 1.44; yaw = 0; }
      else if (which === "north") { pitch = 0; yaw = Math.PI / 2; }
      else if (which === "east") { pitch = 0; yaw = 0; }
      else { pitch = 0.42; yaw = 0.72; }
      place();
    });
  });

  function resize() {
    var width = canvas.clientWidth, height = canvas.clientHeight;
    if (width === 0 || height === 0) { return; }
    renderer.setSize(width, height, false);
    camera.aspect = width / height;
    camera.updateProjectionMatrix();
  }
  window.addEventListener("resize", resize);

  function retint() {
    scene.background = colour("--scene");
    tinted.forEach(function (entry) { entry.material.color.set(css(entry.token)); });
  }
  retint();
  if (window.matchMedia) {
    var media = window.matchMedia("(prefers-color-scheme: dark)");
    if (media.addEventListener) { media.addEventListener("change", retint); }
  }
  new MutationObserver(retint).observe(document.documentElement,
                                       { attributes: true, attributeFilter: ["data-theme"] });

  apply();
  resize();
  place();
  (function frame() {
    if (spin.checked) { yaw += 0.0016; place(); }
    resize();
    renderer.render(scene, camera);
    requestAnimationFrame(frame);
  })();
})();
</script>
"""

# the scene the test wrote, taken from the temp directory unless one sits beside this file
import tempfile
beside = os.path.join(here, "ullrigg-factory-scene.json")
source = beside if os.path.exists(beside) else os.path.join(tempfile.gettempdir(),
                                                            "ullrigg-factory-scene.json")
scene = io.open(source, encoding="utf-8").read()
out = '<meta charset="utf-8">' + chr(10) + head + body.replace("/*SCENE_JSON*/", scene)
path = os.path.join(here, "ullrigg-factory-view.html")
io.open(path, "w", encoding="utf-8").write(out)
print("wrote", path, os.path.getsize(path) // 1024, "kB")
