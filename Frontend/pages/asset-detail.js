window.Pages = window.Pages || {};

const TechnicianData = {
    "Casey Reed": {
        employeeId: "TECH-1001",
        phone: "+91 9876543210",
        email: "casey.reed@vendor.com",
        vendor: "Pacific Climate Services",
        experience: "6 Years",
        skills: "HVAC Maintenance, Chiller Systems, Refrigeration",
        status: "Available"
    },


    "Jordan Patel": {
        employeeId: "TECH-1002",
        phone: "+91 9876543211",
        email: "jordan.patel@vendor.com",
        vendor: "Pacific Climate Services",
        experience: "8 Years",
        skills: "HVAC Controls, Preventive Maintenance, VRF Systems",
        status: "On Site"
    }
};


const AssetDetailData = {


    serviceHistory: [
        {
            serviceDate: "01-Oct-2026",
            serviceType: "Preventive Maintenance",
            technician: "Casey Reed",
            outcome: "Completed"
        },
        {
            serviceDate: "01-Jul-2026",
            serviceType: "Quarterly Inspection",
            technician: "Jordan Patel",
            outcome: "Completed"
        }
    ],


    repairHistory: [
        {
            date: "10-Oct-2026",
            ticketId: "TKT-2408",
            issue: "Compressor Failure",
            resolution: "Relay Replaced"
        },
        {
            date: "05-Aug-2026",
            ticketId: "TKT-2281",
            issue: "Refrigerant Leak",
            resolution: "Leak Repaired"
        }
    ],


    relatedTickets: [
        {
            ticketId: "TKT-2408",
            priority: "High",
            status: "In Progress",
            sla: "At Risk",
            technician: "Casey Reed"
        },
        {
            ticketId: "TKT-2281",
            priority: "Medium",
            status: "Resolved",
            sla: "Met",
            technician: "Jordan Patel"
        }
    ],


    maintenanceRecords: [
        {
            date: "01-Oct-2026",
            type: "Quarterly PM",
            technician: "Casey Reed",
            nextDue: "01-Jan-2027"
        },
        {
            date: "01-Jul-2026",
            type: "Quarterly PM",
            technician: "Jordan Patel",
            nextDue: "01-Oct-2026"
        }
    ]
};


/* ==========================================
   PAGE
========================================== */


window.Pages["asset-detail"] = function () {


    const a = Store.selectedAsset;


    if (!a) {
        return `
            <div class="card">
                <h2>Asset Not Found</h2>
            </div>
        `;
    }


    const serviceRows =
        AssetDetailData.serviceHistory.map(item => [
            item.serviceDate,
            item.serviceType,
            item.technician,
            a.customerName || "Apex Corporation",
            item.outcome
        ]);


    const repairRows =
        AssetDetailData.repairHistory.map(item => [
            item.date,
            item.ticketId,
            item.issue,
            item.resolution
        ]);


    const relatedTicketRows =
        AssetDetailData.relatedTickets.map(item => [
            item.ticketId,
            item.priority,
            item.status,
            item.sla,
            item.technician,
            `
            <button
                class="btn primary"
                onclick="openTechnicianModal('${item.technician}')">
                Technical Details
            </button>
            `
        ]);


    const maintenanceRows =
        AssetDetailData.maintenanceRecords.map(item => [
            item.date,
            item.type,
            item.technician,
            item.nextDue
        ]);


    return `


    <div style="margin-bottom:20px;">
        <a href="#"
           onclick="App.navigate('assets');return false;">
           ← Back to Assets
        </a>
    </div>


    <article class="card">


        <div style="
            display:flex;
            justify-content:space-between;
            align-items:center;
            flex-wrap:wrap;
            gap:12px;
        ">
            <div>
                <h1>${a.id}</h1>
                <p>${a.category}</p>
            </div>


            ${UI.badge(a.status)}
        </div>


    </article>


    <div class="grid two-col">


        <article class="card">


            <h2>Asset Information</h2>


            <div class="info-grid">


                <div class="info-item">
                    <small>Asset ID</small>
                    <strong>${a.id}</strong>
                </div>


                <div class="info-item">
                    <small>Asset Type</small>
                    <strong>${a.category}</strong>
                </div>


                <div class="info-item">
                    <small>Status</small>
                    <strong>${a.status}</strong>
                </div>


                <div class="info-item">
                    <small>Location</small>
                    <strong>${a.locationName}</strong>
                </div>


                <div class="info-item">
                    <small>Customer</small>
                    <strong>${a.customerName || "Apex Corporation"}</strong>
                </div>


                <div class="info-item">
                    <small>Asset Criticality</small>
                    <strong>${a.criticality || "High"}</strong>
                </div>


            </div>


        </article>


        <article class="card">


            <h2>Equipment Specifications</h2>


            <div class="info-grid">


                <div class="info-item">
                    <small>Manufacturer</small>
                    <strong>${a.manufacturer}</strong>
                </div>


                <div class="info-item">
                    <small>Refrigerant</small>
                    <strong>${a.refrigerant}</strong>
                </div>


                <div class="info-item">
                    <small>Voltage</small>
                    <strong>${a.voltage || "460V"}</strong>
                </div>


                <div class="info-item">
                    <small>Phase</small>
                    <strong>${a.phase || "3 Phase"}</strong>
                </div>


                <div class="info-item">
                    <small>Capacity</small>
                    <strong>${a.capacity || "100 Ton"}</strong>
                </div>


            </div>


        </article>


    </div>


    <article class="card">


        <h2>Installation Details</h2>


        <div class="info-grid">


            <div class="info-item">
                <small>Installation Date</small>
                <strong>${a.installationDate || "15-Jan-2024"}</strong>
            </div>


            <div class="info-item">
                <small>Commissioning Date</small>
                <strong>${a.commissioningDate || "20-Jan-2024"}</strong>
            </div>


            <div class="info-item">
                <small>Installed By</small>
                <strong>${a.installedBy || "Pacific Climate Services"}</strong>
            </div>


        </div>


    </article>


    <article class="card">
        <h2>Service History</h2>
        <div style="overflow-x:auto;">
            ${UI.table(
                ["Service Date","Service Type","Technician","Customer","Outcome"],
                serviceRows
            )}
        </div>
    </article>


    <article class="card">
        <h2>Repair History</h2>
        <div style="overflow-x:auto;">
            ${UI.table(
                ["Date","Ticket ID","Issue","Resolution"],
                repairRows
            )}
        </div>
    </article>


    <article class="card">
        <h2>Related Tickets</h2>
        <div style="overflow-x:auto;">
            ${UI.table(
                [
                    "Ticket ID",
                    "Priority",
                    "Status",
                    "SLA",
                    "Assigned Technician",
                    "Details"
                ],
                relatedTicketRows
            )}
        </div>
    </article>


    <article class="card">
        <h2>Maintenance Records</h2>
        <div style="overflow-x:auto;">
            ${UI.table(
                [
                    "Date",
                    "Maintenance Type",
                    "Technician",
                    "Next Due"
                ],
                maintenanceRows
            )}
        </div>
    </article>
    `;
};


/* ==========================================
   MODAL FUNCTIONS
========================================== */


window.openTechnicianModal = function(name) {


    const tech = TechnicianData[name];


    if (!tech) return;


    const modal =
        document.getElementById("modalContainer");


    modal.innerHTML = `
        <div class="modal-overlay">


            <div class="modal">


                <div class="modal-header">


                    <h2>Technician Details</h2>


                    <button
                        class="close-btn"
                        onclick="closeModal()">
                        ✕
                    </button>


                </div>


                <div class="modal-body">


                    <div class="info-grid">


                        <div class="info-item">
                            <small>Name</small>
                            <strong>${name}</strong>
                        </div>


                        <div class="info-item">
                            <small>Employee ID</small>
                            <strong>${tech.employeeId}</strong>
                        </div>


                        <div class="info-item">
                            <small>Phone</small>
                            <strong>${tech.phone}</strong>
                        </div>


                        <div class="info-item">
                            <small>Email</small>
                            <strong>${tech.email}</strong>
                        </div>


                        <div class="info-item">
                            <small>Vendor</small>
                            <strong>${tech.vendor}</strong>
                        </div>


                        <div class="info-item">
                            <small>Experience</small>
                            <strong>${tech.experience}</strong>
                        </div>


                        <div class="info-item">
                            <small>Skills</small>
                            <strong>${tech.skills}</strong>
                        </div>


                        <div class="info-item">
                            <small>Status</small>
                            <strong>${tech.status}</strong>
                        </div>


                    </div>


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
};


window.closeModal = function() {


    const modal =
        document.getElementById("modalContainer");


    if (!modal) return;


    modal.style.display = "none";
    modal.innerHTML = "";
};