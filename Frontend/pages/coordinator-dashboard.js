/**
 * Page: Service Coordinator Dashboard (PB-005 / US005)
 */
window.Pages = window.Pages || {};

window.Pages["coordinator-dashboard"] = function() {
    const openTickets = Store.tickets.filter(t => t.status === "Open" || t.status === "Assigned");
    const inProgress = Store.tickets.filter(t => t.status === "In Progress");
    const atRisk = Store.tickets.filter(t => t.slaState === "At Risk");
    const breached = Store.tickets.filter(t => t.slaState === "Breached");

    const kpiItems = [
        ["Priority Queue", openTickets.length, "Awaiting dispatch / response", "▤"],
        ["SLA At Risk", atRisk.length, "Expiring within 2-4 hours", "◷"],
        ["SLA Breached", breached.length, "Requires immediate escalation", "!"],
        ["In Field Execution", inProgress.length, "Technicians currently on site", "⚒"]
    ];

    const queueRows = Store.tickets
        .filter(t => t.status !== "Closed")
        .map(t => [
            `<b>${t.id}</b>`,
            `<b>${t.customerName}</b><br><small style="color:#64748b">${t.locationName}</small>`,
            `<span title="${t.description}">${t.title}</span>`,
            UI.badge(t.priority),
            UI.badge(t.slaState),
            `<small style="font-weight:700">${t.slaDueText}</small>`,
            UI.badge(t.status),
            `<b>${t.technicianName}</b>`
        ]);

    const followUps = [
        { title: "Escalate Breached Ticket TKT-2408", desc: "Pacific Climate Services response overdue by 2h 15m on AHU-00182", tag: "Urgent", badgeType: "red" },
        { title: "Confirm Technician Arrival for TKT-2410", desc: "Critical refrigeration rack alarm at Oakland store #14. Due in 1h 15m", tag: "Critical", badgeType: "red" },
        { title: "Follow-up on Condensate Leak TKT-2409", desc: "Jordan Patel dispatched to Bellevue Medical Plaza (At Risk)", tag: "Due Soon", badgeType: "amber" },
        { title: "Verify Resolution Notes on TKT-2407", desc: "Morgan Diaz submitted closure report for Daikin RTU vibration", tag: "Review", badgeType: "blue" }
    ];

    return `
        <div class="callout warning" style="margin-bottom: 20px;">
            <strong>Dispatch Watchdog Active:</strong> Real-time SLA monitors are evaluating incoming calls and technician assignments. Tickets approaching breach require rapid escalation.
        </div>

        ${UI.kpis(kpiItems)}

        <div style="margin-top: 20px;">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Active Dispatch & Service Queue</h3>
                        <p>Real-time list of all commercial tickets governed by contractual SLAs.</p>
                    </div>
                    <a href="#" onclick="App.navigate('tickets'); return false;">All Tickets</a>
                </div>
                ${UI.table(["ID", "Customer & Site", "Issue Summary", "Priority", "SLA", "Time Remaining", "Status", "Technician"], queueRows)}
            </article>
        </div>

        <div class="grid two-col" style="margin-top: 20px;">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Coordinator Escalation Follow-ups</h3>
                        <p>High-priority operational actions requiring dispatcher intervention.</p>
                    </div>
                </div>
                <div class="list">
                    ${followUps.map(f => `
                        <div class="list-row">
                            <div>
                                <b>${f.title}</b>
                                <small>${f.desc}</small>
                            </div>
                            <span class="badge ${f.badgeType}">${f.tag}</span>
                        </div>
                    `).join("")}
                </div>
            </article>

            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Technician Availability Status</h3>
                        <p>Field specialist current capacity and assignment count.</p>
                    </div>
                    <a href="#" onclick="App.navigate('technicians'); return false;">View all</a>
                </div>
                <div class="list">
                    ${Store.technicians.map(t => `
                        <div class="list-row">
                            <div>
                                <b>${t.name} (${t.id})</b>
                                <small>${t.vendorName} · ${t.specialization}</small>
                            </div>
                            <div>
                                <span class="badge ${t.activeTickets >= 3 ? 'amber' : 'green'}">${t.activeTickets} Active Jobs</span>
                            </div>
                        </div>
                    `).join("")}
                </div>
            </article>
        </div>
    `;
};
