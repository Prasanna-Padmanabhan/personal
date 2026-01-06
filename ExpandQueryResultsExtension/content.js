// content.js

// Based on your DOM:
const TREEGRID_SELECTOR = 'table[role="treegrid"].bolt-table';
const COLLAPSED_ROW_SELECTOR = 'tr[role="row"][aria-expanded="false"].bolt-tree-row';

// Logging
const LOG = (...a) => console.log('[ADO Auto-Expand]', ...a);

// Deep query (document + open shadow roots)
function queryDeep(root, selector) {
  const out = [];
  const visit = node => {
    if (node.querySelectorAll) out.push(...node.querySelectorAll(selector));
    if (node.shadowRoot) visit(node.shadowRoot);
    for (let child = node.firstElementChild; child; child = child.nextElementSibling) visit(child);
  };
  visit(root);
  return out;
}

function ensureFocus(grid, row) {
  try {
    if (grid && document.activeElement !== grid) grid.focus();
    if (row && document.activeElement !== row) row.focus();
  } catch {}
}

function sendKey(el, key, code, keyCode) {
  const opts = { key, code, keyCode, which: keyCode, bubbles: true, cancelable: true };
  el.dispatchEvent(new KeyboardEvent('keydown', opts));
  el.dispatchEvent(new KeyboardEvent('keyup', opts));
}

// Expand one row via keyboard (ArrowRight)
function expandRowViaKeyboard(grid, row) {
  ensureFocus(grid, row);
  // A short tick helps some builds after focus
  setTimeout(() => sendKey(row, 'ArrowRight', 'ArrowRight', 39), 0);
}

function getCollapsedRows(grid) {
  return Array.from(queryDeep(grid, COLLAPSED_ROW_SELECTOR));
}

function expandPass(root) {
  const grids = queryDeep(root, TREEGRID_SELECTOR);
  if (!grids.length) {
    LOG('No treegrid found in this frame.');
    return 0;
  }

  let expanded = 0;

  grids.forEach(grid => {
    const rows = getCollapsedRows(grid)
      .sort((a, b) => a.getBoundingClientRect().top - b.getBoundingClientRect().top);
    rows.forEach(row => {
      expandRowViaKeyboard(grid, row);
      expanded++;
    });
  });

  if (expanded) LOG(`Expanded ${expanded} row(s) via keyboard`);
  return expanded;
}

function observeAndAutoExpand(root) {
  let pending = null;
  let interval = null;

  const schedule = (delay = 300) => {
    clearTimeout(pending);
    pending = setTimeout(() => {
      const count = expandPass(root);
      // Stop periodic checks once fully expanded
      if (count === 0) {
        const anyCollapsed = queryDeep(root, TREEGRID_SELECTOR)
          .some(g => getCollapsedRows(g).length > 0);
        if (!anyCollapsed && interval) {
          clearInterval(interval);
          interval = null;
          LOG('No collapsed rows left; stopped periodic expansion.');
        }
      }
    }, delay);
  };

  const obs = new MutationObserver(muts => {
    if (muts.some(m => m.addedNodes && m.addedNodes.length)) schedule(250);
  });
  obs.observe(root, { childList: true, subtree: true });

  // Safety net (will stop once expanded)
  interval = setInterval(() => schedule(0), 6000);

  window.addEventListener('beforeunload', () => {
    obs.disconnect();
    if (interval) clearInterval(interval);
  });

  // First pass, after initial paint
  schedule(700);
}

(function init() {
  LOG('Initialized on', location.href);
  observeAndAutoExpand(document);
})();
