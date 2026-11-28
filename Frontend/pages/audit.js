/**
 * Page: System Audit Logs (PB-001 / US001)
 */
window.Pages = window.Pages || {};

window.Pages["audit"] = function() {
    const rows = Store.auditLogs.map(a => [
        `<b>${a.time}</b>`,
        a.actor,
        `<b>${a.action}</b>`,
        `<code>${a.entity}</code>`,
        a.scope,
        UI.badge(a.outcome),
        `<button class="btn small" onclick="App.toast('Full JSON snapshot: ${a.entity}')">Details</button>`
    ]);

    return `
        <article class="card">
            <div class="table-tools">
                <input class="filter-input" placeholder="Search audit trail by actor, action, entity or keyword..." data-filter>
                <select class="filter-input" onchange="App.filterTable(this.value)">
                    <option value="">All Vendors</option>
                    <option value="Pacific Climate">Pacific Climate</option>
                    <option value="Group-wide">Group-wide</option>
                </select>
                <button class="btn" onclick="App.toast('Exported CSV of audit log')">Export Audit Log (CSV)</button>
            </div>

            <div class="card-title">
                <div>
                    <h3>Immutable System & Security Audit Ledger</h3>
                    <p>Traceable history of all administrative actions, data migrations, and ticket status changes.</p>
                </div>
            </div>

            ${UI.table(["Timestamp", "Actor / Initiator", "Action Performed", "Target Entity", "Vendor Tenancy", "Outcome", "Payload"], rows)}

            <div class="list-row" style="margin-top: 14px; padding-top: 10px;">
                <small style="color:#64748b">Complies with enterprise M&A governance and SOC2 compliance rules</small>
                <small style="color:#64748b">Retention: 7 years permanent immutable archive</small>
            </div>
        </article>
    `;
};

