/**
 * Page: Ticket Detail & Technician Comment Thread (PB-007, PB-009)
 */
window.Pages = window.Pages || {};

window.Pages["ticket-detail"] = function(ticketId = "TKT-2408") {
    const ticket = Store.tickets.find(t => t.id === ticketId) || Store.tickets[0];
    const comments = Store.ticketComments.filter(c => c.ticketId === ticket.id);

    const statuses = ["Open", "Assigned", "In Progress", "Resolved", "Closed"];
    const currentIndex = statuses.indexOf(ticket.status);

    return `
        <div style="margin-bottom: 16px;">
            <a href="#" onclick="App.navigate('tickets'); return false;" style="font-size: 13px; color: var(--text-muted); text-decoration: none;">
                ← Back to Ticket Register
            </a>
        </div>

        <!-- LIFECYCLE STEPPER -->
        <div class="stepper" style="margin-bottom: 20px;">
            ${statuses.map((s, idx) => `
                <div class="step ${idx < currentIndex ? 'done' : idx === currentIndex ? 'active' : ''}">
                    ${idx < currentIndex ? '✓ ' : ''}${idx + 1} · ${s}
                </div>
            `).join("")}
        </div>

        <div class="grid two-col">
            <!-- LEFT COLUMN: TICKET DETAILS -->
            <article class="card">
                <div class="card-title">
                    <div>
                        <span style="font-size: 12px; font-weight: 700; color: #64748b;">${ticket.id} · ${ticket.vendorName}</span>
                        <h2 style="font-size: 20px; margin-top: 4px;">${ticket.title}</h2>
                    </div>
                    <div style="display: flex; gap: 6px;">
                        ${UI.badge(ticket.priority)}
                        ${UI.badge(ticket.slaState)}
                        ${UI.badge(ticket.status)}
                    </div>
                </div>

                <div class="info-grid">
                    <div class="info-item">
                        <small>Customer Enterprise</small>
                        <strong>${ticket.customerName}</strong>
                    </div>
                    <div class="info-item">
                        <small>Facility Location</small>
                        <strong>${ticket.locationName}</strong>
                    </div>
                    <div class="info-item">
                        <small>HVAC Equipment Unit</small>
                        <strong>${ticket.assetName}</strong>
                    </div>
                    <div class="info-item">
                        <small>Assigned Technician</small>
                        <strong>${ticket.technicianName}</strong>
                    </div>
                    <div class="info-item">
                        <small>SLA Target Countdown</small>
                        <strong style="color: ${ticket.slaState === 'Breached' ? '#ef4444' : ticket.slaState === 'At Risk' ? '#f59e0b' : '#15803d'}">
                            ${ticket.slaDueText}
                        </strong>
                    </div>
                    <div class="info-item">
                        <small>Created Timestamp</small>
                        <strong>${ticket.createdAt}</strong>
                    </div>
                </div>

                <div style="margin-bottom: 20px;">
                    <h4 style="font-size: 13px; font-weight: 700; margin-bottom: 6px;">Reported Issue Description</h4>
                    <p style="font-size: 13px; line-height: 1.6; color: #334155; background: #f8fafc; padding: 12px; border-radius: 4px; border: 1px solid #e2e8f0;">
                        ${ticket.description}
                    </p>
                </div>

                ${ticket.resolutionNotes ? `
                    <div style="margin-bottom: 20px;">
                        <h4 style="font-size: 13px; font-weight: 700; margin-bottom: 6px; color: #15803d;">Work Performed & Closure Notes</h4>
                        <p style="font-size: 13px; line-height: 1.6; color: #1e293b; background: #f0fdf4; padding: 12px; border-radius: 4px; border: 1px solid #bbf7d0;">
                            ${ticket.resolutionNotes}
                        </p>
                    </div>
                ` : ''}

                <!-- STATUS UPDATE FORM -->
                <div style="border-top: 1px solid #e2e8f0; padding-top: 16px;">
                    <h4 style="font-size: 13px; font-weight: 700; margin-bottom: 12px;">Quick Status & Workflow Update</h4>
                    
                    <div class="form-grid">
                        <div class="field">
                            <label>Update Status to</label>
                            <select id="updateStatusSelect">
                                <option value="In Progress" ${ticket.status === 'In Progress' ? 'selected' : ''}>In Progress</option>
                                <option value="Resolved" ${ticket.status === 'Resolved' ? 'selected' : ''}>Resolved (Complete)</option>
                                <option value="Closed" ${ticket.status === 'Closed' ? 'selected' : ''}>Closed (Final Sign-off)</option>
                                <option value="Assigned" ${ticket.status === 'Assigned' ? 'selected' : ''}>Assigned</option>
                            </select>
                        </div>
                        <div class="field">
                            <label>Status Message / Activity</label>
                            <input id="updateStatusMsg" placeholder="e.g. Filter replaced; testing static pressures">
                        </div>
                    </div>

                    <div style="display: flex; gap: 8px; margin-top: 14px;">
                        <button class="btn primary" onclick="App.handleSaveStatusUpdate('${ticket.id}')">Apply Status Update</button>
                        ${ticket.status !== 'Closed' ? `
                            <button class="btn" onclick="App.openResolveModal('${ticket.id}')">Submit Resolution Notes</button>
                        ` : ''}
                    </div>
                </div>
            </article>

            <!-- RIGHT COLUMN: INTERACTIVE TECHNICIAN COMMENT THREAD -->
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Technician & Dispatch Comment Thread</h3>
                        <p>Field updates, diagnostic findings, and operational coordination.</p>
                    </div>
                </div>

                <div class="comment-thread" id="commentThreadBox">
                    ${comments.length === 0 ? `
                        <div style="padding: 20px; text-align: center; color: #94a3b8; font-size: 13px;">
                            No comments posted yet. Add the first update below.
                        </div>
                    ` : comments.map(c => `
                        <div class="comment-item">
                            <div class="comment-header">
                                <span class="comment-author">
                                    ${c.author} 
                                    <span class="badge ${c.role === 'Technician' ? 'green' : 'blue'}" style="font-size:9px; padding:2px 5px;">${c.role}</span>
                                    ${c.isInternal ? '<span class="badge purple" style="font-size:9px; padding:2px 5px;">Internal Only</span>' : ''}
                                </span>
                                <span class="comment-time">${c.time}</span>
                            </div>
                            <div class="comment-body">${c.text}</div>
                        </div>
                    `).join("")}
                </div>

                <!-- ADD COMMENT BOX -->
                <div class="comment-box-input">
                    <label style="font-size: 12px; font-weight: 600; display: block; margin-bottom: 6px;">Add Field Note / Update</label>
                    <textarea id="newCommentText" placeholder="Post a comment, diagnostic reading, or customer message..."></textarea>
                    
                    <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 10px;">
                        <label style="font-size: 12px; color: #64748b; display: flex; align-items: center; gap: 6px; cursor: pointer;">
                            <input type="checkbox" id="commentInternalCheck">
                            Internal note (hide from customer)
                        </label>
                        <button class="btn primary small" onclick="App.handleAddComment('${ticket.id}')">Post Comment</button>
                    </div>
                </div>
            </article>
        </div>
    `;
};

