window.Pages = window.Pages || {};
Store.selectedDuplicateId =
    Store.selectedDuplicateId || null;
window.Pages["duplicates"] = function () {

    const candidates = Store.duplicateCandidates || [];

    const active = Store.selectedDuplicateId
        ? candidates.find(x => x.id === Store.selectedDuplicateId)
        : null;

    return `

    <div class="tabs">
        <a class="tab">✓ Upload</a>
        <a class="tab">✓ Mapping</a>
        <a class="tab">✓ Validation</a>
        <a class="tab active">Duplicate Review</a>
        <a class="tab" href="#" onclick="App.navigate('onboarding'); return false;">Completion</a>
    </div>

    <div class="callout warning">
        <strong>Duplicate Records Identified</strong><br/>
        Some imported records may already exist in HVAC Connect.
        Review each record and decide whether to merge, create as new, or skip.
    </div>

    <div style="
    display:flex;
    gap:16px;
    margin-bottom:20px;
    flex-wrap:wrap;
">

    <div class="card" style="flex:1; min-width:220px;">
        <div style="display:flex;align-items:center;gap:12px;">
            <span style="font-size:24px;">📄</span>
            <div>
                <h2 style="margin:0;">1250</h2>
                <small>Total Imported Records</small>
            </div>
        </div>
    </div>

    <div class="card" style="flex:1; min-width:220px;">
        <div style="display:flex;align-items:center;gap:12px;">
            <span style="font-size:24px;">✅</span>
            <div>
                <h2 style="margin:0;color:#16a34a;">1220</h2>
                <small>Valid Records</small>
            </div>
        </div>
    </div>

    <div class="card" style="flex:1; min-width:220px;">
        <div style="display:flex;align-items:center;gap:12px;">
            <span style="font-size:24px;">🔍</span>
            <div>
                <h2 style="margin:0;color:#d97706;">
                    ${candidates.length}
                </h2>
                <small>Duplicate Records</small>
            </div>
        </div>
    </div>

    <div class="card" style="flex:1; min-width:220px;">
        <div style="display:flex;align-items:center;gap:12px;">
            <span style="font-size:24px;">⚠️</span>
            <div>
                <h2 style="margin:0;color:#dc2626;">10</h2>
                <small>Validation Errors</small>
            </div>
        </div>
    </div>

</div>

    <article class="card" style="margin-top:20px;">

        <div class="card-title">
            <div>
                <h3>Duplicate Review Queue</h3>
                <p>
                    Review imported records before completing onboarding.
                </p>
            </div>
        </div>

        ${UI.table(
            [
                "Type",
                "Imported Record",
                "Existing Record",
                "Match Reason",
                "Status",
                "Action"
            ],
            candidates.map(c => [

                c.entityType,

                `<b>${c.candidateName}</b>`,

                `<b>${c.existingName}</b>`,

                c.matchReason,

                UI.badge(c.status),

                `
                <button
                    class="btn small primary"
                    onclick="
                        Store.selectedDuplicateId='${c.id}';
                        Router.loadCurrentPage();
                    ">
                    Review
                </button>
                `
            ])
        )}

    </article>

    ${active ? `

    <article class="card" style="margin-top:20px;">

        <div class="card-title">
            <div>
                <h3>Duplicate Record Review</h3>
                <p>
                    Compare imported data with existing system data.
                </p>
            </div>
        </div>

        <div class="compare-container">

            <div class="compare-box">

                <h4>Imported Record</h4>

                <div class="compare-field">
                    <small>Name</small>
                    <b>${active.candidateName}</b>
                </div>

                <div class="compare-field">
                    <small>Email</small>
                    <span>${active.candidateEmail}</span>
                </div>

                <div class="compare-field">
                    <small>Address</small>
                    <span>${active.candidateAddress}</span>
                </div>

                <div class="compare-field">
                    <small>Account Number</small>
                    <code>${active.candidateAccountNo}</code>
                </div>

            </div>

            <div class="compare-box">

                <h4>Existing Record</h4>

                <div class="compare-field">
                    <small>Name</small>
                    <b>${active.existingName}</b>
                </div>

                <div class="compare-field">
                    <small>Email</small>
                    <span>${active.existingEmail}</span>
                </div>

                <div class="compare-field">
                    <small>Address</small>
                    <span>${active.existingAddress}</span>
                </div>

                <div class="compare-field">
                    <small>Account Number</small>
                    <code>${active.existingAccountNo}</code>
                </div>

            </div>

        </div>

        <div class="callout info" style="margin-top:16px;">
            <b>Differences Found</b>

            <ul style="margin-top:8px;">
                <li>Email Address differs</li>
                <li>Account Number differs</li>
                <li>Customer Name is similar</li>
                <li>Address requires review</li>
            </ul>
        </div>
        <div
        id="duplicateReviewMessage"
        style="
            display:none;
            margin-top:16px;
            padding:12px;
            border-radius:8px;
            border:1px solid #fecaca;
            background:#fef2f2;
            color:#b91c1c;
            font-weight:500;">
    </div>
        <div class="field" style="margin-top:16px;">
            <label>
                Decision
                <span style="color:red">*</span>
            </label>

            <select id="duplicateDecision">
                <option value="">Select Decision</option>
                <option value="merge">Merge With Existing Record</option>
                <option value="new">Create New Record</option>
                <option value="skip">Skip Import</option>
            </select>
        </div>

        <div class="field" style="margin-top:16px;">
            <label>
                Review Comments
                <span style="color:red">*</span>
            </label>

            <textarea
                id="duplicateDecisionNotes"
                rows="4"
                placeholder="Enter review comments">
            </textarea>
        </div>

        <div style="
            display:flex;
            justify-content:flex-end;
            gap:8px;
            margin-top:20px;
            border-top:1px solid #e2e8f0;
            padding-top:16px;">

            <button
                class="btn"
                onclick="
                    Store.selectedDuplicateId=null;
                    Router.loadCurrentPage();
                ">
                Cancel
            </button>

            <button
    class="btn primary"
    onclick="saveDuplicateDecision('${active.id}')">
    Save Decision
</button>
        </div>

    </article>

    ` : ""}

    `;
};


/* =========================================
   SAVE DECISION
========================================= */

window.saveDuplicateDecision = function(id) {

    const decision =
        document.getElementById("duplicateDecision")?.value;

    const comments =
        document.getElementById("duplicateDecisionNotes")
            ?.value
            ?.trim();

    if (!decision) {

        showDuplicateMessage(
            "Please select a decision."
        );

        return;
    }

    if (!comments) {

        showDuplicateMessage(
            "Review comments are required."
        );

        return;
    }

    const record =
        Store.duplicateCandidates.find(
            x => x.id === id
        );

    if (!record) {

        showDuplicateMessage(
            "Duplicate record not found."
        );

        return;
    }

    switch (decision) {

        case "merge":
            record.status = "Merged";
            break;

        case "new":
            record.status = "Create New";
            break;

        case "skip":
            record.status = "Skipped";
            break;
    }

    showDuplicateMessage(
        "Decision saved successfully.",
        "success"
    );

    setTimeout(() => {

        Store.selectedDuplicateId = null;

        Router.loadCurrentPage();

    }, 1000);
};

function showDuplicateMessage(
    message,
    type = "error"
) {

    const box =
        document.getElementById(
            "duplicateReviewMessage"
        );

    if (!box) return;

    box.style.display = "block";

    if (type === "success") {

        box.style.background = "#ecfdf5";
        box.style.border = "1px solid #bbf7d0";
        box.style.color = "#15803d";

    } else {

        box.style.background = "#fef2f2";
        box.style.border = "1px solid #fecaca";
        box.style.color = "#b91c1c";
    }

    box.innerHTML = message;
}