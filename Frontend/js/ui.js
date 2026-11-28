// UI Helper Library for HVAC Connect Frontend
// Provides lightweight functions to generate common UI components.
// All functions return HTML strings (or directly manipulate DOM) and are framework‑free.

window.UI = (function () {
  // Mapping of status/type to badge CSS classes
  const statusMap = {
    Open: "badge-primary",
    Assigned: "badge-info",
    "In Progress": "badge-warning",
    Resolved: "badge-success",
    Closed: "badge-secondary",
    "Within SLA": "badge-success",
    "At Risk": "badge-warning",
    Breached: "badge-danger",
  };

  /** Generate a KPI card grid.
   * @param {Array<Array>} items - Each item: [title, value, subtitle, icon]
   */
  function kpis(items) {
    const cards = items
      .map(
        ([title, value, subtitle, icon]) => `
        <div class="kpi-card">
          <div class="kpi-icon">${icon || ""}</div>
          <div class="kpi-value">${value}</div>
          <div class="kpi-title">${title}</div>
          <div class="kpi-subtitle">${subtitle}</div>
        </div>`
      )
      .join("");
    return `<div class="kpi-grid">${cards}</div>`;
  }

  function table(headings, rows, tableId) {
    const idAttr = tableId ? ` id="${tableId}"` : "";
    const head = headings.map(h => `<th>${h}</th>`).join("");
    const body = rows
      .map(r => `<tr>${r.map(c => `<td>${c}</td>`).join("")}</tr>`)
      .join("");
    return `<table class="ui-table"${idAttr}><thead><tr>${head}</tr></thead><tbody>${body}</tbody></table>`;
  }

  function pagination(pageInfo, paginationId) {
    const startItem = pageInfo.totalItems === 0 ? 0 : pageInfo.startItem;
    return `
      <nav class="pagination" aria-label="Pagination">
        <button class="btn small" type="button" data-pagination-id="${paginationId}" data-pagination-action="prev"
          ${pageInfo.currentPage <= 1 ? "disabled" : ""}>Previous</button>
        <span aria-live="polite">
          Page ${pageInfo.currentPage} of ${pageInfo.totalPages}
          (${startItem}-${pageInfo.endItem} of ${pageInfo.totalItems})
        </span>
        <button class="btn small" type="button" data-pagination-id="${paginationId}" data-pagination-action="next"
          ${pageInfo.currentPage >= pageInfo.totalPages ? "disabled" : ""}>Next</button>
      </nav>`;
  }

  function badge(text, type) {
    const cls = type && statusMap[type] ? statusMap[type] : "badge-default";
    return `<span class="badge ${cls}">${text}</span>`;
  }

  function progress(percent) {
    const safe = Math.min(100, Math.max(0, parseInt(percent, 10) || 0));
    return `<div class="progress-bar"><div class="progress-fill" style="width:${safe}%"></div></div>`;
  }

  function toast(message, duration = 3000) {
    const container = document.getElementById("toastContainer");
    if (!container) return;
    const el = document.createElement("div");
    el.className = "toast";
    el.textContent = message;
    container.appendChild(el);
    setTimeout(() => el.remove(), duration);
  }

  function modal(title, bodyHtml, onClose) {
    const container = document.getElementById("modalContainer");
    if (!container) return;
    container.innerHTML = `
      <div class="modal-overlay"></div>
      <div class="modal">
        <div class="modal-header"><h3>${title}</h3><button class="modal-close" aria-label="Close">&times;</button></div>
        <div class="modal-body">${bodyHtml}</div>
      </div>`;
    container.style.display = "block";
    const closeBtn = container.querySelector('.modal-close');
    closeBtn.addEventListener('click', () => {
      container.style.display = 'none';
      if (onClose) onClose();
    });
  }

  return { kpis, table, pagination, badge, progress, toast, modal, statusMap };
})();

