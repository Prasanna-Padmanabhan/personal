
// content.js

// The table is role="treegrid", rows are tr[role="row"] with aria-expanded.
const TREEGRID_SELECTOR = 'table[role="treegrid"].bolt-table';// The clickable expander is a <span class="bolt-tree-expand-button ms-Icon--ChevronRightMed">.
const COLLAPSED_ROW_SELECTOR = 'tr[role="row"][aria-expanded="false"].bolt-tree-row';
const EXPANDER_IN_ROW_SELECTOR = '.bolt-tree-cell .bolt-tree-expand-button, .bolt-tree-cell .ms-Icon--ChevronRightMed';

const LOG = (...a) => console.log('[ADO Auto-Expand]', ...a);

function visible(el) {
  if (!el) return false;
  const r = el.getBoundingClientRect();
  const s = getComputedStyle(el);
  return r.width > 0 && r.height > 0 && s.visibility !== 'hidden' && s.display !== 'none';
}

function clickExpander(el) {
  try {
    el.focus();
    el.click();
  } catch {
    el.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
  }
}

// Walk document and open shadow roots (in case parts of the grid live there)
function queryDeep(root, selector) {
  const out = [];
  const visit = node => {
    if (node.querySelectorAll) out.push(...node.querySelectorAll(selector));
    if (node.shadowRoot) visit(node.shadowRoot);
    let child = node.firstElementChild;
    while (child) { visit(child); child = child.nextElementSibling; }
  };
  visit(root);
  return out;
}

function expandOnce(root) {
  const grids = queryDeep(root, TREEGRID_SELECTOR);
  if (!grids.length) return 0;

  let expanded = 0;

  grids.forEach(grid => {
    // Find all collapsed rows at the current moment
    const rows = Array.from(queryDeep(grid, COLLAPSED_ROW_SELECTOR));
    rows.forEach(row => {
      // Find the expander icon in the first tree cell
      const expander = row.querySelector(EXPANDER_IN_ROW_SELECTOR);
      if (expander && visible(expander)) {
        clickExpander(expander);
        expanded++;
      }
    });
  });

  if (expanded) LOG(`Expanded ${expanded} row(s)`);
  return expanded;
}

function observeAndAutoExpand(root) {
  // Debounced re-run for dynamic loads/virtualization
  let pending;
  const schedule = (delay = 400) => {
    clearTimeout(pending);
    pending = setTimeout(() => expandOnce(root), delay);
  };

  const obs = new MutationObserver(muts => {
    // If grid/rows are added, schedule an expansion pass
    if (muts.some(m => m.addedNodes && m.addedNodes.length)) schedule(400);
  });

  obs.observe(root, { childList: true, subtree: true });

  // Safety net: periodic pass—handles auto-refresh & lazy row rendering
  const interval = setInterval(() => expandOnce(root), 6000);

  window.addEventListener('beforeunload', () => {
    obs.disconnect();
    clearInterval(interval);
  });

  // Initial pass (slightly delayed so first paint completes)
  setTimeout(() => expandOnce(root), 700);
}

(function init() {
  LOG('Initialized on', location.href);
  observeAndAutoExpand(document);
})();
