/**
 * HVAC Connect — Central Mock Data Store
 * In-memory state holding all entity records and helper methods for interactive UI actions.
 */

const Store = {
    vendors: [
        { id: "PCS-001", name: "Pacific Climate Services", region: "California", status: "Active", phase: "Phase 1 · Coexist", progress: "82%", contact: "Jordan Lee (ops@pacificclimate.example)", sitesCount: 124 },
        { id: "NCG-014", name: "Northwest Cooling Group", region: "Washington", status: "Active", phase: "Phase 1 · Validation", progress: "54%", contact: "Dave Miller (d.miller@nwcooling.example)", sitesCount: 86 },
        { id: "DAS-008", name: "Desert Air Systems", region: "Arizona", status: "Active", phase: "Phase 2 · Planning", progress: "91%", contact: "Sarah Vance (s.vance@desertair.example)", sitesCount: 52 },
        { id: "CM-022", name: "Coastal Mechanical", region: "Oregon", status: "Pending Onboarding", phase: "Discovery", progress: "18%", contact: "Mark Olsen (m.olsen@coastalmech.example)", sitesCount: 19 },
        { id: "CM-022", name: "Coastal Mechanical", region: "Oregon", status: "Pending Onboarding", phase: "Discovery", progress: "18%", contact: "Mark Olsen (m.olsen@coastalmech.example)", sitesCount: 19 }
    ],

    repairHistory: [
    {
        id: "RH-001",
        assetId: "AHU-00182",
        ticketId: "TKT-2408",
        serviceDate: "Nov 09, 2026",
        serviceType: "Corrective Maintenance",
        title: "Cooling Performance Failure",
        status: "Resolved",
        priority: "High",
        technicianId: "TECH-104",
        technicianName: "Casey Reed",
        vendorId: "PCS-001",
        vendorName: "Pacific Climate Services",
        findings: "Supply fan belt worn and economizer damper stuck at 10% position.",
        actionTaken: "Replaced supply fan belt and recalibrated economizer actuator.",
        resolutionNotes: "Airflow restored and discharge temperature returned within design parameters.",
        laborHours: 2.5,
        partsUsed: ["Fan Belt B-52", "Belimo Actuator Calibration Kit"]
    },
    {
        id: "RH-002",
        assetId: "AHU-00182",
        ticketId: "TKT-2315",
        serviceDate: "Aug 16, 2026",
        serviceType: "Preventive Maintenance",
        title: "Quarterly PM Inspection",
        status: "Completed",
        priority: "Low",
        technicianId: "TECH-104",
        technicianName: "Casey Reed",
        vendorId: "PCS-001",
        vendorName: "Pacific Climate Services",
        findings: "Dirty return air filters and moderate dust accumulation on cooling coils.",
        actionTaken: "Replaced filters and cleaned evaporator coil assembly.",
        resolutionNotes: "Unit operating within expected airflow and cooling performance.",
        laborHours: 1.5,
        partsUsed: ["MERV-13 Filter Set"]
    },
    {
        id: "RH-003",
        assetId: "AHU-00182",
        ticketId: "TKT-2214",
        serviceDate: "Mar 28, 2026",
        serviceType: "Inspection",
        title: "Temperature Alarm Investigation",
        status: "Completed",
        priority: "Medium",
        technicianId: "TECH-118",
        technicianName: "Jordan Patel",
        vendorId: "PCS-001",
        vendorName: "Pacific Climate Services",
        findings: "Faulty discharge temperature sensor reporting intermittent values.",
        actionTaken: "Replaced sensor and recalibrated BAS point mapping.",
        resolutionNotes: "Temperature alarms cleared and monitoring restored.",
        laborHours: 2,
        partsUsed: ["Discharge Air Sensor"]
    },
    {
        id: "RH-004",
        assetId: "AHU-00182",
        ticketId: "TKT-2120",
        serviceDate: "Dec 11, 2025",
        serviceType: "Emergency Repair",
        title: "Supply Fan Failure",
        status: "Resolved",
        priority: "Critical",
        technicianId: "TECH-104",
        technicianName: "Casey Reed",
        vendorId: "PCS-001",
        vendorName: "Pacific Climate Services",
        findings: "Fan motor winding failure caused complete airflow loss.",
        actionTaken: "Installed replacement motor and verified electrical load readings.",
        resolutionNotes: "Equipment returned to full operational capacity.",
        laborHours: 4.5,
        partsUsed: ["5HP Fan Motor", "Motor Pulley Assembly"]
    }
],

    customers: [
        { id: "CUST-00128", name: "Apex Retail Group", type: "Commercial / Retail", primaryContact: "Morgan Ellis", email: "m.ellis@apex.example", phone: "+1 (408) 555-0128", address: "450 Market St, San Jose, CA", accountsCount: 3, status: "Active" },
        { id: "CUST-00129", name: "Westline Medical Center", type: "Healthcare", primaryContact: "Dr. Evelyn Reed", email: "facilities@westlinemed.example", phone: "+1 (206) 555-0199", address: "1200 Health Way, Bellevue, WA", accountsCount: 2, status: "Active" },
        { id: "CUST-00130", name: "Canyon Business Parks", type: "Commercial Office", primaryContact: "Rachel Clark", email: "rclark@canyonbp.example", phone: "+1 (602) 555-0814", address: "8800 Desert Ridge Pkwy, Phoenix, AZ", accountsCount: 1, status: "Active" },
        { id: "CUST-00131", name: "Northstar Hospitality", type: "Hotel / Lodging", primaryContact: "James Wilson", email: "j.wilson@northstar.example", phone: "+1 (503) 555-0422", address: "500 Marina Way, Portland, OR", accountsCount: 2, status: "Review" },
        { id: "CUST-00131", name: "Northstar Hospitality", type: "Hotel / Lodging", primaryContact: "James Wilson", email: "j.wilson@northstar.example", phone: "+1 (503) 555-0422", address: "500 Marina Way, Portland, OR", accountsCount: 2, status: "Review" }
    ],

    brandAccounts: [
        { canonicalId: "CUST-00128", vendorId: "PCS-001", vendorName: "Pacific Climate Services", localAccountNo: "A-104", status: "Active" },
        { canonicalId: "CUST-00128", vendorId: "NCG-014", vendorName: "Northwest Cooling Group", localAccountNo: "B-209", status: "Active" },
        { canonicalId: "CUST-00128", vendorId: "DAS-008", vendorName: "Desert Air Systems", localAccountNo: "D-077", status: "Active" },
        { canonicalId: "CUST-00129", vendorId: "PCS-001", vendorName: "Pacific Climate Services", localAccountNo: "A-190", status: "Active" },
        { canonicalId: "CUST-00129", vendorId: "NCG-014", vendorName: "Northwest Cooling Group", localAccountNo: "B-312", status: "Active" },
        { canonicalId: "CUST-00130", vendorId: "DAS-008", vendorName: "Desert Air Systems", localAccountNo: "D-142", status: "Active" },
        { canonicalId: "CUST-00131", vendorId: "NCG-014", vendorName: "Northwest Cooling Group", localAccountNo: "B-404", status: "Review" },
        { canonicalId: "CUST-00131", vendorId: "CM-022", vendorName: "Coastal Mechanical", localAccountNo: "C-088", status: "Review" }
    ],

    locations: [
        { id: "LOC-00812", name: "Apex — San Jose HQ", customerId: "CUST-00128", customerName: "Apex Retail Group", vendorId: "PCS-001", vendorName: "Pacific Climate Services", city: "San Jose, CA", address: "450 Market Street, San Jose, CA", timezone: "America/Los_Angeles", operatingHours: "Mon–Fri, 8:00 AM–6:00 PM", accessNotes: "Check in with security at lobby desk. Service elevator on 2nd loading dock.", assetCount: 18, status: "Active" },
        { id: "LOC-00813", name: "Apex — Oakland Store #14", customerId: "CUST-00128", customerName: "Apex Retail Group", vendorId: "PCS-001", vendorName: "Pacific Climate Services", city: "Oakland, CA", address: "120 Broadway, Oakland, CA", timezone: "America/Los_Angeles", operatingHours: "Mon–Sun, 7:00 AM–10:00 PM", accessNotes: "Roof hatch accessed through stockroom B; padlock combination 4821.", assetCount: 7, status: "Active" },
        { id: "LOC-00901", name: "Westline — Bellevue Medical Plaza", customerId: "CUST-00129", customerName: "Westline Medical Center", vendorId: "NCG-014", vendorName: "Northwest Cooling Group", city: "Bellevue, WA", address: "1200 Health Way, Bellevue, WA", timezone: "America/Los_Angeles", operatingHours: "24/7 Facility", accessNotes: "Critical facility. Escort required from engineering dept for chiller plant access.", assetCount: 12, status: "Active" },
        { id: "LOC-00945", name: "Canyon — Phoenix Campus Bldg 1", customerId: "CUST-00130", customerName: "Canyon Business Parks", vendorId: "DAS-008", vendorName: "Desert Air Systems", city: "Phoenix, AZ", address: "8800 Desert Ridge Pkwy, Phoenix, AZ", timezone: "America/Phoenix", operatingHours: "Mon–Fri, 6:00 AM–7:00 PM", accessNotes: "Keycard needed from building management office suite 100.", assetCount: 24, status: "Review" }
    ],

    locationHistory: [
        { id: 1, siteId: "LOC-00812", date: "Today, 10:42 AM", title: "Asset information updated", detail: "Local Admin · Serial number confirmed for AHU-00182", type: "AssetUpdate" },
        { id: 2, siteId: "LOC-00812", date: "Nov 06, 2026", title: "Primary location contact changed", detail: "Data import job #IMP-204 · Assigned to Morgan Ellis", type: "ContactChange" },
        { id: 3, siteId: "LOC-00812", date: "Oct 28, 2026", title: "Site registered in canonical model", detail: "System import from Pacific Climate legacy database", type: "SiteCreated" }
    ],

    assets: [
        { id: "AHU-00182", category: "Air Handling Unit", categoryCode: "AHU", manufacturer: "Carrier", model: "39HQ", serial: "SN-CR-992144", siteId: "LOC-00812", locationName: "Apex — San Jose HQ", refrigerant: "N/A (Chilled Water)", installDate: "2021-03-15", warrantyEnd: "2027-04-12", status: "Active" },
        { id: "CH-00410", category: "Chiller", categoryCode: "CH", manufacturer: "Trane", model: "CGAM 100-Ton", serial: "SN-TR-448102", siteId: "LOC-00812", locationName: "Apex — San Jose HQ", refrigerant: "R-410A", installDate: "2020-08-10", warrantyEnd: "2026-12-01", status: "Active" },
        { id: "RTU-00291", category: "Rooftop Unit", categoryCode: "RTU", manufacturer: "Daikin", model: "DPS Packaged RTU", serial: "SN-DK-330198", siteId: "LOC-00813", locationName: "Apex — Oakland Store #14", refrigerant: "R-410A", installDate: "2022-01-20", warrantyEnd: "2028-02-20", status: "Active" },
        { id: "BLR-00501", category: "Boiler", categoryCode: "BLR", manufacturer: "Burnham Commercial", model: "Series 4S Steam", serial: "SN-BN-789012", siteId: "LOC-00812", locationName: "Apex — San Jose HQ", refrigerant: "N/A (Natural Gas)", installDate: "2019-11-05", warrantyEnd: "2025-11-05", status: "Active" },
        { id: "WIK-00302", category: "Walk-in Cooler/Freezer", categoryCode: "WIK", manufacturer: "Kolpak", model: "ColdZone Dual-Temp", serial: "SN-KP-102938", siteId: "LOC-00813", locationName: "Apex — Oakland Store #14", refrigerant: "R-404A", installDate: "2023-05-18", warrantyEnd: "2029-05-18", status: "Active" },
        { id: "RCK-00712", category: "Refrigeration Rack", categoryCode: "RCK", manufacturer: "Hussmann", model: "Protocol Multi-Compressor", serial: "SN-HS-998811", siteId: "LOC-00813", locationName: "Apex — Oakland Store #14", refrigerant: "CO2 (R-744)", installDate: "2022-09-14", warrantyEnd: "2027-09-14", status: "Active" },
        { id: "FCU-00917", category: "Fan Coil Unit", categoryCode: "FCU", manufacturer: "Mitsubishi", model: "PKFY Wall Mounted", serial: "SN-MB-554412", siteId: "LOC-00901", locationName: "Westline — Bellevue Medical Plaza", refrigerant: "R-410A", installDate: "2018-04-12", warrantyEnd: "2026-10-15", status: "Service Review" }
    ],

    technicians: [
        { id: "TECH-104", name: "Casey Reed", email: "casey.tech@pacificclimate.com", vendorId: "PCS-001", vendorName: "Pacific Climate Services", specialization: "HVAC / Chillers", activeTickets: 3, status: "Active", phone: "+1 (408) 555-0301" },
        { id: "TECH-118", name: "Jordan Patel", email: "jordan.tech@pacificclimate.com", vendorId: "PCS-001", vendorName: "Pacific Climate Services", specialization: "Commercial Refrigeration", activeTickets: 2, status: "Active", phone: "+1 (408) 555-0302" },
        { id: "TECH-202", name: "Morgan Diaz", email: "m.diaz@nwcooling.com", vendorId: "NCG-014", vendorName: "Northwest Cooling Group", specialization: "HVAC / AHU Systems", activeTickets: 2, status: "Active", phone: "+1 (206) 555-0303" },
        { id: "WORK-044", name: "Taylor Brooks", email: "t.brooks@desertair.com", vendorId: "DAS-008", vendorName: "Desert Air Systems", specialization: "Boiler & Heating / Installer", activeTickets: 1, status: "Active", phone: "+1 (602) 555-0304" },
        { id: "WORK-045", name: "Taylor Brooks", email: "t.brooks@desertair.com", vendorId: "DAS-008", vendorName: "Desert Air Systems", specialization: "Boiler & Heating / Installer", activeTickets: 1, status: "Active", phone: "+1 (602) 555-0304" }
    ],

    tickets: [
        {
            id: "TKT-2408",
            title: "Cooling performance failure on main AHU",
            customerId: "CUST-00128",
            customerName: "Apex Retail Group",
            siteId: "LOC-00812",
            locationName: "Apex — San Jose HQ",
            assetId: "AHU-00182",
            assetName: "Carrier 39HQ",
            vendorId: "PCS-001",
            vendorName: "Pacific Climate Services",
            priority: "High",
            slaState: "Breached",
            slaDueText: "Breached by 2h 15m",
            status: "In Progress",
            technicianId: "TECH-104",
            technicianName: "Casey Reed",
            raisedBy: "Morgan Ellis (Facilities Manager)",
            createdAt: "Nov 09, 2026 · 08:20 AM",
            description: "2nd floor south wing reports temperatures exceeding 78°F. Primary supply fan operates but air flow velocity is suboptimal.",
            resolutionNotes: ""
        },
        {
            id: "TKT-2409",
            title: "Water leakage observed from ceiling FCU",
            customerId: "CUST-00129",
            customerName: "Westline Medical Center",
            siteId: "LOC-00901",
            locationName: "Westline — Bellevue Medical Plaza",
            assetId: "FCU-00917",
            assetName: "Mitsubishi PKFY",
            vendorId: "PCS-001",
            vendorName: "Pacific Climate Services",
            priority: "Medium",
            slaState: "At Risk",
            slaDueText: "00:42 remaining",
            status: "Open",
            technicianId: "TECH-118",
            technicianName: "Jordan Patel",
            raisedBy: "Dr. Evelyn Reed",
            createdAt: "Nov 11, 2026 · 10:15 AM",
            description: "Condensate drain pan appears blocked. Drip tray overflowing near examination room 3.",
            resolutionNotes: ""
        },
        {
            id: "TKT-2410",
            title: "Supermarket refrigeration rack high pressure alarm",
            customerId: "CUST-00128",
            customerName: "Apex Retail Group",
            siteId: "LOC-00813",
            locationName: "Apex — Oakland Store #14",
            assetId: "RCK-00712",
            assetName: "Hussmann Protocol Rack",
            vendorId: "PCS-001",
            vendorName: "Pacific Climate Services",
            priority: "Critical",
            slaState: "At Risk",
            slaDueText: "01:15 remaining",
            status: "Assigned",
            technicianId: "TECH-104",
            technicianName: "Casey Reed",
            raisedBy: "Store Manager",
            createdAt: "Nov 11, 2026 · 11:30 AM",
            description: "Rack B compressor group 2 tripped on high head pressure. Dairy display cases rising toward 42°F threshold.",
            resolutionNotes: ""
        },
        {
            id: "TKT-2411",
            title: "Annual boiler combustion efficiency test",
            customerId: "CUST-00128",
            customerName: "Apex Retail Group",
            siteId: "LOC-00812",
            locationName: "Apex — San Jose HQ",
            assetId: "BLR-00501",
            assetName: "Burnham Commercial 4S",
            vendorId: "PCS-001",
            vendorName: "Pacific Climate Services",
            priority: "Low",
            slaState: "Within SLA",
            slaDueText: "24h remaining",
            status: "Open",
            technicianId: null,
            technicianName: "Unassigned",
            raisedBy: "Scheduled Maintenance Watchdog",
            createdAt: "Nov 10, 2026 · 09:00 AM",
            description: "Regulatory annual flue gas analysis, burner tune-up, and safety relief valve inspection.",
            resolutionNotes: ""
        },
        {
            id: "TKT-2407",
            title: "Rooftop unit abnormal vibration noise",
            customerId: "CUST-00128",
            customerName: "Apex Retail Group",
            siteId: "LOC-00813",
            locationName: "Apex — Oakland Store #14",
            assetId: "RTU-00291",
            assetName: "Daikin DPS RTU",
            vendorId: "NCG-014",
            vendorName: "Northwest Cooling Group",
            priority: "Medium",
            slaState: "Within SLA",
            slaDueText: "Met Target",
            status: "Resolved",
            technicianId: "TECH-202",
            technicianName: "Morgan Diaz",
            raisedBy: "Facilities Desk",
            createdAt: "Nov 08, 2026 · 02:15 PM",
            description: "Rumbling vibration heard in customer aisle 6 during compressor stage 2 startup.",
            resolutionNotes: "Inspected blower assembly. Found worn drive belt and loose motor mount bolt. Replaced belt (Size B-52), torqued mounts to OEM spec. Noise eliminated. Operating currents verified within normal range."
        },
        {
            id: "TKT-2401",
            title: "Filter replacement and quarterly inspection",
            customerId: "CUST-00130",
            customerName: "Canyon Business Parks",
            siteId: "LOC-00945",
            locationName: "Canyon — Phoenix Campus",
            assetId: "CH-00410",
            assetName: "Trane CGAM",
            vendorId: "DAS-008",
            vendorName: "Desert Air Systems",
            priority: "Low",
            slaState: "Within SLA",
            slaDueText: "Met Target",
            status: "Closed",
            technicianId: "WORK-044",
            technicianName: "Taylor Brooks",
            raisedBy: "Preventive Schedule",
            createdAt: "Nov 02, 2026 · 08:00 AM",
            description: "Quarterly filter change and condenser coil wash down.",
            resolutionNotes: "All 16 pleated filters replaced. Condenser coils power-washed. System running at optimal subcooling."
        }
    ],

    ticketComments: [
        { ticketId: "TKT-2408", author: "Casey Reed", role: "Technician", time: "Nov 09 · 09:15 AM", text: "Arrived on roof site. Ambient temperature 84°F. Checking static pressure and actuator linkages.", isInternal: false },
        { ticketId: "TKT-2408", author: "Taylor Chen", role: "Coordinator", time: "Nov 09 · 09:30 AM", text: "Customer facility manager notified of technician arrival on roof.", isInternal: true },
        { ticketId: "TKT-2408", author: "Casey Reed", role: "Technician", time: "Nov 09 · 10:45 AM", text: "Found economizer damper stuck at 10% outside air. Belt slippage on return fan. Replacing belt and recalibrating Belimo actuator.", isInternal: false },
        { ticketId: "TKT-2409", author: "Taylor Chen", role: "Coordinator", time: "Nov 11 · 10:20 AM", text: "Ticket flagged At Risk. Due within 42 minutes. Dispatched Jordan Patel with high priority.", isInternal: true }
    ],

    duplicateCandidates: [
        {
            id: "DUP-001",
            entityType: "Customer",
            candidateName: "Apex Retail",
            existingName: "Apex Retail Group",
            candidateSource: "Northwest Cooling Group (NCG-014)",
            existingSource: "Pacific Climate Services (PCS-001)",
            candidateAccountNo: "B-209",
            existingAccountNo: "A-104",
            candidateEmail: "ops@apexretail.example",
            existingEmail: "m.ellis@apex.example",
            candidateAddress: "120 Broadway, Oakland, CA",
            existingAddress: "450 Market St, San Jose, CA",
            confidenceScore: 94,
            confidenceLevel: "High",
            matchReason: "Direct customer name match + matching corporate web domain @apex.example",
            importJob: "IMP-204",
            status: "Pending Review"
        },
        {
            id: "DUP-002",
            entityType: "Customer",
            candidateName: "Westline Med.",
            existingName: "Westline Medical Center",
            candidateSource: "Desert Air Systems (DAS-008)",
            existingSource: "Pacific Climate Services (PCS-001)",
            candidateAccountNo: "D-077",
            existingAccountNo: "A-190",
            candidateEmail: "billing@westlinemed.example",
            existingEmail: "facilities@westlinemed.example",
            candidateAddress: "Bellevue Medical Plaza",
            existingAddress: "1200 Health Way, Bellevue, WA",
            confidenceScore: 78,
            confidenceLevel: "Medium",
            matchReason: "Normalized name substring match ('Westline Med') + identical city (Bellevue)",
            importJob: "IMP-201",
            status: "Pending Review"
        }
    ],

    importJobs: [
        { id: "IMP-204", vendor: "Pacific Climate Services", entity: "Customers & Sites", fileName: "pcs_customers_nov.csv", uploaded: "Nov 08, 2026", status: "Completed", rows: "320", errors: "0" },
        { id: "IMP-203", vendor: "Northwest Cooling Group", entity: "Assets / Equipment", fileName: "ncg_assets_q4.csv", uploaded: "Nov 07, 2026", status: "Review", rows: "142", errors: "6" },
        { id: "IMP-202", vendor: "Desert Air Systems", entity: "Contracts", fileName: "das_service_agreements.csv", uploaded: "Nov 05, 2026", status: "Failed", rows: "58", errors: "12" }
    ],

    onboardingChecklists: [
        { id: 1, vendorId: "PCS-001", phase: "Phase 1", task: "Source system credentials and data owner identified", completed: true, by: "Jordan Lee" },
        { id: 2, vendorId: "PCS-001", phase: "Phase 1", task: "Customer, location and asset CSV exports received", completed: true, by: "Jordan Lee" },
        { id: 3, vendorId: "PCS-001", phase: "Phase 1", task: "Source columns mapped and validated against platform schema", completed: true, by: "Taylor Chen" },
        { id: 4, vendorId: "PCS-001", phase: "Phase 1", task: "Duplicate candidates reviewed and linked in registry", completed: true, by: "Shivansh Rastogi" },
        { id: 5, vendorId: "PCS-001", phase: "Phase 1", task: "Initial coexistence sync verified; local system remains primary", completed: true, by: "Jordan Lee" },
        { id: 6, vendorId: "DAS-008", phase: "Phase 2", task: "Data quality threshold >= 95% confirmed", completed: true, by: "Shivansh Rastogi" },
        { id: 7, vendorId: "DAS-008", phase: "Phase 2", task: "Business leadership approval and planned cutover date established", completed: true, by: "Sarah Vance" },
        { id: 8, vendorId: "DAS-008", phase: "Phase 2", task: "Technicians trained on mobile app & ticket closure", completed: false, by: null },
        { id: 9, vendorId: "DAS-008", phase: "Phase 2", task: "Legacy software transitioned to read-only archive", completed: false, by: null }
    ],

    auditLogs: [
        { time: "Nov 11 · 10:45 AM", actor: "Casey Reed", action: "Posted ticket comment", entity: "Ticket TKT-2408", scope: "Pacific Climate", outcome: "Success" },
        { time: "Nov 11 · 10:15 AM", actor: "System Watchdog", action: "Flagged SLA at risk", entity: "Ticket TKT-2409", scope: "Pacific Climate", outcome: "Warning" },
        { time: "Nov 09 · 10:42 AM", actor: "Jordan Lee", action: "Updated asset serial", entity: "Asset AHU-00182", scope: "Pacific Climate", outcome: "Success" },
        { time: "Nov 08 · 04:30 PM", actor: "Shivansh Rastogi", action: "Approved duplicate match", entity: "Customer CUST-00128", scope: "Group-wide", outcome: "Success" },
        { time: "Nov 08 · 09:18 AM", actor: "Taylor Chen", action: "Completed import job", entity: "Import #IMP-204", scope: "Pacific Climate", outcome: "Success" }
    ],

    /* --- MUTATION METHODS --- */

    addTicket(ticket) {
        const nextId = "TKT-" + (2412 + this.tickets.length);
        const newTicket = {
            id: nextId,
            createdAt: "Just now",
            slaState: "Within SLA",
            slaDueText: "Within 4h Target",
            status: ticket.technicianId ? "Assigned" : "Open",
            technicianName: ticket.technicianId ? (this.technicians.find(t => t.id === ticket.technicianId)?.name || "Assigned") : "Unassigned",
            resolutionNotes: "",
            ...ticket
        };
        this.tickets.unshift(newTicket);
        this.addAuditLog("Created service ticket", `Ticket ${nextId}`, newTicket.vendorName, "Success");
        return newTicket;
    },

    updateTicketStatus(ticketId, newStatus, message = "", workNotes = "") {
        const ticket = this.tickets.find(t => t.id === ticketId);
        if (ticket) {
            ticket.status = newStatus;
            if (newStatus === "Resolved" || newStatus === "Closed") {
                ticket.slaState = "Within SLA";
                ticket.slaDueText = "Resolved";
            }
            if (workNotes) {
                ticket.resolutionNotes = workNotes;
            }
            if (message) {
                this.addTicketComment(ticketId, message, false);
            }
            this.addAuditLog("Updated ticket status to " + newStatus, `Ticket ${ticketId}`, ticket.vendorName, "Success");
            return ticket;
        }
        return null;
    },

    addTicketComment(ticketId, text, isInternal = false) {
        const user = window.Auth ? window.Auth.getCurrentUser() : null;
        const author = user ? user.name : "System User";
        const role = user ? user.role : "Coordinator";

        const comment = {
            ticketId,
            author,
            role,
            time: "Just now",
            text,
            isInternal
        };
        this.ticketComments.push(comment);
        return comment;
    },

    resolveDuplicate(candidateId, decision, notes = "") {
        const candidate = this.duplicateCandidates.find(d => d.id === candidateId);
        if (!candidate) return false;

        candidate.status = decision === "link" ? "Linked to Customer" : decision === "separate" ? "Kept Separate" : "Deferred";
        
        if (decision === "link") {
            // Add brand account link
            this.brandAccounts.push({
                canonicalId: "CUST-00128",
                vendorId: "NCG-014",
                vendorName: "Northwest Cooling Group",
                localAccountNo: candidate.candidateAccountNo,
                status: "Active"
            });
            this.addAuditLog("Linked duplicate customer", `${candidate.candidateName} -> ${candidate.existingName}`, "Group-wide", "Success");
        } else if (decision === "separate") {
            this.addAuditLog("Created separate customer", candidate.candidateName, "Group-wide", "Success");
        }

        return true;
    },

    addAuditLog(action, entity, scope, outcome = "Success") {
        const user = window.Auth ? window.Auth.getCurrentUser() : null;
        this.auditLogs.unshift({
            time: "Just now",
            actor: user ? user.name : "System",
            action,
            entity,
            scope: scope || "Group-wide",
            outcome
        });
    }
};

window.Store = Store;

