/**
 * Page: Customer Registry & Profiles (PB-019, PB-020, PB-022 / US018, US019, US021)
 */
window.Pages = window.Pages || {};
let customerPageRows = [];
let customerPagination = null;
let customerPaginationReady = false;
function getCustomerPagination() {
    if (!customerPagination) {
        customerPagination = App.createPagination(
            Store.customers,
            4,
            (paginatedData, pageInfo) => {
                customerPageRows = paginatedData;
                if (customerPaginationReady && location.hash.replace(/^#/, "") === "customers") {
                    Router.loadCurrentPage();
                }
            },
            "customers",
            {
                searchText: customer => [
                    customer.name,
                    customer.id,
                    customer.type,
                    customer.primaryContact,
                    customer.email,
                    customer.accountsCount,
                    customer.status,
                ].join(" "),
                filters: { 2: customer => customer.type },
            }
        );
        customerPagination.update();
        customerPaginationReady = true;
    }
    return customerPagination;
}
window.Pages["customers"] = function () {
    const user = Auth.getCurrentUser();
    const showCustomerType = !user || user.roleKey !== "vendoradmin";
    getCustomerPagination();
    const rows = customerPageRows.map(c => [
        `<b>${c.name}</b>`,
        `<code>${c.id}</code>`,
        ...(showCustomerType ? [c.type] : []),
        `<b>${c.primaryContact}</b><br><small style="color:#64748b">${c.email}</small>`,
        `<span class="badge blue">${c.accountsCount} linked brands</span>`,
        UI.badge(c.status),
    ]);

    return `
        <article class="card">
            <div class="table-tools">
                <input class="filter-input" placeholder="Search customer name, canonical ID, or email..." data-filter onkeyup="App.globalSearch()">
                ${showCustomerType ? `
                    <select class="filter-input" data-column="2" onchange="App.globalSearch()">
                        <option value="">All customer types</option>
                        <option value="Commercial">Commercial</option>
                        <option value="Healthcare">Healthcare</option>
                        <option value="Retail">Retail</option>
                    </select>
                ` : ""}
            </div>

            <div class="card-title">
                <div>
                    <h3>Canonical Customer Directory</h3>
                    <p>Group-wide master records unifying customer relationships across multiple acquired brands.</p>
                </div>
            </div>

            ${UI.table([
                "Customer Name",
                "Canonical ID",
                ...(showCustomerType ? ["Type"] : []),
                "Primary Contact",
                "Brand Accounts",
                "Status"
            ], rows)}

            ${UI.pagination(customerPagination.getPageInfo(), "customers")}

            <div class="list-row" style="margin-top: 14px; padding-top: 10px;">
                <small style="color:#64748b">Showing ${Store.customers.length} unified canonical customers</small>
                <small style="color:#64748b">Preserves vendor-specific legacy account numbers separately</small>
            </div>
        </article>
    `;
};
