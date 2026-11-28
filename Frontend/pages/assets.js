window.Pages = window.Pages || {};
let assetPageRows = [];
let assetPagination = null;
let assetPaginationReady = false;
function getAssetPagination() {
    if (!assetPagination) {
        assetPagination = App.createPagination(
            Store.assets,
            4,
            (paginatedData, pageInfo) => {
                assetPageRows = paginatedData;
                if (assetPaginationReady && location.hash.replace(/^#/, "") === "assets") {
                    Router.loadCurrentPage();
                }
            },
            "assets",
            {
                searchText: asset => [
                    asset.id,
                    asset.category,
                    asset.categoryCode,
                    asset.manufacturer,
                    asset.model,
                    asset.serial,
                    asset.refrigerant,
                    asset.locationName,
                    asset.warrantyEnd,
                ].join(" "),
                filters: { category: asset => asset.category },
            }
        );
        assetPagination.update();
        assetPaginationReady = true;
    }
    return assetPagination;
}
window.Pages["assets"] = function() {
    getAssetPagination();
    return `
        <div class="table-tools">
            <input class="filter-input" placeholder="Search asset tag, equipment type, model or serial number..." data-filter onkeyup="App.globalSearch()">
            <select class="filter-input" data-column="category" id="equipmentCategoryFilter" onchange="App.globalSearch()">
                <option value="">All 5 MVP Categories</option>
                <option value="Rooftop Unit">Rooftop Units (RTU)</option>
                <option value="Chiller">Chillers (CH)</option>
                <option value="Boiler">Boilers (BLR)</option>
                <option value="Walk-in">Walk-in Coolers / Freezers (WIK)</option>
                <option value="Refrigeration Rack">Refrigeration Racks (RCK)</option>
            </select>
            <button class="btn primary" onclick="App.openCreateAssetModal()">+ Add Equipment Unit</button>
        </div>

        <div class="grid three-col" id="assetsGrid">
            ${assetPageRows.map(a => `
                <article class="card asset-card" data-card data-category="${a.category}">
                    <div>
                        <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom: 8px;">
                            <span class="asset-symbol">${a.categoryCode}</span>
                            ${UI.badge(a.status)}
                        </div>
                        <h3 style="font-size:16px; margin-bottom: 2px;">${a.id}</h3>
                        <p class="asset-type">${a.category}</p>
                        
                        <div class="info-grid" style="grid-template-columns: 1fr 1fr; padding: 10px; gap: 8px; margin-bottom: 10px;">
                            <div class="info-item">
                                <small>Manufacturer</small>
                                <strong>${a.manufacturer}</strong>
                            </div>
                            <div class="info-item">
                                <small>Model</small>
                                <strong>${a.model}</strong>
                            </div>
                            <div class="info-item">
                                <small>Serial Number</small>
                                <code>${a.serial}</code>
                            </div>
                            <div class="info-item">
                                <small>Refrigerant</small>
                                <strong>${a.refrigerant}</strong>
                            </div>
                        </div>

                        <div style="font-size: 11px; color: #64748b; margin-bottom: 4px;">
                            <b>Location:</b> ${a.locationName}
                        </div>
                        <div style="font-size: 11px; color: #64748b;">
                            <b>Warranty Until:</b> ${a.warrantyEnd}
                        </div>
                    </div>

                    <div class="asset-actions">
                        <a href="#" onclick="App.openAssetDetail('${a.id}'); return false;">View Specifications & Repair History →</a>
                    </div>
                </article>
            `).join("")}
        </div>
        ${UI.pagination(assetPagination.getPageInfo(), "assets")}
    `;
};

