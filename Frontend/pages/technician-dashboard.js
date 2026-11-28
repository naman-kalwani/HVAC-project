/**
 * Page: Technician Mobile Dashboard (PB-009 / US009)
 */
window.Pages = window.Pages || {};


window.Pages["technician-dashboard"] = function() {
    const user = Auth.getCurrentUser();
    const techName = user && user.roleKey === "technician" ? user.name : "Casey Reed";
    const techId = user && user.employeeId ? user.employeeId : "TECH-104";
    const spec = user && user.specialization ? user.specialization : "Commercial HVAC & Refrigeration Specialist";


    const myTickets = Store.tickets.filter(t => t.technicianId === "TECH-104" || t.technicianName.includes(techName.split(" ")[0]));
    const inProgress = myTickets.filter(t => t.status === "In Progress");
    const assigned = myTickets.filter(t => t.status === "Assigned");
    const completed = myTickets.filter(t => t.status === "Resolved" || t.status === "Closed");


    const kpiItems = [
        ["My Assigned Jobs", myTickets.length, "Total assigned work", "▤"],
        ["Currently In Progress", inProgress.length, "Active field tasks", "⚒"],
        ["Due Today", myTickets.filter(t => t.slaState === "At Risk" || t.slaState === "Breached").length, "High priority jobs", "!"],
        ["Completed Recently", completed.length, "Resolved tickets", "✓"]
    ];


    return `
        <article class="card" style="margin-bottom: 20px;">
            <div class="profile-strip" style="border-bottom: none; margin-bottom: 0; padding-bottom: 0;">
                <div class="profile-avatar">${techName.split(" ").map(n=>n[0]).join("")}</div>
                <div>
                    <h2>${techName}</h2>
                    <p>${spec} · Pacific Climate Services</p>
                    <small>Employee ID: <b>${techId}</b> · Assigned Territory: San Jose & Oakland Bay Area</small>
                </div>
            </div>
        </article>


        ${UI.kpis(kpiItems)}


        <div style="margin-top: 20px;">
            <div class="card-title">
                <div>
                    <h3>My Assigned Field Service Tickets</h3>
                    <p>Select a job to view building access instructions, post diagnostic updates, or log repairs.</p>
                </div>
            </div>


            <div style="display: flex; flex-direction: column; gap: 14px;">
                ${myTickets.map(t => `
                    <article class="card" style="border-left: 5px solid ${t.slaState === 'Breached' ? '#ef4444' : t.slaState === 'At Risk' ? '#f59e0b' : '#3b82f6'};">
                        <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 10px;">
                            <div>
                                <span style="font-size: 11px; font-weight: 700; color: #64748b;">${t.id} · ${t.locationName}</span>
                                <h3 style="font-size: 17px; margin-top: 2px;">${t.title}</h3>
                            </div>
                            <div style="display: flex; gap: 6px;">
                                ${UI.badge(t.priority)}
                                ${UI.badge(t.slaState)}
                                ${UI.badge(t.status)}
                            </div>
                        </div>


                        <p style="font-size: 13px; color: #475569; margin-bottom: 12px; line-height: 1.5;">
                            <b>Reported Issue:</b> ${t.description}
                        </p>


                        <div class="info-grid" style="margin-bottom: 14px; background: #f8fafc;">
                            <div class="info-item">
                                <small>Equipment Unit</small>
                                <strong>${t.assetName} (${t.assetId})</strong>
                            </div>
                            <div class="info-item">
                                <small>Target SLA</small>
                                <strong>${t.slaDueText}</strong>
                            </div>
                            <div class="info-item">
                                <small>Reported By</small>
                                <strong>${t.raisedBy}</strong>
                            </div>
                        </div>


                        <div style="display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #e2e8f0; padding-top: 12px;">
                            <div style="font-size: 12px; color: #64748b;">
                                Created: ${t.createdAt}
                            </div>
                            <div style="display: flex; gap: 8px;">
                                <button class="btn" onclick="App.openTicketDetail('${t.id}')">View Details & Comments</button>
                                ${t.status === 'Assigned' ? `
                                    <button class="btn primary" onclick="Store.updateTicketStatus('${t.id}', 'In Progress', 'Technician ${techName} has started work on-site.'); App.showToast('Status updated to In Progress', 'success'); Router.loadCurrentPage();">Start Work</button>
                                ` : ''}
                                ${t.status === 'In Progress' ? `
                                    <button class="btn primary" onclick="App.openResolveModal('${t.id}')">Submit Resolution</button>
                                ` : ''}
                            </div>
                        </div>
                    </article>
                `).join("")}
            </div>
        </div>


        <div style="margin-top: 25px;">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Assigned Customer Site Access Details</h3>
                        <p>Access codes, rooftop entrance guidelines, and security check-ins.</p>
                    </div>
                </div>
                <div class="list">
                    <div class="list-row">
                        <div>
                            <b>Apex Retail — San Jose HQ</b>
                            <small>450 Market Street · Check in at lobby desk for service elevator badge to roof plant.</small>
                        </div>


                    </div>
                    <div class="list-row">
                        <div>
                            <b>Apex Retail — Oakland Store #14</b>
                            <small>120 Broadway · Roof hatch through stockroom B. Keylock combination: 4821.</small>
                        </div>


                    </div>
                </div>
            </article>
        </div>
    `;
};