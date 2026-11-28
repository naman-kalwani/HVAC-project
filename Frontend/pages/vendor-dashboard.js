/**
 * Page: Vendor Management Dashboard (PB-004 / US004)
 */
window.Pages = window.Pages || {};

window.Pages["vendor-dashboard"] = function() {
    const user = Auth.getCurrentUser();
    const vendorName = user && user.vendorId ? "Pacific Climate Services" : "Pacific Climate Services";

    const openTickets = Store.tickets.filter(t => t.status === "Open" || t.status === "In Progress" || t.status === "Assigned");
    const breached = Store.tickets.filter(t => t.slaState === "Breached");
    const atRisk = Store.tickets.filter(t => t.slaState === "At Risk");
    const completed = Store.tickets.filter(t => t.status === "Resolved" || t.status === "Closed");

    const kpiItems = [
        ["Active Tickets", openTickets.length, `Under ${vendorName}`, "▤"],
        ["SLA Breaches", breached.length, "Immediate dispatch required", "!"],
        ["Tickets At Risk", atRisk.length, "Due within 2-4 hours", "◷"],
        ["Resolved / Closed", completed.length, "Current billing cycle", "✓"]
    ];

    const urgentTicketRows = Store.tickets
        .filter(t => t.slaState === "Breached" || t.slaState === "At Risk" || t.priority === "Critical")
        .map(t => [
            `<b>${t.id}</b>`,
            t.locationName,
            UI.badge(t.priority),
            UI.badge(t.slaState),
            UI.badge(t.status),
            t.technicianName,
        ]);

    const technicianRows = Store.technicians
        .filter(tech => tech.vendorId === "PCS-001")
        .map(tech => [
            `<b>${tech.name}</b><br><small style="color:#64748b">${tech.id}</small>`,
            tech.specialization,
            `${tech.activeTickets} tickets`,
            UI.badge(tech.status),
            `<button class="btn small" onclick="App.navigate('technicians')">Roster</button>`
        ]);

    return `
        <div class="callout info" style="margin-bottom: 20px;">
            <strong>Vendor Scoped Workspace:</strong> You are managing operational service delivery, local technicians, and customer sites for <b>${vendorName}</b>.
        </div>
        ${UI.kpis(kpiItems)}

        <div class="grid two-col" style="margin-top: 20px;">
            <article class="card" style="grid-column: 1 / span 2;">
                <div class="card-title">
                    <div>
                        <h3>Service Tickets Requiring Attention</h3>
                        <p>At-risk and breached tickets for your service locations.</p>
                    </div>
                    <a href="#" onclick="App.navigate('tickets'); return false;">All tickets</a>
                </div>
                ${UI.table(["Ticket ID", "Customer Site", "Priority", "SLA State", "Status", "Technician"], urgentTicketRows)}
            </article>

        </div>

        <div class="grid two-col" style="margin-top: 20px;">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Recent Customer Sites Serviced</h3>
                        <p>Commercial facilities maintained by ${vendorName}.</p>
                    </div>
                    <a href="#" onclick="App.navigate('customers'); return false;">View Customers</a>
                </div>
                <div class="list">
                    ${Store.locations.filter(l => l.vendorId === 'PCS-001').map(l => `
                        <div class="list-row">
                            <div>
                                <b>${l.name}</b>
                                <small>${l.address} · ${l.assetCount} registered units</small>
                            </div>
                            ${UI.badge(l.status)}
                        </div>
                    `).join("")}
                </div>
            </article>

           
        </div>
    `;
};

