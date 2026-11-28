/**
 * Page: Vendor Onboarding Readiness & Checklist
 * PB-030 / US047
 *
 * Flow:
 * 1. Complete Phase 1 checklist.
 * 2. Approve Cutover through a confirmation modal.
 * 3. Phase 2 appears directly on this page.
 * 4. Complete Phase 2 checklist.
 * 5. Mark vendor as Fully Unified Vendor.
 */

(function () {
    "use strict";

    window.Pages = window.Pages || {};

    /*
     * Remove event listeners installed by an older
     * version of this onboarding file.
     */
    if (
        window.OnboardingPageController &&
        window.OnboardingPageController.abortController
    ) {
        window.OnboardingPageController
            .abortController
            .abort();
    }

    const abortController = new AbortController();

    window.OnboardingPageController = {
        abortController: abortController
    };

    const listenerOptions = {
        signal: abortController.signal
    };


    /* =====================================================
       DATA HELPERS
    ===================================================== */

    function getVendor() {
        if (
            !window.Store ||
            !Array.isArray(window.Store.vendors)
        ) {
            return null;
        }

        return window.Store.vendors[0] || null;
    }


    function getAllTasks() {
        if (
            !window.Store ||
            !Array.isArray(
                window.Store.onboardingChecklists
            )
        ) {
            return [];
        }

        return window.Store.onboardingChecklists;
    }


    function getPhaseTasks(phaseName) {
        return getAllTasks()
            .map(function (task, index) {
                return {
                    task: task,
                    storeIndex: index
                };
            })
            .filter(function (item) {
                return item.task.phase === phaseName;
            });
    }


    function getCurrentUserName() {
        if (
            window.Store &&
            window.Store.currentUser &&
            window.Store.currentUser.name
        ) {
            return window.Store.currentUser.name;
        }

        return "Current User";
    }


    function escapeHtml(value) {
        return String(
            value == null ? "" : value
        )
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }


    /* =====================================================
       APPLICATION HELPERS
    ===================================================== */

    function showToast(message, type) {
        if (
            window.App &&
            typeof window.App.toast === "function"
        ) {
            window.App.toast(message, type);
            return;
        }

        console.log(message);
    }


    /**
     * Live page refresh.
     *
     * The onboarding page replaces its own root element.
     * This does not depend on App.navigate() rerendering
     * the currently active route.
     */
    function refreshOnboardingPage() {
        const currentPage = document.getElementById(
            "onboarding-page-root"
        );

        if (
            currentPage &&
            typeof window.Pages["onboarding"] ===
                "function"
        ) {
            currentPage.outerHTML =
                window.Pages["onboarding"]();

            return;
        }

        /*
         * Fallback when the onboarding root is unavailable.
         */
        if (
            window.App &&
            typeof window.App.navigate === "function"
        ) {
            window.App.navigate("onboarding");
        }
    }


    /* =====================================================
       STATE HELPERS
    ===================================================== */

    function isFullyUnified(vendor) {
        if (!vendor) {
            return false;
        }

        return (
            vendor.fullyUnified === true ||
            vendor.phase2Approved === true ||
            vendor.phase === "Fully Unified" ||
            vendor.status ===
                "Fully Unified Vendor"
        );
    }


    function isPhase2Active(vendor) {
        if (!vendor) {
            return false;
        }

        if (isFullyUnified(vendor)) {
            return false;
        }

        return (
            vendor.phase1Approved === true ||
            vendor.phase ===
                "Phase 2 Cutover Planning" ||
            vendor.phase === "Phase 2" ||
            vendor.status === "Phase 2"
        );
    }


    /**
     * Normalizes older Store data so the page works
     * whether it contains flags, status, or phase values.
     */
    function normalizeVendorState(vendor) {
        if (!vendor) {
            return;
        }

        if (isFullyUnified(vendor)) {
            vendor.phase = "Fully Unified";
            vendor.status =
                "Fully Unified Vendor";
            vendor.progress = "100%";
            vendor.phase1Approved = true;
            vendor.phase2Approved = true;
            vendor.fullyUnified = true;

            return;
        }

        if (isPhase2Active(vendor)) {
            vendor.phase =
                "Phase 2 Cutover Planning";

            vendor.status = "Phase 2";
            vendor.phase1Approved = true;
            vendor.phase2Approved = false;
            vendor.fullyUnified = false;
        }
    }


    /* =====================================================
       MODAL HELPERS
    ===================================================== */

    function closeOnboardingModal() {
        const modal = document.getElementById(
            "onboarding-action-modal"
        );

        if (modal) {
            modal.remove();
        }
    }


    function openOnboardingModal(content) {
        closeOnboardingModal();

        const modal = document.createElement("div");

        modal.id = "onboarding-action-modal";
        modal.className =
            "onboarding-modal-backdrop";

        modal.innerHTML = `
            <div
                class="onboarding-modal"
                role="dialog"
                aria-modal="true"
            >
                ${content}
            </div>
        `;

        document.body.appendChild(modal);
    }


    /* =====================================================
       PHASE 1 CHECKLIST
    ===================================================== */

    function updatePhase1Task(
        taskIndex,
        completed
    ) {
        const vendor = getVendor();
        const tasks = getAllTasks();
        const task = tasks[taskIndex];

        if (!vendor) {
            showToast(
                "Vendor not found",
                "error"
            );

            return;
        }

        if (!task) {
            showToast(
                "Checklist item not found",
                "error"
            );

            return;
        }

        if (task.phase !== "Phase 1") {
            showToast(
                "Invalid Phase 1 checklist item",
                "error"
            );

            return;
        }

        if (
            isPhase2Active(vendor) ||
            isFullyUnified(vendor)
        ) {
            showToast(
                "Phase 1 has already been approved"
            );

            refreshOnboardingPage();
            return;
        }

        task.completed = Boolean(completed);

        if (task.completed) {
            task.by = getCurrentUserName();

            task.completedAt =
                new Date().toISOString();
        } else {
            task.by = "";
            task.completedAt = null;
        }

        /*
         * Refresh first so the button becomes enabled
         * immediately after the final item is checked.
         */
        refreshOnboardingPage();

        showToast(
            task.completed
                ? "Checklist item completed"
                : "Checklist item marked as pending"
        );
    }


    /* =====================================================
       PHASE 1 CUTOVER APPROVAL
    ===================================================== */

    function openCutoverApprovalModal() {
        const vendor = getVendor();

        const phase1Tasks =
            getPhaseTasks("Phase 1");

        const pendingTasks =
            phase1Tasks.filter(
                function (item) {
                    return !item.task.completed;
                }
            );

        if (!vendor) {
            showToast(
                "Vendor not found",
                "error"
            );

            return;
        }

        if (isFullyUnified(vendor)) {
            showToast(
                vendor.name +
                " is already a Fully Unified Vendor"
            );

            return;
        }

        if (isPhase2Active(vendor)) {
            showToast(
                "Phase 1 is already approved"
            );

            refreshOnboardingPage();
            return;
        }

        if (phase1Tasks.length === 0) {
            showToast(
                "No Phase 1 checklist items configured",
                "error"
            );

            return;
        }

        if (pendingTasks.length > 0) {
            showToast(
                "Complete " +
                pendingTasks.length +
                " Phase 1 checklist item(s) before approving cutover",
                "error"
            );

            return;
        }

        openOnboardingModal(`
            <div class="onboarding-modal-header">
                <div>
                    <h3>
                        Approve Phase 1 Cutover
                    </h3>

                    <p>
                        Confirm that
                        ${escapeHtml(vendor.name)}
                        is ready to move to Phase 2.
                    </p>
                </div>

                <button
                    id="close-onboarding-modal"
                    class="modal-close-button"
                    type="button"
                    aria-label="Close"
                >
                    &times;
                </button>
            </div>

            <div class="onboarding-modal-body">
                <div class="callout info">
                    <strong>
                        Phase 1 checklist completed.
                    </strong>

                    Approving the cutover will activate
                    the Phase 2 checklist directly on
                    this page.
                </div>

                <div class="onboarding-approval-summary">
                    <div>
                        <span>Vendor</span>

                        <strong>
                            ${escapeHtml(vendor.name)}
                        </strong>
                    </div>

                    <div>
                        <span>Current phase</span>

                        <strong>
                            Phase 1 Coexist Live
                        </strong>
                    </div>

                    <div>
                        <span>Next phase</span>

                        <strong>
                            Phase 2 Cutover Planning
                        </strong>
                    </div>
                </div>
            </div>

            <div class="onboarding-modal-actions">
                <button
                    id="cancel-cutover-approval"
                    class="btn"
                    type="button"
                >
                    Cancel
                </button>

                <button
                    id="confirm-cutover-approval"
                    class="btn primary"
                    type="button"
                >
                    Approve Cutover
                </button>
            </div>
        `);
    }


    function approvePhase1Cutover() {
        const vendor = getVendor();

        const phase1Tasks =
            getPhaseTasks("Phase 1");

        const pendingTasks =
            phase1Tasks.filter(
                function (item) {
                    return !item.task.completed;
                }
            );

        if (!vendor) {
            closeOnboardingModal();

            showToast(
                "Vendor not found",
                "error"
            );

            return;
        }

        if (
            phase1Tasks.length === 0 ||
            pendingTasks.length > 0
        ) {
            closeOnboardingModal();

            showToast(
                "Complete all Phase 1 checklist items before approving cutover",
                "error"
            );

            refreshOnboardingPage();
            return;
        }

        /*
         * Set every Phase 2 property so the page does
         * not depend on only one state field.
         */
        vendor.phase =
            "Phase 2 Cutover Planning";

        vendor.status = "Phase 2";
        vendor.progress = "0%";

        vendor.phase1Approved = true;

        vendor.phase1ApprovedAt =
            new Date().toISOString();

        vendor.phase2Approved = false;
        vendor.fullyUnified = false;

        closeOnboardingModal();

        /*
         * Immediately display Phase 2.
         */
        refreshOnboardingPage();

        showToast(
            "Cutover approved. Phase 2 is now active."
        );
    }


    /* =====================================================
       PHASE 2 INLINE CHECKLIST
    ===================================================== */

    function updatePhase2Task(
        taskIndex,
        completed
    ) {
        const vendor = getVendor();
        const tasks = getAllTasks();
        const task = tasks[taskIndex];

        if (!vendor) {
            showToast(
                "Vendor not found",
                "error"
            );

            return;
        }

        if (!isPhase2Active(vendor)) {
            showToast(
                "Approve Phase 1 before updating Phase 2",
                "error"
            );

            refreshOnboardingPage();
            return;
        }

        if (isFullyUnified(vendor)) {
            showToast(
                "This vendor is already fully unified"
            );

            refreshOnboardingPage();
            return;
        }

        if (!task) {
            showToast(
                "Phase 2 checklist item not found",
                "error"
            );

            return;
        }

        if (task.phase !== "Phase 2") {
            showToast(
                "Invalid Phase 2 checklist item",
                "error"
            );

            return;
        }

        task.completed = Boolean(completed);

        if (task.completed) {
            task.by = getCurrentUserName();

            task.completedAt =
                new Date().toISOString();
        } else {
            task.by = "";
            task.completedAt = null;
        }

        const phase2Tasks =
            getPhaseTasks("Phase 2");

        const completedCount =
            phase2Tasks.filter(
                function (item) {
                    return Boolean(
                        item.task.completed
                    );
                }
            ).length;

        const percentage =
            phase2Tasks.length > 0
                ? Math.round(
                    (
                        completedCount /
                        phase2Tasks.length
                    ) * 100
                )
                : 0;

        vendor.progress =
            percentage + "%";

        /*
         * Updates item status, count, readiness score,
         * and the Fully Unified button immediately.
         */
        refreshOnboardingPage();

        showToast(
            task.completed
                ? "Phase 2 checklist item completed"
                : "Phase 2 checklist item marked as pending"
        );
    }


    function completePhase2Cutover() {
        const vendor = getVendor();

        const phase2Tasks =
            getPhaseTasks("Phase 2");

        const pendingTasks =
            phase2Tasks.filter(
                function (item) {
                    return !item.task.completed;
                }
            );

        if (!vendor) {
            showToast(
                "Vendor not found",
                "error"
            );

            return;
        }

        if (!isPhase2Active(vendor)) {
            showToast(
                "Phase 1 cutover must be approved first",
                "error"
            );

            return;
        }

        if (isFullyUnified(vendor)) {
            showToast(
                vendor.name +
                " is already a Fully Unified Vendor"
            );

            return;
        }

        if (phase2Tasks.length === 0) {
            showToast(
                "No Phase 2 checklist items configured",
                "error"
            );

            return;
        }

        if (pendingTasks.length > 0) {
            showToast(
                "Complete " +
                pendingTasks.length +
                " Phase 2 checklist item(s) first",
                "error"
            );

            return;
        }

        /*
         * Set every final-state property.
         */
        vendor.phase = "Fully Unified";

        vendor.status =
            "Fully Unified Vendor";

        vendor.progress = "100%";

        vendor.phase1Approved = true;
        vendor.phase2Approved = true;
        vendor.fullyUnified = true;

        vendor.phase2ApprovedAt =
            new Date().toISOString();

        vendor.fullyUnifiedAt =
            new Date().toISOString();

        /*
         * Immediately display the Fully Unified state.
         */
        refreshOnboardingPage();

        showToast(
            vendor.name +
            " is now a Fully Unified Vendor"
        );
    }


    /* =====================================================
       LIVE EVENT LISTENERS
    ===================================================== */

    document.addEventListener(
        "click",
        function (event) {
            const target = event.target;

            if (!(target instanceof Element)) {
                return;
            }


            const approveButton =
                target.closest(
                    "#approve-cutover-button"
                );

            if (approveButton) {
                event.preventDefault();

                if (!approveButton.disabled) {
                    openCutoverApprovalModal();
                }

                return;
            }


            const confirmApprovalButton =
                target.closest(
                    "#confirm-cutover-approval"
                );

            if (confirmApprovalButton) {
                event.preventDefault();
                approvePhase1Cutover();
                return;
            }


            const completePhase2Button =
                target.closest(
                    "#complete-phase-two-button"
                );

            if (completePhase2Button) {
                event.preventDefault();

                if (!completePhase2Button.disabled) {
                    completePhase2Cutover();
                }

                return;
            }


            const downloadButton =
                target.closest(
                    "#download-readiness-button"
                );

            if (downloadButton) {
                event.preventDefault();

                showToast(
                    "Exported onboarding readiness report"
                );

                return;
            }


            const closeButton =
                target.closest(
                    [
                        "#close-onboarding-modal",
                        "#cancel-cutover-approval"
                    ].join(",")
                );

            if (closeButton) {
                event.preventDefault();
                closeOnboardingModal();
                return;
            }


            const modal = document.getElementById(
                "onboarding-action-modal"
            );

            if (
                modal &&
                target === modal
            ) {
                closeOnboardingModal();
            }
        },
        listenerOptions
    );


    document.addEventListener(
        "change",
        function (event) {
            const target = event.target;

            if (
                !(
                    target instanceof
                    HTMLInputElement
                )
            ) {
                return;
            }


            if (
                target.matches(
                    ".phase-one-checkbox"
                )
            ) {
                const taskIndex = Number(
                    target.dataset.taskIndex
                );

                if (Number.isInteger(taskIndex)) {
                    updatePhase1Task(
                        taskIndex,
                        target.checked
                    );
                }

                return;
            }


            if (
                target.matches(
                    ".phase-two-checkbox"
                )
            ) {
                const taskIndex = Number(
                    target.dataset.taskIndex
                );

                if (Number.isInteger(taskIndex)) {
                    updatePhase2Task(
                        taskIndex,
                        target.checked
                    );
                }
            }
        },
        listenerOptions
    );


    /* =====================================================
       PAGE RENDERER
    ===================================================== */

    window.Pages["onboarding"] = function () {
        const vendor = getVendor();

        if (!vendor) {
            return `
                <div id="onboarding-page-root">
                    <article class="card">
                        <div class="callout warning">
                            <strong>
                                No vendor selected.
                            </strong>

                            Select a vendor before
                            opening onboarding.
                        </div>
                    </article>
                </div>
            `;
        }

        normalizeVendorState(vendor);

        const phase1Tasks =
            getPhaseTasks("Phase 1");

        const phase2Tasks =
            getPhaseTasks("Phase 2");

        const phase1CompletedCount =
            phase1Tasks.filter(
                function (item) {
                    return Boolean(
                        item.task.completed
                    );
                }
            ).length;

        const phase2CompletedCount =
            phase2Tasks.filter(
                function (item) {
                    return Boolean(
                        item.task.completed
                    );
                }
            ).length;

        const phase1Complete =
            phase1Tasks.length > 0 &&
            phase1CompletedCount ===
                phase1Tasks.length;

        const phase2Complete =
            phase2Tasks.length > 0 &&
            phase2CompletedCount ===
                phase2Tasks.length;

        const vendorIsFullyUnified =
            isFullyUnified(vendor);

        const vendorIsPhase2 =
            isPhase2Active(vendor);

        const currentStatus =
            vendorIsFullyUnified
                ? "Fully Unified Vendor"
                : vendorIsPhase2
                    ? "Phase 2 Cutover Planning"
                    : (
                        vendor.phase ||
                        "Phase 1 Coexist Live"
                    );

        let readinessScore =
            vendor.progress || "Pending";

        if (vendorIsFullyUnified) {
            readinessScore = "100%";
        } else if (vendorIsPhase2) {
            readinessScore =
                phase2Tasks.length > 0
                    ? Math.round(
                        (
                            phase2CompletedCount /
                            phase2Tasks.length
                        ) * 100
                    ) + "%"
                    : "0%";
        }


        /* =============================================
           PHASE 1 CHECKLIST HTML
        ============================================= */

        let phase1ChecklistHtml = "";

        if (phase1Tasks.length === 0) {
            phase1ChecklistHtml = `
                <div class="callout warning">
                    No Phase 1 checklist items
                    are configured.
                </div>
            `;
        } else {
            phase1ChecklistHtml =
                phase1Tasks
                    .map(function (item) {
                        const task = item.task;

                        const checked =
                            task.completed
                                ? "checked"
                                : "";

                        const disabled =
                            vendorIsPhase2 ||
                            vendorIsFullyUnified
                                ? "disabled"
                                : "";

                        const statusColor =
                            task.completed
                                ? "#15803d"
                                : "#f59e0b";

                        const statusText =
                            task.completed
                                ? "✓ Verified by " +
                                  escapeHtml(
                                      task.by ||
                                      "Current User"
                                  )
                                : "Pending sign-off";

                        return `
                            <label class="check-row">
                                <input
                                    class="phase-one-checkbox"
                                    type="checkbox"
                                    data-task-index="${item.storeIndex}"
                                    ${checked}
                                    ${disabled}
                                >

                                <span>
                                    ${escapeHtml(task.task)}

                                    <small
                                        style="
                                            display: block;
                                            margin-top: 4px;
                                            color: ${statusColor};
                                        "
                                    >
                                        ${statusText}
                                    </small>
                                </span>
                            </label>
                        `;
                    })
                    .join("");
        }


        /* =============================================
           PHASE 2 CHECKLIST HTML
        ============================================= */

        let phase2ChecklistHtml = "";

        if (phase2Tasks.length === 0) {
            phase2ChecklistHtml = `
                <div class="callout warning">
                    No Phase 2 checklist items
                    are configured.
                </div>
            `;
        } else {
            phase2ChecklistHtml =
                phase2Tasks
                    .map(function (item) {
                        const task = item.task;

                        const checked =
                            task.completed
                                ? "checked"
                                : "";

                        const disabled =
                            vendorIsFullyUnified
                                ? "disabled"
                                : "";

                        const statusColor =
                            task.completed
                                ? "#15803d"
                                : "#f59e0b";

                        const statusText =
                            task.completed
                                ? "✓ Verified by " +
                                  escapeHtml(
                                      task.by ||
                                      "Current User"
                                  )
                                : "Pending milestone";

                        return `
                            <label class="check-row">
                                <input
                                    class="phase-two-checkbox"
                                    type="checkbox"
                                    data-task-index="${item.storeIndex}"
                                    ${checked}
                                    ${disabled}
                                >

                                <span>
                                    ${escapeHtml(task.task)}

                                    <small
                                        style="
                                            display: block;
                                            margin-top: 4px;
                                            color: ${statusColor};
                                        "
                                    >
                                        ${statusText}
                                    </small>
                                </span>
                            </label>
                        `;
                    })
                    .join("");
        }


        let phase2SectionHtml = "";

        if (vendorIsFullyUnified) {
            phase2SectionHtml = `
                <div class="callout success">
                    <strong>
                        ✓ Phase 2 completed
                    </strong>

                    All transition and cutover
                    milestones have been completed.
                </div>

                <div
                    class="list"
                    style="margin-top: 10px;"
                >
                    ${phase2ChecklistHtml}
                </div>

                <small
                    style="
                        display: block;
                        margin-top: 8px;
                        color: #15803d;
                    "
                >
                    ${phase2CompletedCount} of
                    ${phase2Tasks.length} completed
                </small>
            `;
        } else if (vendorIsPhase2) {
            phase2SectionHtml = `
                <div class="callout info">
                    <strong>
                        Phase 2 is active.
                    </strong>

                    Complete every transition and
                    cutover milestone below.
                </div>

                <div
                    class="list"
                    style="margin-top: 10px;"
                >
                    ${phase2ChecklistHtml}
                </div>

                <small
                    style="
                        display: block;
                        margin-top: 8px;
                        color: #64748b;
                    "
                >
                    ${phase2CompletedCount} of
                    ${phase2Tasks.length} completed
                </small>
            `;
        } else {
            phase2SectionHtml = `
                <div class="callout warning">
                    Phase 2 will become available
                    after Phase 1 cutover approval.
                </div>
            `;
        }


        /* =============================================
           PRIMARY BUTTON
        ============================================= */

        let primaryActionHtml = "";

        if (vendorIsFullyUnified) {
            primaryActionHtml = `
                <button
                    class="btn primary"
                    type="button"
                    disabled
                >
                    Fully Unified Vendor
                </button>
            `;
        } else if (vendorIsPhase2) {
            primaryActionHtml = `
                <button
                    id="complete-phase-two-button"
                    class="btn primary"
                    type="button"
                    ${phase2Complete ? "" : "disabled"}
                >
                    Mark as Fully Unified
                </button>
            `;
        } else {
            primaryActionHtml = `
                <button
                    id="approve-cutover-button"
                    class="btn primary"
                    type="button"
                    ${phase1Complete ? "" : "disabled"}
                >
                    Approve Cutover
                </button>
            `;
        }


        /* =============================================
           BADGE
        ============================================= */

        let badgeHtml = "";

        if (
            window.UI &&
            typeof window.UI.badge === "function"
        ) {
            badgeHtml = window.UI.badge(
                vendorIsFullyUnified
                    ? "Fully Unified"
                    : vendorIsPhase2
                        ? "Phase 2"
                        : vendor.status
            );
        }


        /* =============================================
           PAGE OUTPUT
        ============================================= */

        return `
            <div id="onboarding-page-root">
                <style>
                    .onboarding-modal-backdrop {
                        position: fixed;
                        inset: 0;
                        z-index: 9999;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        padding: 20px;
                        background:
                            rgba(15, 23, 42, 0.55);
                    }

                    .onboarding-modal {
                        width: 100%;
                        max-width: 620px;
                        max-height: 85vh;
                        overflow-y: auto;
                        border-radius: 10px;
                        background: #ffffff;
                        box-shadow:
                            0 20px 35px
                            rgba(15, 23, 42, 0.2);
                    }

                    .onboarding-modal-header {
                        display: flex;
                        align-items: flex-start;
                        justify-content: space-between;
                        gap: 16px;
                        padding: 18px 20px;
                        border-bottom:
                            1px solid #e2e8f0;
                    }

                    .onboarding-modal-header h3 {
                        margin: 0 0 5px;
                        color: #0f172a;
                        font-size: 17px;
                    }

                    .onboarding-modal-header p {
                        margin: 0;
                        color: #64748b;
                        font-size: 12px;
                    }

                    .onboarding-modal-body {
                        padding: 20px;
                    }

                    .onboarding-modal-actions {
                        display: flex;
                        justify-content: flex-end;
                        gap: 8px;
                        padding: 14px 20px;
                        border-top:
                            1px solid #e2e8f0;
                    }

                    .modal-close-button {
                        padding: 0;
                        border: 0;
                        background: transparent;
                        color: #64748b;
                        font-size: 24px;
                        line-height: 1;
                        cursor: pointer;
                    }

                    .onboarding-approval-summary {
                        display: grid;
                        gap: 10px;
                        margin-top: 16px;
                    }

                    .onboarding-approval-summary > div {
                        display: flex;
                        justify-content:
                            space-between;
                        gap: 20px;
                        padding: 10px 0;
                        border-bottom:
                            1px solid #e2e8f0;
                        font-size: 13px;
                    }

                    .onboarding-approval-summary span {
                        color: #64748b;
                    }

                    .fully-unified-callout {
                        margin-top: 16px;
                        padding: 16px;
                        border:
                            1px solid #bbf7d0;
                        border-radius: 8px;
                        background: #f0fdf4;
                        color: #166534;
                    }

                    .fully-unified-callout strong {
                        display: block;
                        margin-bottom: 5px;
                    }

                    .btn:disabled {
                        cursor: not-allowed;
                        opacity: 0.5;
                    }

                    @media (max-width: 640px) {
                        .onboarding-modal-actions {
                            flex-direction: column;
                        }

                        .onboarding-modal-actions .btn {
                            width: 100%;
                        }

                        .onboarding-approval-summary > div {
                            flex-direction: column;
                            gap: 4px;
                        }
                    }
                </style>

                <div class="tabs">
                    <a
                        class="tab"
                        href="#"
                        onclick="
                            App.navigate('imports');
                            return false;
                        "
                    >
                        Import & Mapping Wizard
                    </a>

                    <a
                        class="tab"
                        href="#"
                        onclick="
                            App.navigate('duplicates');
                            return false;
                        "
                    >
                        Duplicate Review Queue
                    </a>

                    <a
                        class="tab active"
                        href="#"
                        onclick="
                            App.navigate('onboarding');
                            return false;
                        "
                    >
                        Vendor Onboarding Readiness
                    </a>
                </div>

                <article
                    class="card"
                    style="margin-bottom: 20px;"
                >
                    <div class="card-title">
                        <div>
                            <h2>
                                ${escapeHtml(vendor.name)}
                                (${escapeHtml(vendor.id)})
                            </h2>

                            <p>
                                Current Status:
                                <b>
                                    ${escapeHtml(currentStatus)}
                                </b>

                                · Readiness Score:
                                <b>
                                    ${escapeHtml(readinessScore)}
                                </b>
                            </p>
                        </div>

                        ${badgeHtml}
                    </div>

                    <div
                        class="stepper"
                        style="margin: 16px 0;"
                    >
                        <div class="step done">
                            1 · Discovery
                        </div>

                        <div class="step done">
                            2 · Mapping
                        </div>

                        <div class="step done">
                            3 · Validation
                        </div>

                        <div class="step ${
                            vendorIsPhase2 ||
                            vendorIsFullyUnified
                                ? "done"
                                : "active"
                        }">
                            4 · Phase 1 Coexist Live
                        </div>

                        <div class="step ${
                            vendorIsFullyUnified
                                ? "done"
                                : vendorIsPhase2
                                    ? "active"
                                    : ""
                        }">
                            5 · Phase 2 Cutover Planning
                        </div>

                        <div class="step ${
                            vendorIsFullyUnified
                                ? "active"
                                : ""
                        }">
                            6 · Fully Unified
                        </div>
                    </div>

                    ${
                        vendorIsFullyUnified
                            ? `
                                <div class="fully-unified-callout">
                                    <strong>
                                        ✓ Fully Unified Vendor
                                    </strong>

                                    ${escapeHtml(vendor.name)}
                                    has completed Phase 1
                                    coexistence and Phase 2
                                    cutover.

                                    The vendor is now fully
                                    operating through HVAC Connect.
                                </div>
                            `
                            : vendorIsPhase2
                                ? `
                                    <div class="callout info">
                                        <strong>
                                            Phase 2 Cutover Planning Active:
                                        </strong>

                                        Phase 1 has been approved.
                                        Complete the Phase 2
                                        checklist to make the
                                        vendor a Fully Unified
                                        Vendor.
                                    </div>
                                `
                                : `
                                    <div class="callout info">
                                        <strong>
                                            Phase 1 Coexistence Active:
                                        </strong>

                                        Local field teams and office
                                        staff continue running daily
                                        operations in Pacific Climate's
                                        legacy software.

                                        Customer and asset data is
                                        consolidated into HVAC Connect
                                        without business disruption.
                                    </div>
                                `
                    }

                    <div
                        class="grid two-col"
                        style="margin-top: 20px;"
                    >
                        <div>
                            <h3
                                style="
                                    font-size: 14px;
                                    font-weight: 700;
                                    margin-bottom: 8px;
                                "
                            >
                                Phase 1: Coexist & Ingestion Checklist
                            </h3>

                            <div class="list">
                                ${phase1ChecklistHtml}
                            </div>

                            <small
                                style="
                                    display: block;
                                    margin-top: 8px;
                                    color: #64748b;
                                "
                            >
                                ${phase1CompletedCount} of
                                ${phase1Tasks.length} completed
                            </small>
                        </div>

                        <div>
                            <h3
                                style="
                                    font-size: 14px;
                                    font-weight: 700;
                                    margin-bottom: 8px;
                                "
                            >
                                Phase 2: Transition & Cutover Readiness
                            </h3>

                            ${phase2SectionHtml}
                        </div>
                    </div>

                    <div
                        style="
                            display: flex;
                            justify-content: flex-end;
                            gap: 8px;
                            margin-top: 20px;
                            border-top:
                                1px solid #e2e8f0;
                            padding-top: 14px;
                        "
                    >
                        <button
                            id="download-readiness-button"
                            class="btn"
                            type="button"
                        >
                            Download Readiness PDF
                        </button>

                        ${primaryActionHtml}
                    </div>
                </article>
            </div>
        `;
    };
})();