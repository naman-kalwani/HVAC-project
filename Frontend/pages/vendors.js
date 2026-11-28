/**
 * Page: Vendor Management Directory (PB-017, PB-018 / US016, US017)
 */
window.Pages = window.Pages || {};

function escapeVendorHtml(value) {
    return String(value == null ? "" : value).replace(/[&<>"']/g, character => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    })[character]);
}

let vendorPageRows = [];
let vendorPagination = null;
let vendorPaginationReady = false;
function getVendorPagination() {
    if (!vendorPagination) {
        vendorPagination = App.createPagination(
            Store.vendors,
            4,
            (paginatedData, pageInfo) => {
                vendorPageRows = paginatedData;
                if (vendorPaginationReady && location.hash.replace(/^#/, "") === "vendors") {
                    Router.loadCurrentPage();
                }
            },
            "vendors",
            {
                searchText: vendor => [
                    vendor.name,
                    vendor.id,
                    vendor.region,
                    vendor.sitesCount,
                    vendor.phase,
                    vendor.status,
                ].join(" "),
                filters: { 6: vendor => vendor.status },
            }
        );
        vendorPagination.update();
        vendorPaginationReady = true;
    }
    return vendorPagination;
}


window.Pages["vendors"] = function() {
    getVendorPagination();
    const rows =  vendorPageRows.map(c => [
        `<b>${escapeVendorHtml(c.name)}</b>`,
        `<code>${escapeVendorHtml(c.id)}</code>`,
        escapeVendorHtml(c.region),
        `${escapeVendorHtml(c.sitesCount)} sites`,
        escapeVendorHtml(c.phase),
        UI.progress(escapeVendorHtml(c.progress)),
        UI.badge(c.status),
    ]);

    return `
        <article class="card">
            <div class="table-tools">
                <input class="filter-input" placeholder="Search vendor name, code or region..." data-filter  onkeyup="App.globalSearch()">
                    <select class="filter-input" data-column="6" id="vendorStatusFilter" onchange="App.globalSearch()">
                    <option value="">All</option>
                    <option value="Active">Active</option>
                    <option value="Pending Onboarding">Pending Onboarding</option>
                    <option value="Inactive">Inactive</option>
                </select>
                <button class="btn primary" onclick="openCreateVendorModal()">+ Register New Vendor</button>
            </div>

            <div class="card-title">
                <div>
                    <h3>Acquired HVAC Vendor Organizations</h3>
                    <p>Centralized directory of all roll-up companies and operating brands.</p>
                </div>
            </div>

            ${UI.table(["Vendor Name", "Code", "Territory Region", "Serviced Locations", "Onboarding Phase", "Readiness", "Status"], rows)}

            ${UI.pagination(vendorPagination.getPageInfo(), "vendors")}
            
            <div class="list-row" style="margin-top: 14px; padding-top: 10px;">
                <small style="color:#64748b">Showing ${Store.vendors.length} registered vendors across group portfolio</small>
                <small style="color:#64748b">Phase 1: Coexist & Import · Phase 2: Controlled Cutover</small>
            </div>
        </article>
    `;
};

// Aliases for navigation
window.Pages["vendors"] = window.Pages["vendors"];

function openCreateVendorModal() {
    const modal = document.getElementById("modalContainer");
    if (!modal) {
        console.error("Vendor registration modal container not found.");
        return;
    }

    modal.innerHTML = `
        <div class="modal-overlay">
            <div class="modal">
                <div class="modal-header">
                    <h2>Register New Vendor</h2>
                    <button class="close-btn" onclick="App.closeModal()">✕</button>
                </div>
                <div class="modal-body">
                    <div class="form-grid">
                        <div class="form-group">
                            <label for="vendorName">Vendor Name*</label>
                            <input id="vendorName" type="text" required maxlength="120">
                        </div>
                        <div class="form-group">
                            <label for="vendorRegion">Territory Region*</label>
                            <input id="vendorRegion" type="text" required maxlength="80">
                        </div>
                        <div class="form-group">
                            <label for="vendorContact">Primary Contact*</label>
                            <input id="vendorContact" type="number" maxlength="10">
                        </div>
                        <div class="form-group">
                            <label for="vendorEmail">Contact Email*</label>
                            <input id="vendorEmail" type="email" maxlength="254">
                        </div>
                        <div class="form-group">
                            <label for="vendorSites">Serviced Locations</label>
                            <input id="vendorSites" type="number" min="0" step="1" value="0">
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="App.closeModal()">Cancel</button>
                    <button class="btn primary" onclick="saveNewVendor()">Register Vendor</button>
                </div>
            </div>
        </div>
    `;
    modal.style.display = "flex";
}

function saveNewVendor() {
    const name = document.getElementById("vendorName").value.trim();
    const region = document.getElementById("vendorRegion").value.trim();
    const contactName = document.getElementById("vendorContact").value.trim();
    const emailInput = document.getElementById("vendorEmail");
    const contactEmail = emailInput.value.trim();
    const sitesInput = document.getElementById("vendorSites");
    const sitesCount = Number(sitesInput.value);

    if (!name || !region || !Number.isInteger(sitesCount) || sitesCount < 0 ||
        (contactEmail && !emailInput.checkValidity())) {
        App.showToast("Enter a vendor name, region, valid contact email, and location count.", "error");
        return;
    }

    const contact = [contactName, contactEmail].filter(Boolean).join(" · ") || "Not provided";
    Store.vendors.unshift({
        id: `VND-${Date.now()}`,
        name,
        region,
        status: "Pending Onboarding",
        phase: "Discovery",
        progress: "0%",
        contact,
        sitesCount
    });

    App.closeModal();
    App.showToast("Vendor registered successfully.", "success");
    if (!App.refreshPagination("vendors", Store.vendors)) Router.loadCurrentPage();
}

window.openCreateVendorModal = openCreateVendorModal;
window.saveNewVendor = saveNewVendor;
