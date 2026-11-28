/**
 * Page: Super Admin Group Dashboard (PB-003 / US003)
 */
window.Pages = window.Pages || {};

window.Pages["dashboard"] = function() {
    const kpiItems = [
        ["Acquired Vendors", Store.vendors.length, "Across platform portfolio", "▤"],
        ["Canonical Customers", Store.customers.length, "Unified group identities", "♧"],
        ["Customer Locations", Store.locations.length, "Active commercial sites", "⌖"],
        ["Registered Assets", Store.assets.length, "Commercial HVAC units", "▣"]
    ];

    const vendorRows = Store.vendors.map(c => [
        `<b>${c.name}</b><br><small style="color:#64748b">${c.id}</small>`,
        c.region,
        UI.badge(c.status),
        c.phase,
        UI.progress(c.progress)
    ]);

    const qualitySummary = [
        ["Missing Serial Numbers", "1 unit pending field audit", "amber"],
        ["Duplicate Candidates", `${Store.duplicateCandidates.filter(d=>d.status==='Pending Review').length} awaiting review`, "blue"],
        ["Expiring Agreements", "2 contracts expiring in 60 days", "amber"],
        ["Active SLA Breaches", `${Store.tickets.filter(t=>t.slaState==='Breached').length} critical ticket breached`, "red"]
    ];

    const recentActivity = Store.auditLogs.slice(0, 4);

    return `
        ${UI.kpis(kpiItems)}

        <div class="grid two-col" style="margin-top: 20px;">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Acquisition Vendor Portfolio</h3>
                        <p>Onboarding progress and operational status by regional vendor.</p>
                    </div>
                    <a href="#" onclick="App.navigate('vendors'); return false;">View all</a>
                </div>
                ${UI.table(["Vendor", "Region", "Status", "Onboarding Phase", "Readiness"], vendorRows)}
            </article>

            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Data Quality & Risk Snapshot</h3>
                        <p>Key integrity issues requiring administrative review.</p>
                    </div>
                    <a href="#" onclick="App.navigate('duplicates'); return false;">Review queue</a>
                </div>
                <div class="list">
                    ${qualitySummary.map(q => `
                        <div class="list-row">
                            <div>
                                <b>${q[0]}</b>
                                <small>${q[1]}</small>
                            </div>
                            ${UI.badge(q[2] === 'red' ? 'Critical' : q[2] === 'amber' ? 'Attention' : 'Pending')}
                        </div>
                    `).join("")}
                </div>
            </article>
        </div>

        <div class="grid two-col" style="margin-top: 20px;">
            

            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Two-Phase Onboarding Milestones</h3>
                        <p>Phase 1 (Coexist & Import) vs Phase 2 (Cutover)</p>
                    </div>
                    <a href="#" onclick="App.navigate('onboarding'); return false;">Checklists</a>
                </div>
                <div class="callout info" style="margin-bottom: 12px;">
                    <strong>Phase 1 Strategy Active:</strong> Vendors continue daily work in local systems while customer/asset records are unified in platform.
                </div>
                <div class="list">
                    <div class="list-row">
                        <div>
                            <b>Phase 1 — Coexist & Import</b>
                            <small>Pacific Climate Services, Northwest Cooling</small>
                        </div>
                        ${UI.badge('Active Coexistence')}
                    </div>
                    <div class="list-row">
                        <div>
                            <b>Phase 2 — Transition Planning</b>
                            <small>Desert Air Systems (91% ready)</small>
                        </div>
                        ${UI.badge('In Progress')}
                    </div>
                    <div class="list-row">
                        <div>
                            <b>Discovery & Assessment</b>
                            <small>Coastal Mechanical</small>
                        </div>
                        ${UI.badge('Pending')}
                    </div>
                </div>
            </article>
        </div>
    `;
};
