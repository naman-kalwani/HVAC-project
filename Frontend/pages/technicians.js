/**
 * Page: Technicians & Workers Roster (PB-004, PB-009 / US004)
 */
window.Pages = window.Pages || {};

let technicianPageRows = [];
let technicianPagination = null;
let technicianPaginationReady = false;
function getTechnicianPagination() {
    if (!technicianPagination) {
        technicianPagination = App.createPagination(
            Store.technicians,
            4,
            (paginatedData, pageInfo) => {
                technicianPageRows = paginatedData;
                if (technicianPaginationReady && location.hash.replace(/^#/, "") === "technicians") {
                    Router.loadCurrentPage();
                }
            },
            "technicians",
            {
                searchText: technician => [
                    technician.name,
                    technician.id,
                    technician.vendorName,
                    technician.specialization,
                    technician.activeTickets,
                    technician.phone,
                    technician.status,
                ].join(" "),
                filters: { 2: technician => technician.vendorName },
            }
        );
        technicianPagination.update();
        technicianPaginationReady = true;
    }
    return technicianPagination;
}

window.Pages["technicians"] = function() {

    getTechnicianPagination();
    const rows = technicianPageRows.map(t => [
        `<b>${t.name}</b>`,
        `<code>${t.id}</code>`,
        t.vendorName,
        t.specialization,
        `<span class="badge ${t.activeTickets > 2 ? "amber" : "green"}">
            ${t.activeTickets} jobs
        </span>`,
        t.phone,
        UI.badge(t.status),
        `<button class="btn small" onclick="openTechnicianDetail('${t.id}')">
            View Schedule
        </button>`
    ]);

    return `
        <article class="card">
            <div class="table-tools">
                <input class="filter-input" placeholder="Search technician name, ID, vendor or specialization..." data-filter onkeyup="App.globalSearch()">
                <select class="filter-input" data-column="2" onchange="App.globalSearch()">
                    <option value="">All Vendors</option>
                    <option value="Pacific Climate Services">Pacific Climate Services</option>
                    <option value="Northwest Cooling Group">Northwest Cooling Group</option>
                    <option value="Desert Air Systems">Desert Air Systems</option>
                </select>
                <button class="btn primary" onclick="openCreateTechnicianModal()">+ Add Technician / Worker</button>
            </div>

            <div class="card-title">
                <div>
                    <h3>Field Service Technicians & Crew Roster</h3>
                    <p>Qualified field technicians, trade certifications, and active dispatch assignments across vendors.</p>
                </div>
            </div>

            ${UI.table(
                [
                    "Technician Name",
                    "Employee ID",
                    "Vendor Affiliation",
                    "Trade Specialization",
                    "Active Workload",
                    "Contact Phone",
                    "Status",
                    "Actions"
                ],
                rows
            )}

            ${UI.pagination(technicianPagination.getPageInfo(), "technicians")}


            <div
                class="list-row"
                style="margin-top:14px;padding-top:10px;">
                <small style="color:#64748b">
                    Showing ${Store.technicians.length} field workers across group
                </small>

                <small style="color:#64748b">
                    Directly assignable through Service Coordinator dispatch desk
                </small>
            </div>

        </article>
    `;
};


  // Technician Detail Modal

function openTechnicianDetail(technicianId) {

    const tech = Store.technicians.find(
        t => t.id === technicianId
    );

    const modal = document.getElementById("modalContainer");

    if (!modal || !tech) return;

    modal.innerHTML = `
        <div class="modal-overlay">

            <div class="modal">

                <div class="modal-header">
                    <h2>${tech.name}</h2>

                    <button
                        class="close-btn"
                        onclick="closeModal()">
                        ✕
                    </button>
                </div>

                <div class="modal-body">

                    <div class="form-grid">

                        <div class="form-group">
                            <label>Employee ID</label>
                            <p>${tech.id}</p>
                        </div>

                        <div class="form-group">
                            <label>Vendor</label>
                            <p>${tech.vendorName}</p>
                        </div>

                        <div class="form-group">
                            <label>Specialization</label>
                            <p>${tech.specialization}</p>
                        </div>

                        <div class="form-group">
                            <label>Phone Number</label>
                            <p>${tech.phone}</p>
                        </div>

                        <div class="form-group">
                            <label>Active Jobs</label>
                            <p>${tech.activeTickets}</p>
                        </div>

                        <div class="form-group">
                            <label>Status</label>
                            <p>${tech.status}</p>
                        </div>

                    </div>

                    <hr style="margin:20px 0">

                    <h3>Today's Schedule</h3>

                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Time</th>
                                <th>Customer</th>
                                <th>Location</th>
                                <th>Status</th>
                            </tr>
                        </thead>

                        <tbody>
                            <tr>
                                <td>09:00 AM</td>
                                <td>Global Foods</td>
                                <td>Seattle</td>
                                <td>Completed</td>
                            </tr>

                            <tr>
                                <td>11:30 AM</td>
                                <td>SuperMart</td>
                                <td>Bellevue</td>
                                <td>In Progress</td>
                            </tr>

                            <tr>
                                <td>03:00 PM</td>
                                <td>Central Mall</td>
                                <td>Tacoma</td>
                                <td>Scheduled</td>
                            </tr>
                        </tbody>
                    </table>

                </div>

                <div class="modal-footer">
                    <button
                        class="btn"
                        onclick="closeModal()">
                        Close
                    </button>
                </div>

            </div>

        </div>
    `;

    modal.style.display = "flex";
}

/* ==================================================
   Close Modal
================================================== */

function closeModal() {

    const modal = document.getElementById("modalContainer");

    if (!modal) return;

    modal.style.display = "none";
    modal.innerHTML = "";
}

//    Create Technician Modal

function openCreateTechnicianModal() {

    const modal = document.getElementById("modalContainer");

    if (!modal) return;

    modal.innerHTML = `
        <div class="modal-overlay">

            <div class="modal">

                <div class="modal-header">
                    <h2>Add Technician / Worker</h2>

                    <button
                        class="close-btn"
                        onclick="closeModal()">
                        ✕
                    </button>
                </div>

                <div class="modal-body">

                    <form id="techForm" onsubmit="saveTechnician(event)">
                    <div class="form-grid">

                        <div class="form-group">
                            <label>Full Name *</label>
                            <input
                                id="techName"
                                type="text"
                                required
                                placeholder="John Smith">
                        </div>

                        <div class="form-group">
                            <label>Email Address *</label>
                            <input
                                id="techEmail"
                                type="email"
                                required
                                maxlength="254"
                                placeholder="john@company.com">
                        </div>

                        <div class="form-group">
                            <label>Phone Number *</label>
                            <input
                                id="techPhone"
                                type="tel"
                                inputmode="numeric"
                                pattern="[0-9]{10}"
                                required
                                oninput="this.value = this.value.replace(/\\D/g, '').slice(0, 10)"
                                placeholder="e.g. 9876543210">
                        </div>

                        <div class="form-group">
                            <label>Password *</label>
                            <input
                                id="techPassword"
                                type="password"
                                minlength="8"
                                required
                                placeholder="Minimum 8 characters">
                        </div>

                        <div class="form-group">
                            <label>Vendor *</label>
                            <select id="techVendor" required>
                                <option value="">Select Vendor</option>
                                <option>Pacific Climate Services</option>
                                <option>Northwest Cooling Group</option>
                                <option>Desert Air Systems</option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Specialization *</label>
                            <select id="techSpecialization" required>
                                <option value="">Select Specialization</option>
                                <option>HVAC / Chillers</option>
                                <option>Refrigeration</option>
                                <option>Electrical</option>
                                <option>Mechanical</option>
                                <option>Controls & Automation</option>
                            </select>
                        </div>

                    </div>
                    </form>

                </div>

                <div class="modal-footer">

                    <button
                        class="btn"
                        onclick="closeModal()">
                        Cancel
                    </button>

                    <button
                        class="btn primary"
                        type="submit"
                        form="techForm">
                        Create Technician
                    </button>

                </div>

            </div>

        </div>
    `;

    modal.style.display = "flex";
}

//   Save Technician

function saveTechnician(event) {
    if (event) event.preventDefault();

    const form = document.getElementById("techForm");
    if (!form || !form.reportValidity()) return;

    const nameInput = document.getElementById("techName");
    const emailInput = document.getElementById("techEmail");
    const phoneInput = document.getElementById("techPhone");
    const passwordInput = document.getElementById("techPassword");
    const vendorInput = document.getElementById("techVendor");
    const specializationInput = document.getElementById("techSpecialization");
    const name = nameInput.value.trim();
    const email = emailInput.value.trim();
    const phone = phoneInput.value.trim();

    if (!name) {
        App.showToast("Enter the technician's full name.", "error");
        nameInput.focus();
        return;
    }

    if (!emailInput.validity.valid) {
        App.showToast("Enter a valid email address.", "error");
        emailInput.focus();
        return;
    }

    if (!/^\d{10}$/.test(phone)) {
        App.showToast("Enter a valid 10-digit mobile number.", "error");
        phoneInput.focus();
        return;
    }

    if (passwordInput.value.length < 8) {
        App.showToast("Password must be at least 8 characters.", "error");
        passwordInput.focus();
        return;
    }

    if (!vendorInput.value) {
        App.showToast("Select a vendor.", "error");
        vendorInput.focus();
        return;
    }

    if (!specializationInput.value) {
        App.showToast("Select a specialization.", "error");
        specializationInput.focus();
        return;
    }

    const technician = {
        id: "TECH" + Date.now(),
        name,
        email,
        phone,
        password: passwordInput.value,
        vendorName: vendorInput.value,
        specialization: specializationInput.value,
        status: "Active",
        activeTickets: 0
    };

    console.log("Technician Payload:", technician);


    alert("Technician Created Successfully");


    closeModal();
}
//Expose Functions Globally
window.openTechnicianDetail = openTechnicianDetail;
window.openCreateTechnicianModal = openCreateTechnicianModal;
window.saveTechnician = saveTechnician;
window.closeModal = closeModal;