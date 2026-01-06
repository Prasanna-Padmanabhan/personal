// content.js

// Grid & row selectors based on your snippet
const TREEGRID_SELECTOR = 'table[role="treegrid"].bolt-table';
const COLLAPSED_ROW_SELECTOR = 'tr[role="row"][aria-expanded="false"].bolt-tree-row';
const EXPANDER_IN_ROW_SELECTOR = '.bolt-tree-cell .bolt-tree-expand-button, .bolt-tree-cell .ms-Icon--ChevronRightMed';

const LOG = (...a) => console.log('[ADO Auto-Expand]', ...a);

// Utilities
function visible(el) {
  if (!el) return false;
  const r = el.getBoundingClientRect();
  const s = getComputedStyle(el);
  return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none';
}

// Query across document + open shadow roots (if any)
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

// Focus grid and row (helps first row accept synthetic clicks)
function ensureFocus(grid, row) {
  try {
    if (grid && document.activeElement !== grid) grid.focus();
    if (row && document.activeElement !== row) row.focus();
  } catch {}
}

function clickExpander(el) {
  try {
    el.focus();
    // Use pointer events first; some Fabric controls expect them
    el.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true }));
    el.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
    el.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    el.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }));
    el.dispatchEvent(new PointerEvent('pointerup', { bubbles: true }));
  } catch {
    // Fallback to simple click
    el.click();
  }
}

function getCollapsedRowsIn(grid) {
  return Array.from(queryDeep(grid, COLLAPSED_ROW_SELECTOR));
}

// Expand pass: tries to expand all currently-collapsed rows
function expandPass(root) {
  const grids = queryDeep(root, TREEGRID_SELECTOR);
  if (!grids.length) return 0;

  let expanded = 0;

  grids.forEach(grid => {
    const rows = getCollapsedRowsIn(grid);
    // Sort so first visible row is expanded earliest (handles initial focus quirk)
    rows.sort((a, b) => (a.getBoundingClientRect().top - b.getBoundingClientRect().top));

    rows.forEach((row, idx) => {
      const expander = row.querySelector(EXPANDER_IN_ROW_SELECTOR);
      if (expander && visible(expander)) {
        // Explicitly focus grid & current row to satisfy first-row interaction
        ensureFocus(grid, row);
        clickExpander(expander);
        expanded++;
      }
    });
  });

  if (expanded) LOG(`Expanded ${expanded} row(s)`);
  return expanded;
}

function observeAndAutoExpand(root) {
  let pending = null;
  let interval = null;

  const schedule = (delay = 400) => {
    clearTimeout(pending);
    pending = setTimeout(() => {
      const count = expandPass(root);
      // If we expanded nothing, and there are no collapsed rows left, stop the interval
      if (count === 0) {
        const anyCollapsed =
          queryDeep(root, TREEGRID_SELECTOR).some(grid => getCollapsedRowsIn(grid).length > 0);
        if (!anyCollapsed && interval) {
          clearInterval(interval);
          interval = null;
          LOG('No collapsed rows left; stopped periodic expansion.');
        }
      }
    }, delay);
  };

  const obs = new MutationObserver(muts => {
    // Only run when something actually changed
    if (muts.some(m => m.addedNodes && m.addedNodes.length > 0)) schedule(350);
  });
  obs.observe(root, { childList: true, subtree: true });

  // Start a light safety net that we will *stop* once all rows are expanded
  interval = setInterval(() => schedule(0), 6000);

  window.addEventListener('beforeunload', () => {
    obs.disconnect();
    if (interval) clearInterval(interval);
  });

  // Initial pass after first paint
  setTimeout(() => schedule(0), 700);
}

(function init() {
  LOG('Initialized on', location.href);
  observeAndAutoExpand(document);
})();
