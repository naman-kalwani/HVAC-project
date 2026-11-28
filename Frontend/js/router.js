// Simple hash‑based router for HVAC Connect frontend
// Exposes a single function `Router.loadCurrentPage()` that reads the URL hash,
// resolves the corresponding page module from `window.Pages`, and renders it
// into the #pageContent container. Unknown hashes fall back to the 404 page
// (if defined) or the dashboard.

window.Router = (function () {
  /** Render a page by its key.
   * @param {string} pageKey
   */
  function render(pageKey) {
    const container = document.getElementById('pageContent');
    if (!container) return;
    const pageFn = window.Pages[pageKey] || window.Pages['404'];
    if (typeof pageFn === 'function') {
      container.innerHTML = pageFn();
    } else {
      container.innerHTML = `<div class="error">Page not found.</div>`;
    }
  }

  /** Load the current hash (or default) and render the page. */
  function loadCurrentPage() {
    const hash = location.hash.replace(/^#/, '');
    const pageKey = hash || 'dashboard';
    render(pageKey);
  }

  // Listen for hash changes (navigation via links or App.navigate)
  window.addEventListener('hashchange', loadCurrentPage);

  // Public API
  return {
    loadCurrentPage,
    render,
  };
})();

