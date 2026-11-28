/**
 * Page: End User / Facility Manager Dashboard
 */
window.Pages = window.Pages || {};

window.Pages["enduser-dashboard"] = function() {
    const user = Auth.getCurrentUser();
    const customerName = user && user.roleKey === "enduser" ? user.workspace : "Apex Retail Group";

    const mySites = Store.locations.filter(l => l.customerName.includes("Apex"));
    const myAssets = Store.assets.filter(a => a.locationName.includes("Apex"));
    const myTickets = Store.tickets.filter(t => t.customerName.includes("Apex"));
    const activeRequests = myTickets.filter(t => t.status !== "Closed");

    const kpiItems = [
        ["Registered Equipment", myAssets.length, "Across your facilities", "▣"],
        ["Active Service Requests", activeRequests.length, "Being serviced by vendor", "▤"],
        ["Maintained Locations", mySites.length, "San Jose & Oakland", "⌖"],
        ["Completed Repairs", myTickets.filter(t => t.status === "Closed" || t.status === "Resolved").length, "Historical jobs", "✓"]
    ];

    return `
        <div class="callout info" style="margin-bottom: 20px;">
            <strong>Customer Facility Portal:</strong> Welcome to the client hub for <b>${customerName}</b>. Monitor your registered HVAC equipment, submit service requests, and track real-time vendor response.
        </div>

        ${UI.kpis(kpiItems)}

        <div class="grid two-col" style="margin-top: 20px;">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Recent Service Requests</h3>
                        <p>Track the progress of tickets logged for your equipment.</p>
                    </div>
                    <a href="#" onclick="App.navigate('tickets'); return false;">All Tickets</a>
                </div>
                ${UI.table(
                    ["Ticket ID", "Equipment Unit", "Reported Issue", "Status", "Vendor"],
                    myTickets.map(t => [
                        `<b>${t.id}</b>`,
                        t.assetName,
                        t.title,
                        UI.badge(t.status),
                        t.vendorName,
                    ])
                )}
            </article>

            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>My Maintained Equipment</h3>
                        <p>HVAC and commercial refrigeration units at your sites.</p>
                    </div>
                    <a href="#" onclick="App.navigate('assets'); return false;">All assets</a>
                </div>
                <div class="list">
                    ${myAssets.slice(0, 4).map(a => `
                        <div class="list-row">
                            <div>
                                <b>${a.id} · ${a.category}</b>
                                <small>${a.manufacturer} ${a.model} (${a.locationName})</small>
                            </div>
                            ${UI.badge(a.status)}
                        </div>
                    `).join("")}
                </div>
            </article>
        </div>

        <div class="grid two-col" style="margin-top: 20px;">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Facility Locations</h3>
                        <p>Sites serviced under current maintenance contracts.</p>
                    </div>
                    <a href="#" onclick="App.navigate('locations'); return false;">All Locations</a>
                </div>
                <div class="list">
                    ${mySites.map(s => `
                        <div class="list-row">
                            <div>
                                <b>${s.name}</b>
                                <small>${s.address} · Serviced by ${s.vendorName}</small>
                            </div>
                            <span class="badge green">${s.assetCount} Units</span>
                        </div>
                    `).join("")}
                </div>
            </article>

            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Need Urgent Support?</h3>
                        <p>Emergency breakdown protocol for refrigeration and critical chillers.</p>
                    </div>
                </div>
                <div style="font-size: 13px; line-height: 1.6; color: #475569;">
                    <p style="margin-bottom: 12px;">
                        For critical temperature failures (walk-in coolers, freezer racks, or hospital chillers), submit a <b>Critical Priority</b> ticket to trigger our 2-hour response SLA watchdog.
                    </p>
                    <button class="btn primary" onclick="App.openCreateTicketModal()">Submit Emergency Request</button>
                </div>
            </article>
        </div>
    `;
};

