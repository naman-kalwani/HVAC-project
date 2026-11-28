/**
 * Page: Data Import Wizard
 * PB-006, PB-024 to PB-027 / US006
 */

window.Pages = window.Pages || {};

window.ImportWizardState = window.ImportWizardState || {
    vendorId: "",
    entity: "Customers",
    batchDescription: "",
    fileName: "",
    fileSize: 0
};

/**
 * Escape dynamic values before inserting them into HTML.
 */
function escapeImportHtml(value) {
    return String(value === null || value === undefined ? "" : value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

/**
 * Navigate through the existing application router.
 */
function navigateImportPage(pageName) {
    if (
        window.App &&
        typeof window.App.navigate === "function"
    ) {
        window.App.navigate(pageName);
        return;
    }

    console.error(
        'Unable to navigate to "' +
        pageName +
        '". App.navigate is unavailable.'
    );
}

/**
 * Inline message container used on each wizard page.
 */
function getImportMessageContainer() {
    return `
        <div
            id="importWizardMessage"
            role="alert"
            aria-live="assertive"
            aria-atomic="true"
            style="display: none;"
        ></div>
    `;
}

/**
 * Display an inline error, success, warning, or information message.
 * This function never uses window.alert().
 */
function showImportMessage(message, type) {
    var messageType = type || "error";

    var messageBox = document.getElementById(
        "importWizardMessage"
    );

    if (!messageBox) {
        var stepper = document.querySelector(".stepper");

        if (!stepper) {
            console.error(message);
            return;
        }

        messageBox = document.createElement("div");
        messageBox.id = "importWizardMessage";
        messageBox.setAttribute("role", "alert");
        messageBox.setAttribute("aria-live", "assertive");
        messageBox.setAttribute("aria-atomic", "true");

        stepper.insertAdjacentElement(
            "afterend",
            messageBox
        );
    }

    var configurations = {
        error: {
            background: "#fef2f2",
            border: "#fecaca",
            color: "#991b1b",
            icon: "!",
            title: "Unable to continue"
        },
        success: {
            background: "#f0fdf4",
            border: "#bbf7d0",
            color: "#166534",
            icon: "✓",
            title: "Success"
        },
        warning: {
            background: "#fffbeb",
            border: "#fde68a",
            color: "#92400e",
            icon: "!",
            title: "Attention required"
        },
        info: {
            background: "#eff6ff",
            border: "#bfdbfe",
            color: "#1e40af",
            icon: "i",
            title: "Information"
        }
    };

    var configuration =
        configurations[messageType] ||
        configurations.error;

    messageBox.style.display = "flex";
    messageBox.style.alignItems = "flex-start";
    messageBox.style.justifyContent = "space-between";
    messageBox.style.gap = "12px";
    messageBox.style.marginTop = "16px";
    messageBox.style.marginBottom = "16px";
    messageBox.style.padding = "12px 14px";
    messageBox.style.borderRadius = "8px";
    messageBox.style.background =
        configuration.background;
    messageBox.style.border =
        "1px solid " + configuration.border;
    messageBox.style.color = configuration.color;
    messageBox.style.boxSizing = "border-box";
    messageBox.style.width = "100%";

    messageBox.innerHTML = `
        <div
            style="
                display: flex;
                align-items: flex-start;
                gap: 10px;
            "
        >
            <span
                aria-hidden="true"
                style="
                    display: inline-flex;
                    align-items: center;
                    justify-content: center;
                    flex: 0 0 22px;
                    width: 22px;
                    height: 22px;
                    margin-top: 1px;
                    border-radius: 50%;
                    background: ${configuration.color};
                    color: #ffffff;
                    font-size: 13px;
                    font-weight: 700;
                    line-height: 1;
                "
            >
                ${configuration.icon}
            </span>

            <div>
                <div
                    style="
                        margin-bottom: 2px;
                        font-size: 13px;
                        font-weight: 700;
                    "
                >
                    ${configuration.title}
                </div>

                <div
                    style="
                        font-size: 13px;
                        line-height: 1.5;
                    "
                >
                    ${escapeImportHtml(message)}
                </div>
            </div>
        </div>

        <button
            type="button"
            aria-label="Dismiss message"
            onclick="hideImportMessage()"
            style="
                flex: 0 0 auto;
                border: 0;
                padding: 0 4px;
                background: transparent;
                color: ${configuration.color};
                font-size: 20px;
                line-height: 1;
                cursor: pointer;
            "
        >
            &times;
        </button>
    `;

    messageBox.scrollIntoView({
        behavior: "smooth",
        block: "nearest"
    });
}

/**
 * Hide the current inline message.
 */
function hideImportMessage() {
    var messageBox = document.getElementById(
        "importWizardMessage"
    );

    if (!messageBox) {
        return;
    }

    messageBox.style.display = "none";
    messageBox.innerHTML = "";
}

/**
 * Mark a form control as invalid.
 */
function markImportFieldInvalid(field) {
    if (!field) {
        return;
    }

    field.setAttribute("aria-invalid", "true");
    field.style.borderColor = "#dc2626";
    field.style.boxShadow =
        "0 0 0 3px rgba(220, 38, 38, 0.12)";

    field.focus();
}

/**
 * Remove invalid styling from import controls.
 */
function clearImportFieldErrors() {
    var fieldIds = [
        "importVendorSelect",
        "importEntitySelect",
        "importFileInput"
    ];

    fieldIds.forEach(function (fieldId) {
        var field = document.getElementById(fieldId);

        if (!field) {
            return;
        }

        field.removeAttribute("aria-invalid");
        field.style.borderColor = "";
        field.style.boxShadow = "";
    });
}

/**
 * Build a basic fallback table if UI.table is unavailable.
 */
function buildImportFallbackTable(headers, rows) {
    var tableHeaders = headers
        .map(function (header) {
            return "<th>" +
                escapeImportHtml(header) +
                "</th>";
        })
        .join("");

    var tableRows = rows
        .map(function (row) {
            return `
                <tr>
                    ${row
                        .map(function (cell) {
                            return "<td>" + cell + "</td>";
                        })
                        .join("")}
                </tr>
            `;
        })
        .join("");

    return `
        <div style="overflow-x: auto;">
            <table
                style="
                    width: 100%;
                    border-collapse: collapse;
                "
            >
                <thead>
                    <tr>${tableHeaders}</tr>
                </thead>

                <tbody>
                    ${tableRows}
                </tbody>
            </table>
        </div>
    `;
}

/**
 * Render a table through UI.table when available.
 */
function renderImportTable(headers, rows) {
    if (
        window.UI &&
        typeof window.UI.table === "function"
    ) {
        return window.UI.table(headers, rows);
    }

    return buildImportFallbackTable(headers, rows);
}

/**
 * Render a badge through UI.badge when available.
 */
function renderImportBadge(text, className) {
    if (
        window.UI &&
        typeof window.UI.badge === "function"
    ) {
        return window.UI.badge(text);
    }

    return `
        <span class="badge ${className || ""}">
            ${escapeImportHtml(text)}
        </span>
    `;
}

/**
 * Shared top tabs.
 */
function getImportTabs(pendingDuplicateCount) {
    return `
        <div class="tabs">
            <a
                class="tab active"
                href="#"
                onclick="App.navigate('imports'); return false;"
            >
                Import &amp; Mapping Wizard
            </a>

            <a
                class="tab"
                href="#"
                onclick="App.navigate('duplicates'); return false;"
            >
                Duplicate Review Queue
                (${pendingDuplicateCount || 0})
            </a>

            <a
                class="tab"
                href="#"
                onclick="App.navigate('onboarding'); return false;"
            >
                Vendor Onboarding Readiness
            </a>
        </div>
    `;
}

/**
 * Stage 1:
 * Select Source File
 */
window.Pages["imports"] = function () {
    var store = window.Store || {};

    var vendors = Array.isArray(store.vendors)
        ? store.vendors
        : [];

    var importJobs = Array.isArray(store.importJobs)
        ? store.importJobs
        : [];

    var duplicateCandidates = Array.isArray(
        store.duplicateCandidates
    )
        ? store.duplicateCandidates
        : [];

    var pendingDuplicateCount =
        duplicateCandidates.filter(
            function (duplicate) {
                return duplicate.status ===
                    "Pending Review";
            }
        ).length;

    var vendorOptions;

    if (vendors.length > 0) {
        vendorOptions = vendors
            .map(function (vendor) {
                var vendorId =
                    escapeImportHtml(vendor.id);

                var vendorName =
                    escapeImportHtml(vendor.name);

                var selected =
                    String(
                        window.ImportWizardState.vendorId
                    ) === String(vendor.id)
                        ? "selected"
                        : "";

                return `
                    <option
                        value="${vendorId}"
                        ${selected}
                    >
                        ${vendorName} (${vendorId})
                    </option>
                `;
            })
            .join("");
    } else {
        vendorOptions = `
            <option value="">
                No vendors available
            </option>
        `;
    }

    var recentJobRows = importJobs.map(
        function (job) {
            return [
                "<strong>" +
                    escapeImportHtml(job.id) +
                "</strong>",

                escapeImportHtml(job.vendor),

                escapeImportHtml(job.uploaded),

                escapeImportHtml(job.rows) +
                    " rows (" +
                    escapeImportHtml(job.errors) +
                    " err)",

                renderImportBadge(
                    job.status,
                    "blue"
                )
            ];
        }
    );

    return `
        ${getImportTabs(pendingDuplicateCount)}

        <!-- STAGE 1 ACTIVE -->
        <div class="stepper">
            <div class="step active">
                1 · Select Source File
            </div>

            <div class="step">
                2 · Map Columns
            </div>

            <div class="step">
                3 · Preview &amp; Validate
            </div>

            <div class="step">
                4 · Duplicate Check
            </div>

            <div class="step">
                5 · Commit to Staging
            </div>
        </div>

        <article class="callout info" style="margin: 16px 0;">
            <strong>What this wizard does</strong>
            <p style="margin: 6px 0 10px;">
                It prepares a vendor spreadsheet for review. Your upload is not added to live records automatically.
            </p>
            <ol style="margin: 0; padding-left: 20px;">
                <li>Choose the vendor, the kind of data, and its CSV or Excel file.</li>
                <li>Match spreadsheet columns to the platform fields.</li>
                <li>Check invalid or missing values, then review possible duplicates.</li>
                <li>Commit the reviewed records to staging for the controlled import process.</li>
            </ol>
        </article>

        ${getImportMessageContainer()}

        <div class="grid two-col">
            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Vendor Data Ingestion</h3>

                        <p>
                            Upload CSV or Excel files from legacy
                            vendor systems.
                        </p>
                    </div>
                </div>

                <div class="form-grid">
                    <div class="field full">
                        <label for="importVendorSelect">
                            Acquired Vendor Entity
                            <span style="color: #ef4444;">
                                *
                            </span>
                        </label>

                        <select
                            id="importVendorSelect"
                            onchange="
                                ImportWizardActions.clearFieldError(this)
                            "
                        >
                            ${vendorOptions}
                        </select>
                    </div>

                    <div class="field full">
                        <label for="importEntitySelect">
                            Target Data Entity
                            <span style="color: #ef4444;">
                                *
                            </span>
                        </label>

                        <select
                            id="importEntitySelect"
                            onchange="
                                ImportWizardActions.clearFieldError(this)
                            "
                        >
                            <option
                                value="Customers"
                                ${
                                    window.ImportWizardState.entity ===
                                    "Customers"
                                        ? "selected"
                                        : ""
                                }
                            >
                                Customers &amp; Account Linkages
                            </option>

                            <option
                                value="Locations"
                                ${
                                    window.ImportWizardState.entity ===
                                    "Locations"
                                        ? "selected"
                                        : ""
                                }
                            >
                                Customer Facilities &amp; Sites
                            </option>

                            <option
                                value="Assets"
                                ${
                                    window.ImportWizardState.entity ===
                                    "Assets"
                                        ? "selected"
                                        : ""
                                }
                            >
                                HVAC Equipment Catalogue
                            </option>

                            <option
                                value="Contracts"
                                ${
                                    window.ImportWizardState.entity ===
                                    "Contracts"
                                        ? "selected"
                                        : ""
                                }
                            >
                                Service Agreements &amp;
                                Coverage Dates
                            </option>
                        </select>
                    </div>

                    <div class="field full">
                        <label for="importFileInput">
                            Select CSV / Excel File
                            <span style="color: #ef4444;">
                                *
                            </span>
                        </label>

                        <input
                            type="file"
                            id="importFileInput"
                            accept=".csv,.xlsx,.xls"
                            style="padding: 10px;"
                            onchange="
                                ImportWizardActions.handleFileChange(this)
                            "
                        >

                        <small
                            style="
                                color: #64748b;
                                display: block;
                                margin-top: 4px;
                                font-size: 11px;
                            "
                        >
                            Max 10 MB per upload.
                            Supported formats: CSV, XLSX and XLS.
                        </small>

                        <small
                            id="importFileStatus"
                            style="
                                display: none;
                                margin-top: 6px;
                                font-size: 11px;
                            "
                        ></small>
                    </div>

                    <div class="field full">
                        <label for="importBatchDesc">
                            Batch Description (Optional)
                        </label>

                        <input
                            id="importBatchDesc"
                            type="text"
                            value="${escapeImportHtml(
                                window.ImportWizardState
                                    .batchDescription
                            )}"
                            placeholder="e.g. Q4 2026 Customer Master extract from legacy QuickBooks"
                        >
                    </div>
                </div>

                <div
                    style="
                        margin-top: 16px;
                        display: flex;
                        justify-content: flex-end;
                    "
                >
                    <button
                        type="button"
                        class="btn primary"
                        onclick="
                            ImportWizardActions.uploadAndParse()
                        "
                    >
                        Upload &amp; Parse File
                    </button>
                </div>
            </article>

            <article class="card">
                <div class="card-title">
                    <div>
                        <h3>Recent Ingestion Jobs</h3>

                        <p>
                            Historical batch imports, validation
                            outcomes and staging logs.
                        </p>
                    </div>
                </div>

                ${
                    renderImportTable(
                        [
                            "Job ID",
                            "Vendor",
                            "Uploaded",
                            "Total Rows",
                            "Status"
                        ],
                        recentJobRows
                    )
                }
            </article>
        </div>
    `;
};

/**
 * Stage 2:
 * Map Columns
 */
window.Pages["columnMapping"] = function () {
    var store = window.Store || {};

    var duplicateCandidates = Array.isArray(
        store.duplicateCandidates
    )
        ? store.duplicateCandidates
        : [];

    var pendingDuplicateCount =
        duplicateCandidates.filter(
            function (duplicate) {
                return duplicate.status ===
                    "Pending Review";
            }
        ).length;

    var mappingRows = [
        [
            "Client_Legal_Name",
            "Apex Retail Group LLC",
            "Customer Name",
            '<span class="badge red">Required</span>',
            "Strip trailing LLC/Inc"
        ],
        [
            "Legacy_Account_No",
            "A-104",
            "Brand Account Number",
            '<span class="badge red">Required</span>',
            "Exact preserve"
        ],
        [
            "Contact_Email",
            "m.ellis@apex.example",
            "Primary Email",
            '<span class="badge blue">Optional</span>',
            "Lowercase validate"
        ],
        [
            "Service_Address",
            "450 Market Street, San Jose, CA",
            "Site Address Line",
            '<span class="badge red">Required</span>',
            "Postal code extract"
        ],
        [
            "Legacy_Key_ID",
            "PCS-104",
            "Source System Identifier",
            '<span class="badge blue">Optional</span>',
            "Provenance stamp"
        ]
    ];

    return `
        ${getImportTabs(pendingDuplicateCount)}

        <!-- STAGE 2 ACTIVE -->
        <div class="stepper">
            <div class="step done">
                1 · Select Source File
            </div>

            <div class="step active">
                2 · Map Columns
            </div>

            <div class="step">
                3 · Preview &amp; Validate
            </div>

            <div class="step">
                4 · Duplicate Check
            </div>

            <div class="step">
                5 · Commit to Staging
            </div>
        </div>

        ${getImportMessageContainer()}

        <article
            class="card"
            style="margin-top: 20px;"
        >
            <div class="card-title">
                <div>
                    <h3>Column Mapping Preview</h3>

                    <p>
                        Map legacy vendor spreadsheet headers
                        to HVAC Connect canonical fields.
                    </p>

                    ${
                        window.ImportWizardState.fileName
                            ? `
                                <small
                                    style="color: #64748b;"
                                >
                                    Source file:
                                    <strong>
                                        ${escapeImportHtml(
                                            window.ImportWizardState
                                                .fileName
                                        )}
                                    </strong>
                                </small>
                            `
                            : ""
                    }
                </div>

                ${renderImportBadge(
                    "Mapping Verified",
                    "blue"
                )}
            </div>

            ${
                renderImportTable(
                    [
                        "Source File Column",
                        "Sample Row Value",
                        "Platform Canonical Field",
                        "Requirement",
                        "Transform Rule"
                    ],
                    mappingRows
                )
            }

            <div
                style="
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-top: 16px;
                    gap: 16px;
                    flex-wrap: wrap;
                "
            >
                <small style="color: #64748b;">
                    5 fields mapped correctly ·
                    0 unmapped required fields
                </small>

                <div
                    style="
                        display: flex;
                        gap: 8px;
                        flex-wrap: wrap;
                    "
                >
                    <button
                        type="button"
                        class="btn"
                        onclick="App.navigate('imports')"
                    >
                        Back
                    </button>

                    <button
                        type="button"
                        class="btn"
                        onclick="
                            ImportWizardActions.saveMapping()
                        "
                    >
                        Save Mapping Template
                    </button>

                    <button
                        type="button"
                        class="btn primary"
                        onclick="
                            ImportWizardActions.runValidation()
                        "
                    >
                        Run Field Validation
                    </button>
                </div>
            </div>
        </article>
    `;
};

/**
 * Stage 3:
 * Preview and Validate
 */
window.Pages["previewValidation"] = function () {
    var store = window.Store || {};

    var duplicateCandidates = Array.isArray(
        store.duplicateCandidates
    )
        ? store.duplicateCandidates
        : [];

    var pendingDuplicateCount =
        duplicateCandidates.filter(
            function (duplicate) {
                return duplicate.status ===
                    "Pending Review";
            }
        ).length;

    var summaryRows = [
        ["Total Records", "1,250"],
        ["Valid Records", "1,220"],
        ["Warnings", "20"],
        ["Errors", "10"]
    ];

    var issueRows = [
        [
            "12",
            "Customer Name",
            "Required field missing"
        ],
        [
            "27",
            "Service Address",
            "Invalid address format"
        ],
        [
            "45",
            "Email",
            "Invalid email format"
        ]
    ];

    return `
        ${getImportTabs(pendingDuplicateCount)}

        <!-- STAGE 3 ACTIVE -->
        <div class="stepper">
            <div class="step done">
                1 · Select Source File
            </div>

            <div class="step done">
                2 · Map Columns
            </div>

            <div class="step active">
                3 · Preview &amp; Validate
            </div>

            <div class="step">
                4 · Duplicate Check
            </div>

            <div class="step">
                5 · Commit to Staging
            </div>
        </div>

        ${getImportMessageContainer()}

        <article
            class="card"
            style="margin-top: 20px;"
        >
            <div class="card-title">
                <div>
                    <h3>Validation Summary</h3>

                    <p>
                        Review imported records before proceeding.
                    </p>

                    ${
                        window.ImportWizardState.fileName
                            ? `
                                <small
                                    style="color: #64748b;"
                                >
                                    Validated file:
                                    <strong>
                                        ${escapeImportHtml(
                                            window.ImportWizardState
                                                .fileName
                                        )}
                                    </strong>
                                </small>
                            `
                            : ""
                    }
                </div>

                ${renderImportBadge(
                    "Ready for Review",
                    "blue"
                )}
            </div>

            ${
                renderImportTable(
                    ["Metric", "Count"],
                    summaryRows
                )
            }

            <div style="margin-top: 20px;">
                <h4>Validation Issues</h4>

                ${
                    renderImportTable(
                        ["Row", "Field", "Issue"],
                        issueRows
                    )
                }
            </div>

            <div
                style="
                    display: flex;
                    justify-content: flex-end;
                    gap: 8px;
                    margin-top: 20px;
                    flex-wrap: wrap;
                "
            >
                <button
                    type="button"
                    class="btn"
                    onclick="
                        App.navigate('columnMapping')
                    "
                >
                    Back to Mapping
                </button>

                <button
                    type="button"
                    class="btn primary"
                    onclick="
                        App.navigate('duplicates')
                    "
                >
                    Proceed to Duplicate Check
                </button>
            </div>
        </article>
    `;
};

/**
 * Import Wizard actions.
 */
window.ImportWizardActions = {
    /**
     * Remove error styling after the user changes a field.
     */
    clearFieldError: function (field) {
        if (!field) {
            return;
        }

        field.removeAttribute("aria-invalid");
        field.style.borderColor = "";
        field.style.boxShadow = "";
    },

    /**
     * Show file selection status immediately.
     */
    handleFileChange: function (fileInput) {
        this.clearFieldError(fileInput);
        hideImportMessage();

        var fileStatus = document.getElementById(
            "importFileStatus"
        );

        if (
            !fileInput ||
            !fileInput.files ||
            fileInput.files.length === 0
        ) {
            if (fileStatus) {
                fileStatus.style.display = "none";
                fileStatus.textContent = "";
            }

            return;
        }

        var selectedFile = fileInput.files[0];
        var maximumFileSize = 10 * 1024 * 1024;
        var allowedExtensions = [
            "csv",
            "xlsx",
            "xls"
        ];

        var fileNameParts =
            selectedFile.name.split(".");

        var fileExtension =
            fileNameParts.length > 1
                ? fileNameParts.pop().toLowerCase()
                : "";

        if (!allowedExtensions.includes(fileExtension)) {
            showImportMessage(
                '"' +
                selectedFile.name +
                '" is not supported. Select a CSV, XLSX, or XLS file.',
                "error"
            );

            markImportFieldInvalid(fileInput);

            if (fileStatus) {
                fileStatus.style.display = "block";
                fileStatus.style.color = "#991b1b";
                fileStatus.textContent =
                    "Unsupported file format.";
            }

            return;
        }

        if (selectedFile.size > maximumFileSize) {
            var fileSizeInMb =
                selectedFile.size /
                (1024 * 1024);

            showImportMessage(
                '"' +
                selectedFile.name +
                '" is ' +
                fileSizeInMb.toFixed(2) +
                " MB. The maximum permitted size is 10 MB.",
                "error"
            );

            markImportFieldInvalid(fileInput);

            if (fileStatus) {
                fileStatus.style.display = "block";
                fileStatus.style.color = "#991b1b";
                fileStatus.textContent =
                    "File exceeds the 10 MB limit.";
            }

            return;
        }

        if (fileStatus) {
            var validFileSize =
                selectedFile.size /
                (1024 * 1024);

            fileStatus.style.display = "block";
            fileStatus.style.color = "#166534";
            fileStatus.textContent =
                selectedFile.name +
                " (" +
                validFileSize.toFixed(2) +
                " MB) is ready to upload.";
        }
    },

    /**
     * Validate stage 1 and open column mapping.
     */
    uploadAndParse: function () {
        hideImportMessage();
        clearImportFieldErrors();

        var vendorSelect =
            document.getElementById(
                "importVendorSelect"
            );

        var entitySelect =
            document.getElementById(
                "importEntitySelect"
            );

        var fileInput =
            document.getElementById(
                "importFileInput"
            );

        var batchDescriptionInput =
            document.getElementById(
                "importBatchDesc"
            );

        if (
            !vendorSelect ||
            !vendorSelect.value
        ) {
            showImportMessage(
                "Select an acquired vendor entity.",
                "error"
            );

            markImportFieldInvalid(
                vendorSelect
            );

            return;
        }

        if (
            !entitySelect ||
            !entitySelect.value
        ) {
            showImportMessage(
                "Select a target data entity.",
                "error"
            );

            markImportFieldInvalid(
                entitySelect
            );

            return;
        }

        if (
            !fileInput ||
            !fileInput.files ||
            fileInput.files.length === 0
        ) {
            showImportMessage(
                "Select a CSV, XLSX, or XLS file before continuing.",
                "error"
            );

            markImportFieldInvalid(
                fileInput
            );

            return;
        }

        var selectedFile =
            fileInput.files[0];

        var maximumFileSize =
            10 * 1024 * 1024;

        var allowedExtensions = [
            "csv",
            "xlsx",
            "xls"
        ];

        var fileNameParts =
            selectedFile.name.split(".");

        var fileExtension =
            fileNameParts.length > 1
                ? fileNameParts.pop().toLowerCase()
                : "";

        if (
            !allowedExtensions.includes(
                fileExtension
            )
        ) {
            showImportMessage(
                '"' +
                selectedFile.name +
                '" is not supported. Select a CSV, XLSX, or XLS file.',
                "error"
            );

            markImportFieldInvalid(
                fileInput
            );

            return;
        }

        if (
            selectedFile.size >
            maximumFileSize
        ) {
            var fileSizeInMb =
                selectedFile.size /
                (1024 * 1024);

            showImportMessage(
                '"' +
                selectedFile.name +
                '" is ' +
                fileSizeInMb.toFixed(2) +
                " MB. The maximum permitted size is 10 MB.",
                "error"
            );

            markImportFieldInvalid(
                fileInput
            );

            return;
        }

        window.ImportWizardState.vendorId =
            vendorSelect.value;

        window.ImportWizardState.entity =
            entitySelect.value;

        window.ImportWizardState.batchDescription =
            batchDescriptionInput
                ? batchDescriptionInput.value.trim()
                : "";

        window.ImportWizardState.fileName =
            selectedFile.name;

        window.ImportWizardState.fileSize =
            selectedFile.size;

        showImportMessage(
            '"' +
            selectedFile.name +
            '" was parsed successfully. Opening column mapping.',
            "success"
        );

        window.setTimeout(
            function () {
                navigateImportPage(
                    "columnMapping"
                );
            },
            600
        );
    },

    /**
     * Save the current mapping.
     */
    saveMapping: function () {
        hideImportMessage();

        showImportMessage(
            "The column mapping template was saved successfully.",
            "success"
        );
    },

    /**
     * Run field validation and open stage 3.
     */
    runValidation: function () {
        hideImportMessage();

        showImportMessage(
            "Field validation completed. Opening the validation summary.",
            "success"
        );

        window.setTimeout(
            function () {
                navigateImportPage(
                    "previewValidation"
                );
            },
            600
        );
    }
};