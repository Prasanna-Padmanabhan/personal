// content.js

// Based on your DOM:
const TREEGRID_SELECTOR = 'table[role="treegrid"].bolt-table';
const COLLAPSED_ROW_SELECTOR = 'tr[role="row"][aria-expanded="false"].bolt-tree-row';

// Timing constants
const DEFAULT_EXPANSION_DELAY_MS = 300;
const MUTATION_SCHEDULE_DELAY_MS = 250;
const INITIAL_EXPANSION_DELAY_MS = 700;
const PERIODIC_CHECK_INTERVAL_MS = 6000;

// Limits to prevent excessive DOM traversal
const MAX_QUERY_DEPTH = 50;
const MAX_QUERY_NODES = 5000;

// Logging
const LOG = (...args) => console.log('[ADO Auto-Expand]', ...args);

/**
 * Recursively queries both the regular DOM and any open shadow DOM trees
 * starting from the given root, returning all elements that match the selector.
 *
 * @param {Document | Element | ShadowRoot} root - The root node from which to begin the deep search.
 * @param {string} selector - A CSS selector used to match elements within the DOM and shadow DOM.
 * @returns {Element[]} An array of elements that match the provided selector.
 */
function querySelectorDeepWithShadow(root, selector) {
  const out = [];
  let visitedCount = 0;

  const visit = (node, depth) => {
    if (!node) return;
    if (depth > MAX_QUERY_DEPTH) return;
    if (visitedCount >= MAX_QUERY_NODES) return;

    visitedCount++;

    if (node.querySelectorAll) {
      out.push(...node.querySelectorAll(selector));
    }

    if (node.shadowRoot) {
      visit(node.shadowRoot, depth + 1);
    }

    for (let child = node.firstElementChild; child; child = child.nextElementSibling) {
      visit(child, depth + 1);
    }
  };

  visit(root, 0);
  return out;
}

function ensureFocus(grid, row) {
  try {
    if (grid && document.activeElement !== grid) grid.focus();
    if (row && document.activeElement !== row) row.focus();
  } catch (e) {
    LOG('Failed to ensure focus:', e);
  }
}

function sendKey(el, key, code, keyCode) {
  // NOTE: Azure DevOps' tree grid keyboard handling still relies on the legacy
  // `keyCode` / `which` properties for navigation. Although these properties
  // are deprecated in modern web standards and `key` / `code` are also set,
  // we intentionally include `keyCode` and `which` here to preserve behavior
  // in Azure DevOps. Remove these only once Azure DevOps no longer depends
  // on them.
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
  return Array.from(querySelectorDeepWithShadow(grid, COLLAPSED_ROW_SELECTOR));
}

function expandPass(root) {
  const grids = querySelectorDeepWithShadow(root, TREEGRID_SELECTOR);
  if (!grids.length) {
    LOG('No treegrid found in this frame.');
    return 0;
  }

  let expanded = 0;

  grids.forEach(grid => {
    const rows = getCollapsedRows(grid);
    // Cache positions to avoid repeated layout recalculations
    const rowsWithPos = rows
      .map(row => ({ row, top: row.getBoundingClientRect().top }))
      .sort((a, b) => a.top - b.top);
    rowsWithPos.forEach(({ row }) => {
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

  const schedule = (delay = DEFAULT_EXPANSION_DELAY_MS) => {
    clearTimeout(pending);
    pending = setTimeout(() => {
      const count = expandPass(root);
      // Stop periodic checks once fully expanded
      if (count === 0) {
        const anyCollapsed = querySelectorDeepWithShadow(root, TREEGRID_SELECTOR)
          .some(g => getCollapsedRows(g).length > 0);
        if (!anyCollapsed && interval) {
          clearInterval(interval);
          interval = null;
          LOG('No collapsed rows left; stopped periodic expansion.');
        }
      }
    }, delay);
  };

  const mutationObserver = new MutationObserver(mutations => {
    if (mutations.some(m => m.addedNodes && m.addedNodes.length)) schedule(MUTATION_SCHEDULE_DELAY_MS);
  });

  // Observe root with subtree to detect dynamically added grids anywhere in the document
  mutationObserver.observe(root, { childList: true, subtree: true });

  // Safety net (will stop once expanded)
  interval = setInterval(() => schedule(0), PERIODIC_CHECK_INTERVAL_MS);

  window.addEventListener('beforeunload', () => {
    mutationObserver.disconnect();
    if (interval) clearInterval(interval);
  });

  // First pass, after initial paint
  schedule(INITIAL_EXPANSION_DELAY_MS);
}

(function init() {
  LOG('Initialized on', location.href);
  observeAndAutoExpand(document);
})();
