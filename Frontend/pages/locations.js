/**
 * Page: Customer Sites & Facility Locations (PB-021 / US020)
 */
window.Pages = window.Pages || {};

function escapeLocationHtml(value) {
    return String(value == null ? "" : value).replace(/[&<>"']/g, character => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    })[character]);
}

let locationPageRows = [];
let locationPagination = null;
let locationPaginationReady = false;

function getVisibleLocations(user) {
    if (!user || user.roleKey !== "enduser") return Store.locations;

    const customer = Store.customers.find(c =>
        c.name === user.workspace ||
        c.primaryContact === user.name ||
        c.email.toLowerCase() === (user.email || "").toLowerCase());
    const customerName = customer ? customer.name : user.workspace;
    return Store.locations.filter(l =>
        (customer && l.customerId === customer.id) ||
        l.customerName === customerName);
}

function getLocationPagination(locations) {
    if (!locationPagination) {
        locationPagination = App.createPagination(
            locations,
            6,
            (paginatedData, pageInfo) => {
                locationPageRows = paginatedData;
                if (locationPaginationReady && location.hash.replace(/^#/, "") === "locations") {
                    Router.loadCurrentPage();
                }
            },
            "locations",
            {
                searchText: location => [
                    location.id,
                    location.name,
                    location.customerName,
                    location.address,
                    location.city,
                    location.vendorName,
                    location.operatingHours,
                    location.accessNotes,
                    location.status,
                ].join(" "),
                filters: { vendor: location => location.vendorName },
            }
        );
        locationPagination.update();
        locationPaginationReady = true;
    }
    return locationPagination;
}

window.Pages["locations"] = function() {
    const user = Auth.getCurrentUser();
    const isEndUser = user && user.roleKey === "enduser";
    const locations = getVisibleLocations(user);
    getLocationPagination(locations);

    return `
        <article class="card">
            <div class="card-title">
                <div>
                    <h3>${isEndUser ? "My Locations" : "Customer Sites & Facility Locations"}</h3>
                    <p>${isEndUser ? "View and add facilities registered to your account." : "Manage customer facilities and service locations."}</p>
                </div>
                ${isEndUser ? `<button class="btn primary" onclick="openCreateLocationModal()">+ Add New Location</button>` : ""}
            </div>
            <div class="table-tools">
                <input class="filter-input" placeholder="Search customer sites, address, city or vendor..." data-filter onkeyup="App.globalSearch()">
                ${isEndUser ? "" : `
                <select class="filter-input" data-card data-column="vendor" onchange="App.globalSearch()">
                    <option value="">All Vendors</option>
                    <option value="Pacific Climate">Pacific Climate Services</option>
                    <option value="Northwest Cooling">Northwest Cooling Group</option>
                    <option value="Desert Air">Desert Air Systems</option>
                </select>
                <button class="btn primary" onclick="openCreateLocationModal()">+ Add Location</button>
                `}
            </div>

            <div class="grid three-col">
                ${locationPageRows.length ? locationPageRows.map(l => `
                    <article class="site-card" data-card data-vendor="${escapeLocationHtml(l.vendorName)}">
                        <div>
                            <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom: 6px;">
                                <span style="font-size:10px; font-weight:700; color:#64748b;">${escapeLocationHtml(l.id)}</span>
                                ${UI.badge(l.status)}
                            </div>
                            <h3>${escapeLocationHtml(l.name)}</h3>
                            <p>${escapeLocationHtml(l.address)}</p>
                            <div style="font-size: 12px; color: #475569; margin-bottom: 12px;">
                                <div><b>Customer:</b> ${escapeLocationHtml(l.customerName)}</div>
                                <div><b>Vendor:</b> ${escapeLocationHtml(l.vendorName || "Pending assignment")}</div>
                                <div><b>Hours:</b> ${escapeLocationHtml(l.operatingHours || "Not provided")}</div>
                            </div>
                            <div class="callout" style="padding: 8px 12px; font-size: 11px; margin-bottom: 12px; background: #f8fafc;">
                                <b>Access Notes:</b> ${escapeLocationHtml(l.accessNotes || "No access notes provided.")}
                            </div>
                        </div>
                        <div>
                            <div class="site-meta">
                                <span>⌖ <b>${Number(l.assetCount) || 0}</b> HVAC Units</span>
                                <span>${escapeLocationHtml((l.timezone || "Local time").split('/')[1] || l.timezone || "Local time")}</span>
                            </div>
                        </div>
                    </article>
                `).join("") : `<p class="callout info">${isEndUser && !locations.length ? "No locations are registered to your account yet. Add a location to get started." : "No locations match your search."}</p>`}
            </div>
            ${UI.pagination(locationPagination.getPageInfo(), "locations")}
        </article>
    `;
};


function openCreateLocationModal() {
    const user = Auth.getCurrentUser();

    if (user && user.roleKey === "enduser") {
        openEndUserCreateLocationModal(user);
        return;
    }


    const modal = document.getElementById("modalContainer");


    if (!modal) return;


    modal.innerHTML = `
        <div class="modal-overlay">


            <div class="modal large">


                <div class="modal-header">
                    <h2>Add Customer Site / Location</h2>


                    <button
                        class="close-btn"
                        onclick="closeModal()">
                        ✕
                    </button>
                </div>


                <div class="modal-body">


                    <div class="form-grid">


                        <div class="form-group">
                            <label>Location Name *</label>
                            <input id="locationName" type="text">
                        </div>


                        <div class="form-group">
                            <label>Customer Name *</label>
                            <input id="customerName" type="text">
                        </div>


                        <div class="form-group">
                            <label>Street Address *</label>
                            <input id="address" type="text">
                        </div>


                        <div class="form-group">
                            <label>City *</label>
                            <input id="city" type="text">
                        </div>


                        <div class="form-group">
                            <label>State *</label>
                            <input id="state" type="text">
                        </div>


                        <div class="form-group">
                            <label>Postal Code *</label>
                            <input id="postalCode" type="text">
                        </div>


                        <div class="form-group">
                            <label>Assigned Vendor *</label>


                            <select id="vendorName">
                                <option value="">Select Vendor</option>
                                <option>Pacific Climate Services</option>
                                <option>Northwest Cooling Group</option>
                                <option>Desert Air Systems</option>
                            </select>
                        </div>


                        <div class="form-group">
                            <label>Operating Hours</label>


                            <input
                                id="operatingHours"
                                type="text"
                                placeholder="08:00 AM - 06:00 PM">
                        </div>


                        <div
                            class="form-group"
                            style="grid-column:1 / span 2;">


                            <label>Access Notes</label>


                            <textarea
                                id="accessNotes"
                                rows="4"
                                style="
                                    width:100%;
                                    padding:10px;
                                    border:1px solid #CBD5E1;
                                    border-radius:8px;">
                            </textarea>


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
                        onclick="saveLocation()">
                        Create Location
                    </button>


                </div>


            </div>


        </div>
    `;


    modal.style.display = "flex";
}

function openEndUserCreateLocationModal(user) {
    const modal = document.getElementById("modalContainer");
    if (!modal) {
        console.error("Location modal container not found.");
        return;
    }

    modal.innerHTML = `
        <div class="modal-overlay">
            <div class="modal large">
                <div class="modal-header">
                    <h2>Add New Location</h2>
                    <button class="close-btn" onclick="closeModal()">✕</button>
                </div>
                <div class="modal-body">
                    <p style="margin-bottom:16px;color:#64748b;">Add a facility to ${escapeLocationHtml(user.workspace || user.name)}.</p>
                    <div class="form-grid">
                        <div class="form-group">
                            <label for="userLocationName">Location Name *</label>
                            <input id="userLocationName" type="text" maxlength="120" required>
                        </div>
                        <div class="form-group">
                            <label for="userLocationAddress">Street Address *</label>
                            <input id="userLocationAddress" type="text" maxlength="200" required>
                        </div>
                        <div class="form-group">
                            <label for="userLocationCity">City *</label>
                            <input id="userLocationCity" type="text" maxlength="80" required>
                        </div>
                        <div class="form-group">
                            <label for="userLocationState">State *</label>
                            <input id="userLocationState" type="text" maxlength="80" required>
                        </div>
                        <div class="form-group">
                            <label for="userLocationPostalCode">Postal Code *</label>
                            <input id="userLocationPostalCode" type="text" inputmode="numeric" maxlength="10" required>
                        </div>
                        <div class="form-group">
                            <label for="userLocationHours">Operating Hours</label>
                            <input id="userLocationHours" type="text" maxlength="100" placeholder="e.g. Mon–Fri, 8:00 AM–6:00 PM">
                        </div>
                        <div class="form-group" style="grid-column:1 / -1;">
                            <label for="userLocationNotes">Access Notes</label>
                            <textarea id="userLocationNotes" rows="3" maxlength="500"></textarea>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn" onclick="closeModal()">Cancel</button>
                    <button class="btn primary" onclick="saveEndUserLocation()">Add Location</button>
                </div>
            </div>
        </div>
    `;
    modal.style.display = "flex";
}

function saveEndUserLocation() {
    const user = Auth.getCurrentUser();
    if (!user || user.roleKey !== "enduser") {
        App.showToast("Only end users can add locations from this page.", "error");
        return;
    }

    const value = id => document.getElementById(id).value.trim();
    const name = value("userLocationName");
    const addressLine = value("userLocationAddress");
    const city = value("userLocationCity");
    const state = value("userLocationState");
    const postalCode = value("userLocationPostalCode");

    if (!name || !addressLine || !city || !state || !postalCode) {
        App.showToast("Complete all required location fields.", "error");
        return;
    }
    if (!/^[A-Za-z0-9 -]{3,10}$/.test(postalCode)) {
        App.showToast("Enter a valid postal code.", "error");
        document.getElementById("userLocationPostalCode").focus();
        return;
    }

    const customer = Store.customers.find(c =>
        c.name === user.workspace ||
        c.primaryContact === user.name ||
        c.email.toLowerCase() === (user.email || "").toLowerCase());
    const customerName = customer ? customer.name : (user.workspace || user.name);
    const location = {
        id: `LOC-${Date.now()}`,
        name,
        customerId: customer ? customer.id : null,
        customerName,
        vendorId: null,
        vendorName: "Pending assignment",
        city: `${city}, ${state}`,
        address: `${addressLine}, ${city}, ${state} ${postalCode}`,
        timezone: "America/Los_Angeles",
        operatingHours: value("userLocationHours") || "Not provided",
        accessNotes: value("userLocationNotes") || "No access notes provided.",
        assetCount: 0,
        status: "Pending Review"
    };

    Store.locations.unshift(location);
    closeModal();
    App.showToast("Location added successfully.", "success");
    if (!App.refreshPagination("locations", getVisibleLocations(user))) Router.loadCurrentPage();
}


function saveLocation() {


    const locationName =
        document.getElementById("locationName").value.trim();


    const customerName =
        document.getElementById("customerName").value.trim();


    const address =
        document.getElementById("address").value.trim();


    const city =
        document.getElementById("city").value.trim();


    const state =
        document.getElementById("state").value.trim();


    const postalCode =
        document.getElementById("postalCode").value.trim();


    const vendorName =
        document.getElementById("vendorName").value;


    const operatingHours =
        document.getElementById("operatingHours").value.trim();


    /* ===========================
       VALIDATIONS
    =========================== */


    if (!locationName) {
        alert("Location Name is required");
        return;
    }


    if (locationName.length < 3) {
        alert("Location Name must be at least 3 characters");
        return;
    }


    if (!customerName) {
        alert("Customer Name is required");
        return;
    }


    if (customerName.length < 3) {
        alert("Customer Name must be at least 3 characters");
        return;
    }


    if (!address) {
        alert("Street Address is required");
        return;
    }


    if (!city) {
        alert("City is required");
        return;
    }


    if (!state) {
        alert("State is required");
        return;
    }


    if (!postalCode) {
        alert("Postal Code is required");
        return;
    }


    const pinRegex = /^[0-9]{6}$/;


    if (!pinRegex.test(postalCode)) {
        alert("Postal Code must contain exactly 6 digits");
        return;
    }


    if (!vendorName) {
        alert("Please select a vendor");
        return;
    }


    if (
        operatingHours &&
        !/^[0-9]{2}:[0-9]{2}/.test(operatingHours)
    ) {
        alert(
            "Operating Hours should be like 08:00 AM - 06:00 PM"
        );
        return;
    }


    const location = {
        id: "LOC-" + Date.now(),
        name: locationName,
        customerName,
        address,
        city,
        state,
        postalCode,
        vendorName,
        operatingHours,
        accessNotes:
            document.getElementById("accessNotes").value,
        assetCount: 0,
        status: "Active"
    };


    console.log("Location Payload:", location);


    alert("Location Created Successfully");


    closeModal();
}


function closeModal() {
    const modal = document.getElementById("modalContainer");


    if (!modal) return;


    modal.style.display = "none";
    modal.innerHTML = "";
}


// Aliases for navigation
window.Pages["my-sites"] = window.Pages["locations"];
window.saveEndUserLocation = saveEndUserLocation;
