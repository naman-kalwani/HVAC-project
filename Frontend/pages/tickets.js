/**
 * Page: Ticket Register (PB-007 / US007)
 */
const TICKET_PAGE_SIZE = 4;
window.Pages = window.Pages || {};  
let ticketPageRows = [];
let ticketPagination = null;
let ticketPaginationReady = false;

function getTicketPagination() {
    if (!ticketPagination) {
        ticketPagination = App.createPagination(
            Store.tickets,
            TICKET_PAGE_SIZE,
            (paginatedData, pageInfo) => {
                ticketPageRows = paginatedData;
                if (ticketPaginationReady && location.hash.replace(/^#/, "") === "tickets") {
                    Router.loadCurrentPage();
                }
            },
            "tickets",
            {
                searchText: ticket => [
                    ticket.id,
                    ticket.customerName,
                    ticket.locationName,
                    ticket.title,
                    ticket.description,
                    ticket.assetName,
                    ticket.vendorName,
                    ticket.priority,
                    ticket.slaState,
                    ticket.status,
                    ticket.technicianName,
                ].join(" "),
                filters: {
                    6: ticket => ticket.status,
                    5: ticket => ticket.slaState,
                },
            }
        );
        ticketPagination.update();
        ticketPaginationReady = true;
    }
    return ticketPagination;
}

window.Pages["tickets"] = function () {
    const user = Auth.getCurrentUser();
    const showManageAction = user && user.roleKey !== "enduser";
    getTicketPagination();
    const rows = ticketPageRows.map(t => [
        `<b>${t.id}</b>`,
        `<b>${t.customerName}</b><br><small style="color:#64748b">${t.locationName}</small>`,
        `<span title="${t.description}">${t.title}</span><br><small style="color:#64748b">Unit: ${t.assetName}</small>`,
        t.vendorName,
        UI.badge(t.priority),
        UI.badge(t.slaState),
        UI.badge(t.status),
        `<b>${t.technicianName}</b>`,
        ...(showManageAction
            ? [`<button class="btn small primary" onclick="openManageTicketModal('${t.id}')">Manage</button>`]
            : [])
    ]);

    return `
        <article class="card">
            <div class="table-tools">
                <input class="filter-input" placeholder="Search ticket ID, customer, site, asset tag or issue..." data-filter onkeyup="App.globalSearch()">
                <select class="filter-input" data-column="6" id="ticketStatusFilter" onchange="App.globalSearch()">
                    <option value="">All (5 Lifecycle Stages)</option>
                    <option value="Open">Open</option>
                    <option value="Assigned">Assigned</option>
                    <option value="In Progress">In Progress</option>
                    <option value="Resolved">Resolved</option>
                    <option value="Closed">Closed</option>
                </select>
                <select class="filter-input"  data-column="5" id="ticketSlaFilter" onchange="App.globalSearch()">
                    <option value="">All SLA States</option>
                    <option value="Within SLA">Within SLA</option>
                    <option value="At Risk">At Risk</option>
                    <option value="Breached">Breached</option>
                </select>

            </div>


            <div class="card-title">
                <div>
                    <h3>Service Operations Ticket Register</h3>
                    <p>
                        Commercial breakdown repairs, scheduled inspections,
                        and emergency calls.
                    </p>
                </div>
            </div>


            ${UI.table(
        [
            "Ticket ID",
            "Customer & Location",
            "Reported Issue",
            "Vendor",
            "Priority",
            "SLA Status",
            "Ticket Status",
            "Technician",
            ...(showManageAction ? ["Action"] : [])
        ],
        rows
    )}


        ${UI.pagination(ticketPagination.getPageInfo(), "tickets")}


            <div
                class="list-row"
                style="margin-top:14px;padding-top:10px;">


                <small style="color:#64748b">
                    Showing ${Store.tickets.length} tickets across portfolio
                </small>


                <small style="color:#64748b">
                    Lifecycle: Open → Assigned → In Progress → Resolved → Closed
                </small>


            </div>


        </article>
    `;
};




//    MANAGE TICKET MODAL
function openManageTicketModal(ticketId) {


    const ticket = Store.tickets.find(
        t => t.id === ticketId
    );


    const modal =
        document.getElementById("modalContainer");


    if (!modal || !ticket) return;


    modal.innerHTML = `
        <div class="modal-overlay">


            <div class="modal">


                <div class="modal-header">


                    <h2>
                        Manage Ticket ${ticket.id}
                    </h2>


                    <button
                        class="close-btn"
                        onclick="closeModal()">
                        ✕
                    </button>


                </div>


                <div class="modal-body">


                    <div class="form-grid">


                        <div class="form-group">
                            <label>Priority</label>


                            <select id="priority">
                                <option ${ticket.priority === "Low" ? "selected" : ""}>Low</option>
                                <option ${ticket.priority === "Medium" ? "selected" : ""}>Medium</option>
                                <option ${ticket.priority === "High" ? "selected" : ""}>High</option>
                                <option ${ticket.priority === "Critical" ? "selected" : ""}>Critical</option>
                            </select>
                        </div>


                        <div class="form-group">
                            <label>SLA Status</label>


                            <select id="slaState">
                                <option ${ticket.slaState === "Within SLA" ? "selected" : ""}>Within SLA</option>
                                <option ${ticket.slaState === "At Risk" ? "selected" : ""}>At Risk</option>
                                <option ${ticket.slaState === "Breached" ? "selected" : ""}>Breached</option>
                            </select>
                        </div>


                        <div class="form-group">
                            <label>Ticket Status</label>


                            <select id="ticketStatus">
                                <option ${ticket.status === "Open" ? "selected" : ""}>Open</option>
                                <option ${ticket.status === "Assigned" ? "selected" : ""}>Assigned</option>
                                <option ${ticket.status === "In Progress" ? "selected" : ""}>In Progress</option>
                                <option ${ticket.status === "Resolved" ? "selected" : ""}>Resolved</option>
                                <option ${ticket.status === "Closed" ? "selected" : ""}>Closed</option>
                            </select>
                        </div>


                    </div>


                </div>


                <div class="modal-footer">


                    <button
                        class="btn"
                        onclick="closeModal()">
                        Cancel
                    </button>


                    <button
                        class="btn primary"
                        onclick="saveTicketChanges('${ticket.id}')">
                        Update Ticket
                    </button>


                </div>


            </div>


        </div>
    `;


    modal.style.display = "flex";
}






//    SAVE CHANGES


function saveTicketChanges(ticketId) {


    const ticket = Store.tickets.find(
        t => t.id === ticketId
    );


    if (!ticket) return;


    ticket.priority = document.getElementById("priority").value;
    ticket.slaState = document.getElementById("slaState").value;
    ticket.status = document.getElementById("ticketStatus").value;

    closeModal();
    App.showToast("Ticket updated successfully.", "success");
    if (!App.refreshPagination("tickets", Store.tickets)) Router.loadCurrentPage();
}




//    CLOSE MODAL




function closeModal() {


    const modal =
        document.getElementById("modalContainer");


    if (!modal) return;


    modal.style.display = "none";
    modal.innerHTML = "";
}