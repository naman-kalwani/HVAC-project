// Core app initialization and navigation for HVAC Connect frontend
// Ties together Auth, Router, UI helpers and the role-based sidebar.

window.App = (function () {
  const paginationControllers = new Map();

  // Sidebar navigation items per role
  const sidebarConfig = {
    "Super Admin": [
      { key: "dashboard",   label: "Dashboard",         },
      { key: "imports",      label: "Import Wizard",    },
      { key: "duplicates",   label: "Duplicate Review", },
      { key: "onboarding",   label: "Onboarding",       },
      { key: "vendors",  label: "Vendors",      },
      { key: "customers",    label: "Customers",        },
      { key: "assets",       label: "Assets",           },
      { key: "technicians",  label: "Technicians",      },
      { key: "tickets",      label: "Tickets",          },
     
    ],
    "Vendor Admin": [
      { key: "vendor-dashboard", label: "Dashboard",   },
      { key: "customers",        label: "Customers",   },
      { key: "assets",           label: "Assets",      },
      { key: "technicians",      label: "Technicians", },
      { key: "tickets",          label: "Tickets",     },
    ],
    "Service Coordinator": [
      { key: "coordinator-dashboard", label: "Dashboard",    },
      { key: "tickets",               label: "Ticket Queue", },
      { key: "technicians",           label: "Technicians",  },
    ],
    "Technician": [
      { key: "technician-dashboard", label: "My Dashboard",  },
      { key: "tickets",              label: "My Tickets",    },
    ],
    "End User": [
      { key: "enduser-dashboard", label: "My Dashboard",},
      { key: "assets",           label: "My Assets", },
      { key: "tickets",           label: "My Tickets", },
      { key: "locations",         label: "My Locations", },
    ],
  };

  /** Build the sidebar HTML using CSS classes that match main.css */
  function renderSidebar() {
    const user = Auth.getCurrentUser();
    if (!user) return;
    const container = document.getElementById('sidebar');
    if (!container) return;

    const items = sidebarConfig[user.role] || [];
    const currentHash = (location.hash || '').replace('#', '');

     const linksHtml = items.map(item => {
      const isActive = currentHash === item.key ? ' active' : '';
      return `<a href="#${item.key}" class="nav-link${isActive}" data-key="${item.key}">
                <span>${item.label}</span>
              </a>`;
    }).join('');

    container.innerHTML = `
      <div class="side-context">
        <small>Workspace</small>
        <strong>${user.workspace || 'HVAC Connect'}</strong>
      </div>
      <nav>${linksHtml}</nav>
    `;

    // Click handler
    container.querySelectorAll('.nav-link').forEach(link => {
      link.addEventListener('click', function (e) {
        e.preventDefault();
        navigate(this.dataset.key);
      });
    });
  }

  function refreshPagination(paginationId, data) {
    const controller = paginationControllers.get(paginationId);
    if (!controller) return false;
    controller.setData(data);
    return true;
  }

  /** Highlight the active sidebar link */
  function updateActiveLink() {
    const hash = (location.hash || '').replace('#', '');
    document.querySelectorAll('#sidebar .nav-link').forEach(link => {
      link.classList.toggle('active', link.dataset.key === hash);
    });
  }

    /** Open Add Equipment Modal */
  function openCreateAssetModal() {

    const modal = document.getElementById('modalContainer');

    modal.innerHTML = `
      <div class="modal-overlay">
        <div class="modal large">

          <div class="modal-header">
            <h2>Add Equipment Unit</h2>
            <button class="close-btn" onclick="App.closeModal()">✕</button>
          </div>

          <div class="modal-body">

            <div class="form-grid">

              <div class="form-group">
                <label>Asset ID *</label>
                <input type="text"
                       id="assetId"
                       placeholder="AHU-00182">
              </div>

              <div class="form-group">
                <label>Category *</label>
                <select id="assetCategory">
                  <option value="">Select Category</option>
                  <option value="Air Handling Unit">Air Handling Unit</option>
                  <option value="Rooftop Unit">Rooftop Unit</option>
                  <option value="Chiller">Chiller</option>
                  <option value="Boiler">Boiler</option>
                  <option value="Walk-in Cooler">Walk-in Cooler</option>
                  <option value="Refrigeration Rack">Refrigeration Rack</option>
                </select>
              </div>

              <div class="form-group">
                <label>Category Code *</label>
                <select id="categoryCode" disabled>
                  <option value="">Select Category First</option>
                  <option value="AHU">AHU</option>
                  <option value="RTU">RTU</option>
                  <option value="CH">CH</option>
                  <option value="BLR">BLR</option>
                  <option value="WIK">WIK</option>
                  <option value="RCK">RCK</option>
                </select>
              </div>

              <div class="form-group">
                <label>Manufacturer *</label>
                <input type="text"
                       id="manufacturer"
                       placeholder="Carrier">
              </div>

              <div class="form-group">
                <label>Model *</label>
                <input type="text"
                       id="model"
                       placeholder="39HQ">
              </div>

              <div class="form-group">
                <label>Serial Number *</label>
                <input type="text"
                       id="serial"
                       placeholder="SN-CR-992144">
              </div>

              <div class="form-group">
                <label>Site ID *</label>
                <input type="text"
                       id="siteId"
                       placeholder="LOC-00812">
              </div>

              <div class="form-group">
                <label>Location Name *</label>
                <input type="text"
                       id="locationName"
                       placeholder="Apex - San Jose HQ">
              </div>

              <div class="form-group">
                <label>Refrigerant *</label>
                <input type="text"
                       id="refrigerant"
                       placeholder="N/A (Chilled Water)">
              </div>

              <div class="form-group">
                <label>Install Date *</label>
                <input type="date"
                       id="installDate">
              </div>

              <div class="form-group">
                <label>Warranty End Date *</label>
                <input type="date"
                       id="warrantyEnd">
              </div>

              <div class="form-group">
                <label>Status *</label>
                <select id="assetStatus">
                  <option value="">Select Status</option>
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                  <option value="Maintenance Due">Maintenance Due</option>
                  <option value="Out Of Service">Out Of Service</option>
                </select>
              </div>

            </div>

          </div>

          <div class="modal-footer">
            <button class="btn"
                    onclick="App.closeModal()">
              Cancel
            </button>

            <button class="btn primary"
                    onclick="App.saveAsset()">
              Save Equipment
            </button>
          </div>

        </div>
      </div>
    `;

    modal.style.display = 'flex';

    // Auto-populate Category Code

    const category = document.getElementById('assetCategory');
    const categoryCode = document.getElementById('categoryCode');

    const mapping = {
        "Air Handling Unit": "AHU",
        "Rooftop Unit": "RTU",
        "Chiller": "CH",
        "Boiler": "BLR",
        "Walk-in Cooler": "WIK",
        "Refrigeration Rack": "RCK"
    };

    category.addEventListener('change', function () {
        categoryCode.value = mapping[this.value] || "";
    });
}

function createPagination(data, pageSize, renderCallback, paginationId, options) {
  if (!Array.isArray(data)) {
    throw new TypeError("Pagination data must be an array.");
  }
  if (!Number.isInteger(pageSize) || pageSize < 1) {
    throw new RangeError("Pagination page size must be a positive integer.");
  }
  if (typeof renderCallback !== "function") {
    throw new TypeError("Pagination requires a render callback.");
  }
  if (!paginationId || paginationControllers.has(paginationId)) {
    throw new Error("Pagination IDs must be unique and non-empty.");
  }

  const config = options || {};
  let sourceData = data;
  let filteredData = data;
  let currentPage = 1;
  let search = "";
  let filters = {};

  function matches(item) {
    const searchableText = config.searchText
      ? String(config.searchText(item)).toLowerCase()
      : "";
    if (search && !searchableText.includes(search)) return false;

    return Object.keys(filters).every(column => {
      const getValue = config.filters && config.filters[column];
      if (!getValue || !filters[column]) return true;
      return String(getValue(item)).toLowerCase().includes(filters[column]);
    });
  }

  function getPageInfo() {
    const totalItems = filteredData.length;
    const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
    currentPage = Math.min(currentPage, totalPages);
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = Math.min(startIndex + pageSize, totalItems);
    return {
      currentPage,
      totalPages,
      totalItems,
      startItem: totalItems ? startIndex + 1 : 0,
      endItem: endIndex,
    };
  }

  function update() {
    filteredData = sourceData.filter(matches);
    const pageInfo = getPageInfo();
    const startIndex = (pageInfo.currentPage - 1) * pageSize;
    renderCallback(filteredData.slice(startIndex, startIndex + pageSize), pageInfo);
    return pageInfo;
  }

  function setFilters(nextSearch, nextFilters) {
    search = String(nextSearch || "").trim().toLowerCase();
    filters = Object.keys(nextFilters || {}).reduce((values, column) => {
      values[column] = String(nextFilters[column] || "").trim().toLowerCase();
      return values;
    }, {});
    currentPage = 1;
    update();
  }

  function setData(nextData) {
    if (!Array.isArray(nextData)) {
      throw new TypeError("Pagination data must be an array.");
    }
    sourceData = nextData;
    currentPage = 1;
    update();
  }

  function next() {
    const pageInfo = getPageInfo();
    if (currentPage < pageInfo.totalPages) {
      currentPage++;
      update();
    }
  }

  function prev() {
    if (currentPage > 1) {
      currentPage--;
      update();
    }
  }

  function reset() {
    currentPage = 1;
    update();
  }

  const controller = { next, prev, reset, update, setData, setFilters, getPageInfo };
  paginationControllers.set(paginationId, controller);
  return controller;
}

function setupPaginationControls() {
  const container = document.getElementById("pageContent");
  if (!container || container.dataset.paginationBound) return;

  container.dataset.paginationBound = "true";
  container.addEventListener("click", event => {
    const button = event.target.closest("[data-pagination-action]");
    if (!button || !container.contains(button)) return;

    const controller = paginationControllers.get(button.dataset.paginationId);
    if (!controller) {
      console.error(`Pagination controller "${button.dataset.paginationId}" was not found.`);
      return;
    }

    if (button.dataset.paginationAction === "next") controller.next();
    else if (button.dataset.paginationAction === "prev") controller.prev();
  });
}


/** Temporary Save Function */
function saveAsset() {


  const saveBtn = document.querySelector(
    '.modal-footer .btn.primary'
);

if (saveBtn) {
    saveBtn.disabled = true;
}

    const asset = {
        id: document.getElementById('assetId').value.trim(),
        category: document.getElementById('assetCategory').value.trim(),
        categoryCode: document.getElementById('categoryCode').value.trim(),
        manufacturer: document.getElementById('manufacturer').value.trim(),
        model: document.getElementById('model').value.trim(),
        serial: document.getElementById('serial').value.trim(),
        siteId: document.getElementById('siteId').value.trim(),
        locationName: document.getElementById('locationName').value.trim(),
        refrigerant: document.getElementById('refrigerant').value.trim(),
        installDate: document.getElementById('installDate').value,
        warrantyEnd: document.getElementById('warrantyEnd').value,
        status: document.getElementById('assetStatus').value
    };

    if (!asset.id) {
    App.showToast('Asset ID is required', 'error');
    return;
}

if (!asset.category) {
    App.showToast('Please select a category', 'error');
    return;
}

if (!asset.categoryCode) {
    App.showToast('Category Code is required', 'error');
    return;
}

if (!asset.manufacturer) {
    App.showToast('Manufacturer is required', 'error');
    return;
}

if (!asset.model) {
    App.showToast('Model is required', 'error');
    return;
}

if (!asset.serial) {
    App.showToast('Serial Number is required', 'error');
    return;
}

if (!asset.siteId) {
    App.showToast('Site ID is required', 'error');
    return;
}

if (!asset.locationName) {
    App.showToast('Location Name is required', 'error');
    return;
}

if (!asset.refrigerant) {
    App.showToast('Refrigerant is required', 'error');
    return;
}

if (!asset.installDate) {
    App.showToast('Install Date is required', 'error');
    return;
}

if (!asset.warrantyEnd) {
    App.showToast('Warranty End Date is required', 'error');
    return;
}

if (!asset.status) {
    App.showToast('Please select a status', 'error');
    return;
}

const assetPattern = /^[A-Z]{2,5}-\d{3,10}$/;

if (!assetPattern.test(asset.id)) {
    App.showToast('Asset ID should be like AHU-00182', 'error');
    return;
}

const categoryCodePattern = /^[A-Z]{2,5}$/;

if (!categoryCodePattern.test(asset.categoryCode)) {
    App.showToast('Invalid Category Code', 'error');
    return;
}

const sitePattern = /^LOC-\d{3,10}$/;

if (!sitePattern.test(asset.siteId)) {
    App.showToast('Site ID should be like LOC-00812', 'error');
    return;
}

const today = new Date();
today.setHours(0,0,0,0);

const warrantyDate = new Date(asset.warrantyEnd);

if (warrantyDate <= today) {
    App.showToast('Warranty End Date must be a future date', 'error');
    return;
}

const installDate = new Date(asset.installDate);

if (warrantyDate <= installDate) {
    App.showToast('Warranty End Date must be after Install Date', 'error');
    return;
}

    console.log('Equipment Payload:', asset);

    Store.assets.push(asset);

    App.showToast('Equipment added successfully', 'success');
    App.closeModal();
    if (!refreshPagination("assets", Store.assets)) Router.loadCurrentPage();

}

/** Close Modal */
function closeModal() {

    const modal = document.getElementById('modalContainer');

    if (!modal) return;

    modal.style.display = 'none';
    modal.innerHTML = '';
}


  /** Navigate to a page */
  function navigate(page) {
    location.hash = page;
    updateActiveLink();
  }

  /** Log out */
  function logout() {
    Auth.logout();
  }

  /** Mobile menu toggle */
  function setupMobileMenu() {
    const toggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');
    if (toggle && sidebar) {
      toggle.addEventListener('click', () => {
        sidebar.classList.toggle('open');
      });
    }
  }

  function showToast(message, type = 'success') {

    const toast = document.createElement('div');

    toast.className = `toast toast-${type}`;
    toast.textContent = message;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('show');
    }, 100);

    setTimeout(() => {
        toast.classList.remove('show');

        setTimeout(() => {
            toast.remove();
        }, 300);
    }, 3000);
}



function openRepairHistory(assetId) {
    const modal = document.getElementById('modalContainer');

    if (!modal) {
        console.error('Modal container not found');
        return;
    }

    // Find the selected asset
    const asset = Store.assets.find(item => item.id === assetId);

    if (!asset) {
        App.showToast('Asset details could not be found', 'error');
        return;
    }

    // Get repair records belonging to the selected asset
    const history = Store.repairHistory
        .filter(item => item.assetId === assetId)
        .sort((a, b) => new Date(b.serviceDate) - new Date(a.serviceDate));

    // Create timeline records
    const historyHtml = history.length
        ? history.map(item => `
            <div class="service-timeline-item">

                <div class="service-timeline-marker"></div>

                <article class="service-history-card">

                    <div class="service-history-top">
                        <div>
                            <span class="service-date">
                                ${item.serviceDate}
                            </span>

                            <h4>
                                ${item.title}
                            </h4>

                            <span class="service-reference">
                                ${item.id} · ${item.ticketId}
                            </span>
                        </div>

                        <span class="service-status service-status-${getStatusClass(item.status)}">
                            ${item.status}
                        </span>
                    </div>

                    <div class="service-meta-grid">

                        <div>
                            <small>Service Type</small>
                            <strong>${item.serviceType}</strong>
                        </div>

                        <div>
                            <small>Priority</small>
                            <strong>${item.priority}</strong>
                        </div>

                        <div>
                            <small>Technician</small>
                            <strong>${item.technicianName}</strong>
                        </div>

                        <div>
                            <small>Service Provider</small>
                            <strong>${item.vendorName}</strong>
                        </div>

                        <div>
                            <small>Labor Hours</small>
                            <strong>${item.laborHours} hours</strong>
                        </div>

                    </div>

                    <div class="service-detail-section">
                        <h5>Findings</h5>
                        <p>${item.findings}</p>
                    </div>

                    <div class="service-detail-section">
                        <h5>Action Taken</h5>
                        <p>${item.actionTaken}</p>
                    </div>

                    ${
                        item.resolutionNotes
                            ? `
                                <div class="service-resolution">
                                    <h5>Resolution</h5>
                                    <p>${item.resolutionNotes}</p>
                                </div>
                              `
                            : ''
                    }

                </article>

            </div>
        `).join('')
        : `
            <div class="empty-service-history">
                <div class="empty-service-icon">🔧</div>

                <h3>No service history available</h3>

                <p>
                    No repair or maintenance activity has been
                    recorded for this asset.
                </p>
            </div>
        `;

    // Render modal
    modal.innerHTML = `
        <div class="modal-overlay">

            <div class="modal large service-history-modal">

                <div class="modal-header">

                    <div>
                        <h2>Specifications & Repair History</h2>

                        <p class="modal-subtitle">
                            Complete maintenance timeline for ${asset.id}
                        </p>
                    </div>

                    <button
                        type="button"
                        class="close-btn"
                        aria-label="Close repair history"
                        onclick="App.closeModal()">
                        ✕
                    </button>

                </div>

                <div class="modal-body service-history-body">

                    <section class="asset-history-summary">

                        <div class="asset-history-heading">

                            <span class="asset-symbol">
                                ${asset.categoryCode}
                            </span>

                            <div>
                                <h3>
                                    ${asset.manufacturer} ${asset.model}
                                </h3>

                                <p>
                                    ${asset.id} · ${asset.category}
                                </p>
                            </div>

                            <span class="asset-history-status">
                                ${asset.status}
                            </span>

                        </div>

                        <div class="asset-specification-grid">

                            <div>
                                <small>Serial Number</small>
                                <strong>${asset.serial}</strong>
                            </div>

                            <div>
                                <small>Site ID</small>
                                <strong>${asset.siteId}</strong>
                            </div>

                            <div>
                                <small>Location</small>
                                <strong>${asset.locationName}</strong>
                            </div>

                            <div>
                                <small>Refrigerant</small>
                                <strong>${asset.refrigerant}</strong>
                            </div>

                            <div>
                                <small>Install Date</small>
                                <strong>
                                    ${formatServiceDate(asset.installDate)}
                                </strong>
                            </div>

                            <div>
                                <small>Warranty Until</small>
                                <strong>
                                    ${formatServiceDate(asset.warrantyEnd)}
                                </strong>
                            </div>

                        </div>

                    </section>

                    <div class="service-history-heading-row">

                        <div>
                            <h3>Service Timeline</h3>

                            <p>
                                ${history.length}
                                ${
                                    history.length === 1
                                        ? 'service record'
                                        : 'service records'
                                }
                            </p>
                        </div>

                    </div>

                    <section class="service-timeline">
                        ${historyHtml}
                    </section>

                </div>

                <div class="modal-footer">

                    <button
                        type="button"
                        class="btn"
                        onclick="App.closeModal()">
                        Close
                    </button>

                </div>

            </div>

        </div>
    `;

    modal.style.display = 'flex';
}


function getStatusClass(status) {
    const normalizedStatus = String(status || '')
        .toLowerCase()
        .replace(/\s+/g, '-');

    const allowedClasses = [
        'resolved',
        'completed',
        'closed',
        'in-progress',
        'open'
    ];

    return allowedClasses.includes(normalizedStatus)
        ? normalizedStatus
        : 'default';
}

function formatServiceDate(dateValue) {
    if (!dateValue) {
        return 'Not available';
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
        return dateValue;
    }

    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: '2-digit'
    });
}

function formatServiceDate(dateValue) {
    if (!dateValue) return 'Not available';

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
        return dateValue;
    }

    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: '2-digit'
    });
}


function openResolveModal(ticketId) {
    const modal = document.getElementById('modalContainer');
    if (!modal) return;


    modal.innerHTML = `
        <div class="modal-overlay">
            <div class="modal large"
                 style="width:600px;max-width:95vw;background:#fff;border-radius:12px;">


                <div class="modal-header"
                     style="display:flex;justify-content:space-between;align-items:center;padding:24px 30px;border-bottom:1px solid #e2e8f0;">
                    <h2 style="margin:0;font-size:32px;font-weight:700;">
                        Submit Resolution
                    </h2>


                    <button class="close-btn"
                            style="background:none;border:none;font-size:24px;cursor:pointer;"
                            onclick="App.closeModal()">
                        ✕
                    </button>
                </div>


                <div class="modal-body"
                     style="padding:30px;">


                    <div class="form-group"
                         style="display:flex;flex-direction:column;">
                        <label style="margin-bottom:8px;font-weight:600;color:#334155;">
                            Resolution Remarks <span style="color:red">*</span>
                        </label>


                        <textarea id="resolutionRemarks"
                                  rows="5"
                                  placeholder="Enter resolution details..."
                                  style="width:100%;border:1px solid #cbd5e1;border-radius:8px;padding:12px;resize:vertical;"></textarea>


                        <span id="resolutionError"
                              style="display:none;color:#ef4444;font-size:12px;margin-top:6px;">
                            Resolution Remarks are required.
                        </span>
                    </div>


                </div>


                <div class="modal-footer"
                     style="display:flex;justify-content:flex-end;gap:12px;padding:20px 30px;background:#f8fafc;border-top:1px solid #e2e8f0;">
                    <button class="btn primary"
                            onclick="App.saveResolution('${ticketId}')">
                        Submit
                    </button>


                </div>


            </div>
        </div>
    `;


    modal.style.display = 'flex';


    return modal;
}


function saveResolution(ticketId) {
  const remarksElement = document.getElementById('resolutionRemarks');
  const errorElement = document.getElementById('resolutionError');


  if (!remarksElement || !errorElement) {
      console.error('Resolution elements not found');
      return;
  }


  const remarks = remarksElement.value.trim();


  // Mandatory validation
  if (remarks.length === 0) {
      errorElement.style.display = 'block';
      remarksElement.style.border = '1px solid #ef4444';
      remarksElement.focus();
      return;
  }


  // Clear error
  errorElement.style.display = 'none';
  remarksElement.style.border = '1px solid #cbd5e1';


  const ticket = Store.updateTicketStatus(
      ticketId,
      'Resolved',
      'Technician submitted resolution notes.',
      remarks
  );

  if (!ticket) {
      showToast('Ticket could not be found.', 'error');
      return;
  }

  closeModal();
  showToast('Resolution submitted successfully.', 'success');
  Router.loadCurrentPage();
}


  /* View Details and Comments Button */ 


  function openTicketDetail(ticketId) {


    const existingModal = document.getElementById('ticketDetailModal');
    if (existingModal) {
        existingModal.remove();
    }


    const modal = document.createElement('div');
    modal.id = 'ticketDetailModal';


    modal.innerHTML = `
        <div class="modal-overlay"
             style="position:fixed;top:0;left:0;width:100%;height:100%;
                    background:rgba(0,0,0,0.5);display:flex;
                    justify-content:center;align-items:center;z-index:9999;">


            <div class="modal large"
                 style="width:600px;max-width:95vw;background:#fff;border-radius:12px;">


                <div class="modal-header"
                     style="display:flex;justify-content:space-between;
                            align-items:center;padding:24px 30px;
                            border-bottom:1px solid #e2e8f0;">


                    <h2 style="margin:0;font-size:32px;font-weight:700;">
                        Ticket Details & Comments
                    </h2>


                    <button
                        style="background:none;border:none;
                               font-size:24px;cursor:pointer;"
                        onclick="document.getElementById('ticketDetailModal').remove()">
                        ✕
                    </button>


                </div>


                <div class="modal-body"
                     style="padding:30px;max-height:70vh;overflow-y:auto;">


                    <div style="display:grid;grid-template-columns:1fr 1fr;gap:24px;">


                        <div>
                            <strong>Ticket ID</strong>
                            <div>${ticketId}</div>
                        </div>


                        <div>
                            <strong>Status</strong>
                            <div>In Progress</div>
                        </div>


                        <div>
                            <strong>Priority</strong>
                            <div>High</div>
                        </div>


                        <div>
                            <strong>SLA Status</strong>
                            <div>Breached</div>
                        </div>


                        <div>
                            <strong>Assigned Technician</strong>
                            <div>John Smith</div>
                        </div>


                        <div>
                            <strong>Equipment</strong>
                            <div>Carrier 39HQ (AHU-00182)</div>
                        </div>


                    </div>


                    <div style="margin-top:24px;">
                        <strong>Reported Issue</strong>
                        <div style="margin-top:8px;padding:12px;
                                    background:#f8fafc;border-radius:8px;">
                            Cooling performance failure on main AHU.
                        </div>
                    </div>


                    <div style="margin-top:24px;">
                        <strong>Technician Remarks</strong>
                        <div style="margin-top:8px;padding:12px;
                                    background:#eff6ff;
                                    border-left:4px solid #2563eb;
                                    border-radius:8px;">
                            Fan belt inspected. Air flow reduced due to worn-out belt.
                            Replacement recommended.
                        </div>
                    </div>


                    <div style="margin-top:24px;">
                        <strong>Resolution Remarks</strong>
                        <div style="margin-top:8px;padding:12px;
                                    background:#ecfdf5;
                                    border-left:4px solid #10b981;
                                    border-radius:8px;">
                            Awaiting resolution submission.
                        </div>
                    </div>


                    <div style="margin-top:24px;">
                        <strong>Comments</strong>
                        <div style="margin-top:8px;padding:12px;
                                    background:#f8fafc;border-radius:8px;">
                            • Ticket created by Facilities Manager<br>
                            • Technician assigned<br>
                            • Site inspection completed
                        </div>
                    </div>


                </div>


          
            </div>
        </div>
    `;


    document.body.appendChild(modal);
}

    function globalSearch() {
    const searchInput = document.querySelector("[data-filter]");
    const search = searchInput ? searchInput.value : "";
    const filterValues = Array.from(document.querySelectorAll("[data-column]"))
      .reduce((values, filter) => {
        values[filter.dataset.column] = filter.value;
        return values;
      }, {});
    const paginationButton = document.querySelector(".pagination [data-pagination-id]");
    const controller = paginationButton &&
      paginationControllers.get(paginationButton.dataset.paginationId);

    if (controller) {
      const activeElement = document.activeElement;
      const searchWasFocused = searchInput && activeElement === searchInput;
      const activeColumn = activeElement &&
        document.activeElement.getAttribute("data-column");
      const selectionStart = searchWasFocused
        ? searchInput.selectionStart
        : null;
      controller.setFilters(search, filterValues);

      const refreshedSearch = document.querySelector("[data-filter]");
      if (refreshedSearch) {
        refreshedSearch.value = search;
        if (searchWasFocused) {
          refreshedSearch.focus();
          if (selectionStart !== null) {
            refreshedSearch.setSelectionRange(selectionStart, selectionStart);
          }
        }
      }
      Object.keys(filterValues).forEach(column => {
        const refreshedFilter = Array.from(document.querySelectorAll("[data-column]"))
          .find(filter => filter.dataset.column === column);
        if (refreshedFilter) refreshedFilter.value = filterValues[column];
      });
      if (activeColumn !== null) {
        const refreshedActiveFilter = Array.from(document.querySelectorAll("[data-column]"))
          .find(filter => filter.dataset.column === activeColumn);
        if (refreshedActiveFilter) refreshedActiveFilter.focus();
      }
      return;
    }




    const filters =
        document.querySelectorAll("[data-column]");




    // TABLE PAGES
    const rows =
        document.querySelectorAll("tbody tr");




    if (rows.length > 0) {




        rows.forEach(row => {




            let visible =
                row.textContent.toLowerCase().includes(search);




            filters.forEach(filter => {




                const value =
                    filter.value.toLowerCase();




                if (!value) return;




                const column =
                    parseInt(filter.dataset.column);




                if (
                    !row.cells[column]?.textContent
                        .toLowerCase()
                        .includes(value)
                ) {
                    visible = false;
                }




            });




            row.style.display =
                visible ? "" : "none";




        });




        return;
    }




    // CARD PAGES (Assets, Locations, etc.)
    const cards =
        document.querySelectorAll("[data-card]");




    cards.forEach(card => {




        let visible =
            card.textContent.toLowerCase().includes(search);




        filters.forEach(filter => {




            const value =
                filter.value.toLowerCase();




            if (!value) return;




            const field =
                filter.dataset.column;




            const cardValue =
                (card.dataset[field] || "")
                    .toLowerCase();




            if (!cardValue.includes(value)) {
                visible = false;
            }




        });




        card.style.display =
            visible ? "" : "none";




    });




}


// /Open create ticke model/
  function openCreateTicketModal() {


    const modal = document.getElementById("modalContainer");


    modal.innerHTML = `
        <div class="modal-overlay">
            <div class="modal large">


                <div class="modal-header">
                    <h2>Create Service Request</h2>


                    <button class="close-btn"
                        onclick="App.closeModal()">
                        ✕
                    </button>
                </div>


                <div class="modal-body">


                    <div class="form-group">
                        <label>Equipment</label>


                        <select id="ticketAsset">
                            ${Store.assets.map(asset => `
                                <option value="${asset.id}">
                                    ${asset.id} - ${asset.category}
                                </option>
                            `).join("")}
                        </select>
                    </div>


                    <div class="form-group">
                        <label>Priority</label>


                        <select id="ticketPriority">
                            <option>Low</option>
                            <option>Medium</option>
                            <option>High</option>
                            <option>Critical</option>
                        </select>
                    </div>


                    <div class="form-group">
                        <label>Issue Title</label>
                        <input id="ticketTitle" type="text">
                    </div>


                    <div class="form-group">
                        <label>Description</label>
                        <textarea id="ticketDescription"></textarea>
                    </div>


                </div>


                <div class="modal-footer">


                    <button class="btn"
                        onclick="App.closeModal()">
                        Cancel
                    </button>


                    <button class="btn primary"
                        onclick="App.saveTicket()">
                        Create Ticket
                    </button>


                </div>


            </div>
        </div>
    `;


    modal.style.display = "flex";
  }




  /*save ticket asset*/
  function saveTicket() {


    const assetId = document.getElementById("ticketAsset").value;
     asset = Store.assets.find(a => a.id === assetId);


    const ticket = {
    id: "TKT-" + (2404 + Store.tickets.length + 1),

    title: document.getElementById("ticketTitle").value,

    customerId: "CUST-00128",
    customerName: "Apex Retail Group",

    siteId: asset?.siteId || "",
    locationName: asset?.locationName || "",

    assetId: asset?.id || "",
    assetName: asset
        ? `${asset.manufacturer} ${asset.model}`
        : "",

    vendorId: "PCS-001",
    vendorName: "Pacific Climate Services",

    priority: document.getElementById("ticketPriority").value,

    slaState: "Within SLA",
    slaDueText: "Within 4h Target",

    status: "Open",

    technicianId: "",
    technicianName: "Unassigned",

    raisedBy: "End User",

    createdAt: new Date().toLocaleString(),

    description: document.getElementById("ticketDescription").value,

    resolutionNotes: ""
};

Store.tickets.unshift(ticket);

alert(`Ticket ${ticket.id} Created Successfully`);

closeModal();
if (!refreshPagination("tickets", Store.tickets)) Router.loadCurrentPage();
  }
  
  /** Init */
  async function init() {
    const user = await Auth.refreshSession();
    if (!user) {
      window.location.replace('index.html');
      return;
    }

    // Header
    const nameEl = document.getElementById('currentUserName');
    const roleEl = document.getElementById('currentUserRole');
    const avatarEl = document.getElementById('userAvatar');
    if (nameEl) nameEl.textContent = user.fullName || user.name || '';
    if (roleEl) roleEl.textContent = user.role || '';
    if (avatarEl) avatarEl.textContent = user.avatar || (user.name || 'U').slice(0, 2).toUpperCase();

    // Logout
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) logoutBtn.addEventListener('click', logout);

    // Mobile menu
    setupMobileMenu();

    // Sidebar
    renderSidebar();

    // Default hash
    if (!location.hash) {
      const defaultPage = Auth.getDefaultLandingPage(user);
      location.hash = '#' + defaultPage;
    }

    // Listen for hash changes to update active link
    window.addEventListener('hashchange', updateActiveLink);

    // Load page
    setupPaginationControls();
    Router.loadCurrentPage();
  }
function openAssetDetail(assetId) {


    const asset = Store.assets.find(a => a.id === assetId);


    if (!asset) {
        alert("Asset not found");
        return;
    }


    Store.selectedAsset = asset;


    navigate("asset-detail");
}
return {
    init,
    navigate,
    logout,
    openCreateAssetModal,
    saveAsset,
    openRepairHistory,
    closeModal,
    showToast,
    toast: showToast,
    saveResolution,
    openResolveModal,
    openTicketDetail,
    openAssetDetail,
    openCreateTicketModal,
    globalSearch,
    refreshPagination,
    saveTicket,
    createPagination,
};

})();
