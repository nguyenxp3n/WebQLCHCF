(function () {
    const app = window.WebQLCHCF = window.WebQLCHCF || {};

    app.initOrderCreate = function (container) {
        const root = container || document;
        const page = root.matches?.("[data-page='order-create']")
            ? root
            : root.querySelector?.("[data-page='order-create']");

        if (!page || page.dataset.orderCreateInitialized === "true") {
            return;
        }
        page.dataset.orderCreateInitialized = "true";

        const config = window.orderCreateConfig || {};
        const currency = new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND",
            maximumFractionDigits: 0
        });

        const rows = Array.from(page.querySelectorAll(".order-product-row"));
        const productBody = page.querySelector("#order-product-body");
        const cartItems = page.querySelector("#cart-items");
        const cartEmpty = page.querySelector("#cart-empty-state");
        const customerIdInput = page.querySelector("#order-customer-id");
        const customerPhoneSearch = page.querySelector("#customer-phone-search");
        const customerSuggestions = page.querySelector("#customer-suggestions");
        const customerStatus = page.querySelector("#customer-search-status");
        const selectedCustomerBadge = page.querySelector("#selectedCustomerBadge");
        const cartCustomerName = page.querySelector("#cart-customer-name");
        const quickCustomerPhone = page.querySelector("#quick-customer-phone");
        const quickCustomerName = page.querySelector("#quick-customer-name");
        const quickCustomerError = page.querySelector("#quick-customer-error");
        const quickCustomerInlineButton = page.querySelector("#quick-customer-inline-btn");
        const tableModeLabel = page.querySelector("#tableModeLabel");
        const tableNextLabel = page.querySelector("#table-next-label");
        const areaSelector = page.querySelector("#order-area-selector");
        const areaWarning = page.querySelector("#order-area-warning");

        if (!productBody || !cartItems || !customerIdInput) {
            return;
        }

        let selectedCustomer = null;
        let customerSearchTimer = null;

        function $(selector) {
            return page.querySelector(selector);
        }

        function money(value) {
            const safeValue = Number.isFinite(value) ? value : 0;
            return currency.format(Math.max(0, safeValue));
        }

        function numeric(value) {
            if (typeof value === "number") {
                return value;
            }
            return Number(String(value || "0").replace(",", ".")) || 0;
        }

        function escapeHtml(value) {
            return String(value ?? "")
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }

        function promotionField(item, pascal, camel) {
            return item[pascal] ?? item[camel];
        }

        function tableField(item, pascal, camel) {
            return item?.[pascal] ?? item?.[camel] ?? "";
        }

        function findAreaTable(area) {
            return (config.areaTables || []).find(item => tableField(item, "KhuVuc", "khuVuc") === area);
        }

        function setAreaWarning(message) {
            if (!areaWarning) {
                return;
            }
            areaWarning.textContent = message || "";
            areaWarning.classList.toggle("d-none", !message);
        }

        function renderAreaTablePreview() {
            if (config.isEditing || !tableNextLabel) {
                return;
            }

            const groundFloor = "Tầng trệt";
            let area = page.querySelector("input[name='KhuVuc']:checked")?.value || groundFloor;
            let table = findAreaTable(area);

            if (!table) {
                setAreaWarning("Khu vực này đã hết bàn trống.");
                if (area !== groundFloor) {
                    const defaultAreaInput = page.querySelector(`input[name='KhuVuc'][value='${groundFloor}']`);
                    if (defaultAreaInput) {
                        defaultAreaInput.checked = true;
                    }
                    area = groundFloor;
                    table = findAreaTable(area);
                }
            } else {
                setAreaWarning("");
            }

            if (!table) {
                const hasAnyTable = (config.areaTables || []).length > 0;
                if (!hasAnyTable) {
                    setAreaWarning("Hiện không còn bàn trống tại quán.");
                }
                tableNextLabel.textContent = hasAnyTable
                    ? "Khu vực này đã hết bàn trống."
                    : "Hiện không còn bàn trống tại quán.";
                return;
            }

            const name = tableField(table, "TenBan", "tenBan");
            const code = tableField(table, "MaBan", "maBan");
            tableNextLabel.innerHTML = `Bàn trống tiếp theo: <strong>${escapeHtml(name)}</strong> (${escapeHtml(code)})`;
        }

        function getPromotion(maKM) {
            return (config.promotions || []).find(item =>
                String(promotionField(item, "MaKM", "maKM") || "") === String(maKM || "")
            );
        }

        function calcDiscount(base, maKM) {
            const promotion = getPromotion(maKM);
            if (!promotion || base <= 0) {
                return 0;
            }

            const type = String(promotionField(promotion, "LoaiKM", "loaiKM") || "");
            const value = numeric(promotionField(promotion, "GiaTri", "giaTri"));
            if (type === "Phần trăm") {
                return value >= 100 ? base : Math.min(base, base * value / 100);
            }
            if (type === "Tiền mặt") {
                return Math.min(base, value);
            }
            return 0;
        }

        function getRowData(row) {
            const qtyInput = row.querySelector(".quantity-input");
            const promoInput = row.querySelector(".order-promo-source");
            const qty = Math.max(0, parseInt(qtyInput.value || "0", 10) || 0);
            const price = numeric(row.dataset.price || qtyInput.dataset.price);
            const base = qty * price;
            const maKM = promoInput ? promoInput.value : "";
            const discount = calcDiscount(base, maKM);

            return {
                index: row.dataset.index,
                code: row.dataset.code,
                name: row.dataset.name,
                category: row.dataset.category,
                categoryName: row.dataset.categoryName,
                price,
                qty,
                maKM,
                base,
                discount,
                final: Math.max(0, base - discount),
                qtyInput,
                promoInput
            };
        }

        function setQty(row, qty) {
            const input = row.querySelector(".quantity-input");
            input.value = Math.max(0, qty);
            updateCart();
        }

        function buildPromoSelect(data) {
            const select = document.createElement("select");
            select.className = "form-select form-select-sm cart-promo-select";
            select.dataset.index = data.index;
            select.innerHTML = '<option value="">Không áp dụng khuyến mãi</option>';

            (config.promotions || []).forEach(item => {
                const maKM = promotionField(item, "MaKM", "maKM") || "";
                const tenKM = promotionField(item, "TenKM", "tenKM") || maKM;
                const option = document.createElement("option");
                option.value = maKM;
                option.textContent = tenKM;
                option.selected = maKM === data.maKM;
                select.appendChild(option);
            });

            return select;
        }

        function renderCartLine(data) {
            const item = document.createElement("div");
            item.className = "cart-line";
            item.dataset.index = data.index;

            item.innerHTML = `
                <div class="cart-line-head">
                    <div>
                        <div class="fw-bold text-primary">${escapeHtml(data.name)}</div>
                        <div class="small text-muted">${escapeHtml(data.code)} · ${money(data.price)}</div>
                    </div>
                    <button type="button" class="btn btn-sm btn-link text-danger cart-remove" title="Xóa món">
                        <i class="bi bi-x-circle-fill"></i>
                    </button>
                </div>
                <div class="cart-line-controls">
                    <div class="input-group input-group-sm order-qty-control">
                        <button type="button" class="btn btn-outline-secondary cart-minus"><i class="bi bi-dash"></i></button>
                        <input type="text" class="form-control text-center fw-bold" value="${data.qty}" readonly />
                        <button type="button" class="btn btn-outline-secondary cart-plus"><i class="bi bi-plus"></i></button>
                    </div>
                    <div class="text-end">
                        <div class="fw-bold">${money(data.final)}</div>
                        ${data.discount > 0 ? `<div class="small text-warning">Giảm ${money(data.discount)}</div>` : ""}
                    </div>
                </div>
            `;

            const promoWrap = document.createElement("div");
            promoWrap.className = "mt-2";
            promoWrap.appendChild(buildPromoSelect(data));
            item.appendChild(promoWrap);
            return item;
        }

        function applyFilters() {
            const keyword = ($("#product-search")?.value || "").trim().toLowerCase();
            const category = $("#category-filter")?.value || "";
            const selectedOnly = $("#selected-only-filter")?.checked || false;

            rows.forEach(row => {
                const data = getRowData(row);
                const matchesKeyword = !keyword
                    || String(data.code).toLowerCase().includes(keyword)
                    || String(data.name).toLowerCase().includes(keyword);
                const matchesCategory = !category || data.category === category;
                const matchesSelected = !selectedOnly || data.qty > 0;
                row.hidden = !(matchesKeyword && matchesCategory && matchesSelected);
            });
        }

        function sortSelectedRowsFirst() {
            rows.slice()
                .sort((a, b) => {
                    const da = getRowData(a);
                    const db = getRowData(b);
                    if ((da.qty > 0) !== (db.qty > 0)) {
                        return da.qty > 0 ? -1 : 1;
                    }
                    return String(da.name).localeCompare(String(db.name), "vi");
                })
                .forEach(row => productBody.appendChild(row));
        }

        function updateServiceMode() {
            const mode = page.querySelector("input[name='HinhThuc']:checked")?.value || "TaiQuan";
            const takeAway = mode === "MangVe";
            $("#cart-service-mode").textContent = takeAway ? "Mang về" : "Tại quán";
            if (areaSelector) {
                areaSelector.hidden = takeAway;
            }

            if (tableModeLabel) {
                tableModeLabel.textContent = config.isEditing
                    ? "Đang sửa order"
                    : (takeAway ? "Bàn MVE" : "Tự chọn bàn trống");
            }

            $("#serviceModeHint").textContent = config.isEditing
                ? "Bàn hiện tại được giữ nguyên khi sửa order."
                : (takeAway
                    ? "Đơn mang về dùng MVE và không cập nhật bàn vật lý."
                    : "Hệ thống sẽ tự chọn một bàn trống khi tạo hóa đơn.");

            if (tableNextLabel) {
                tableNextLabel.hidden = !config.isEditing && takeAway;
            }
            if (!takeAway) {
                renderAreaTablePreview();
            } else {
                setAreaWarning("");
            }
        }

        function updateCustomerDisplay() {
            const name = selectedCustomer ? selectedCustomer.tenKH : "Không tích điểm";
            selectedCustomerBadge.textContent = name;
            cartCustomerName.textContent = selectedCustomer
                ? `Khách hàng: ${name}`
                : "Tích điểm: Không";
            customerIdInput.value = selectedCustomer ? selectedCustomer.maKH : "";
        }

        function updateCart() {
            let itemCount = 0;
            let totalQty = 0;
            let subtotal = 0;
            let discount = 0;
            let grandTotal = 0;

            cartItems.querySelectorAll(".cart-line").forEach(item => item.remove());

            rows.forEach(row => {
                const data = getRowData(row);
                row.classList.toggle("is-selected", data.qty > 0);

                if (data.qty <= 0) {
                    return;
                }

                itemCount += 1;
                totalQty += data.qty;
                subtotal += data.base;
                discount += data.discount;
                grandTotal += data.final;
                cartItems.appendChild(renderCartLine(data));
            });

            cartEmpty.hidden = itemCount > 0;
            $("#cart-item-count").textContent = `${itemCount} món`;
            $("#cart-total-qty").textContent = totalQty;
            $("#cart-subtotal").textContent = money(subtotal);
            $("#cart-discount").textContent = money(discount);
            $("#cart-grand-total").textContent = money(grandTotal);
            $("#mobile-grand-total").textContent = money(grandTotal);
            updateServiceMode();
            updateCustomerDisplay();
            sortSelectedRowsFirst();
            const productScroller = page.querySelector(".order-products-scroll");
            if (itemCount > 0 && productScroller && productScroller.scrollTop > 0) {
                productScroller.scrollTop = 0;
            }
            applyFilters();
        }

        function normalizeCustomer(raw) {
            return {
                maKH: raw.MaKH ?? raw.maKH ?? "",
                tenKH: raw.TenKH ?? raw.tenKH ?? "",
                sdt: raw.SDT ?? raw.sdt ?? "",
                diemTichLuy: raw.DiemTichLuy ?? raw.diemTichLuy ?? 0,
                giamGiaCoTheApDung: raw.GiamGiaCoTheApDung ?? raw.giamGiaCoTheApDung ?? ""
            };
        }

        function selectCustomer(raw) {
            selectedCustomer = normalizeCustomer(raw);
            customerPhoneSearch.value = selectedCustomer.sdt;
            customerSuggestions.hidden = true;
            quickCustomerInlineButton?.classList.add("d-none");
            customerStatus.className = "small mt-2 text-success";
            customerStatus.textContent = `Đã chọn ${selectedCustomer.tenKH} để tích điểm.`;
            updateCart();
        }

        async function searchCustomers() {
            const keyword = (customerPhoneSearch.value || "").trim();
            quickCustomerPhone.value = keyword;

            if (keyword.length < 3) {
                customerSuggestions.hidden = true;
                quickCustomerInlineButton?.classList.add("d-none");
                customerStatus.className = "small mt-2 text-muted";
                customerStatus.textContent = "Không tích điểm";
                return;
            }

            const response = await fetch(`${config.searchCustomersUrl}?keyword=${encodeURIComponent(keyword)}`, {
                headers: { "Accept": "application/json" }
            });
            const customers = (await response.json()).map(normalizeCustomer);

            customerSuggestions.innerHTML = "";
            if (customers.length === 0) {
                customerSuggestions.hidden = true;
                quickCustomerInlineButton?.classList.remove("d-none");
                customerStatus.className = "small mt-2 text-warning";
                customerStatus.textContent = "Không tìm thấy khách. Hóa đơn vẫn tạo không tích điểm.";
                return;
            }

            quickCustomerInlineButton?.classList.add("d-none");
            customers.forEach(customer => {
                const button = document.createElement("button");
                button.type = "button";
                button.className = "customer-suggestion-item";
                button.innerHTML = `
                    <span class="fw-bold text-primary">${escapeHtml(customer.tenKH)}</span>
                    <span class="small text-muted">${escapeHtml(customer.maKH)} · ${escapeHtml(customer.sdt)}</span>
                    <span class="small">Điểm tích lũy: <strong>${customer.diemTichLuy || 0}</strong>${customer.giamGiaCoTheApDung ? ` · ${escapeHtml(customer.giamGiaCoTheApDung)}` : ""}</span>
                `;
                button.addEventListener("click", () => selectCustomer(customer));
                customerSuggestions.appendChild(button);
            });

            customerSuggestions.hidden = false;
            customerStatus.className = "small mt-2 text-muted";
            customerStatus.textContent = "Chọn khách trong gợi ý để tích điểm.";
        }

        async function saveQuickCustomer() {
            quickCustomerError.classList.add("d-none");
            quickCustomerError.textContent = "";

            const token = page.querySelector("input[name='__RequestVerificationToken']")?.value || "";
            const body = new URLSearchParams();
            body.set("__RequestVerificationToken", token);
            body.set("TenKH", quickCustomerName.value.trim());
            body.set("SDT", quickCustomerPhone.value.trim());

            const response = await fetch(config.quickCreateCustomerUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8",
                    "Accept": "application/json"
                },
                body
            });
            const result = await response.json();

            if (!result.success) {
                quickCustomerError.textContent = result.message || "Không thể thêm khách hàng.";
                quickCustomerError.classList.remove("d-none");
                return;
            }

            selectCustomer(result.customer);
            quickCustomerName.value = "";
            const modalElement = page.querySelector("#quickCustomerModal");
            const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
            modal.hide();
        }

        page.addEventListener("click", function (event) {
            const plus = event.target.closest(".btn-plus, .cart-plus");
            const minus = event.target.closest(".btn-minus, .cart-minus");
            const remove = event.target.closest(".cart-remove");

            if (plus || minus || remove) {
                const cartLine = event.target.closest(".cart-line");
                const row = cartLine
                    ? rows.find(item => item.dataset.index === cartLine.dataset.index)
                    : event.target.closest(".order-product-row");
                if (!row) {
                    return;
                }

                const data = getRowData(row);
                if (plus) {
                    setQty(row, data.qty + 1);
                } else if (minus) {
                    setQty(row, data.qty - 1);
                } else if (remove) {
                    setQty(row, 0);
                }
            }
        });

        page.addEventListener("input", function (event) {
            if (event.target.matches(".quantity-input")) {
                event.target.value = Math.max(0, parseInt(event.target.value || "0", 10) || 0);
                updateCart();
            }
            if (event.target.matches("#product-search")) {
                applyFilters();
            }
            if (event.target.matches("#customer-phone-search")) {
                selectedCustomer = null;
                updateCustomerDisplay();
                clearTimeout(customerSearchTimer);
                customerSearchTimer = setTimeout(searchCustomers, 250);
            }
        });

        page.addEventListener("keydown", function (event) {
            if (event.target.matches("#customer-phone-search") && event.key === "Enter") {
                event.preventDefault();
            }
        });

        page.addEventListener("change", function (event) {
            if (event.target.matches("input[name='HinhThuc']")) {
                updateCart();
            }
            if (event.target.matches("input[name='KhuVuc']")) {
                renderAreaTablePreview();
            }
            if (event.target.matches("#category-filter, #selected-only-filter")) {
                applyFilters();
            }
            if (event.target.matches(".cart-promo-select")) {
                const row = rows.find(item => item.dataset.index === event.target.dataset.index);
                if (row) {
                    row.querySelector(".order-promo-source").value = event.target.value;
                    updateCart();
                }
            }
        });

        $("#clear-cart-btn")?.addEventListener("click", function () {
            rows.forEach(row => setQty(row, 0));
        });

        $("#clear-customer-btn")?.addEventListener("click", function () {
            selectedCustomer = null;
            customerPhoneSearch.value = "";
            customerSuggestions.hidden = true;
            quickCustomerInlineButton?.classList.add("d-none");
            customerStatus.className = "small mt-2 text-muted";
            customerStatus.textContent = "Không tích điểm";
            updateCart();
        });

        $("#save-quick-customer-btn")?.addEventListener("click", saveQuickCustomer);

        $("#quickCustomerModal")?.addEventListener("show.bs.modal", function () {
            quickCustomerPhone.value = customerPhoneSearch.value.trim();
            quickCustomerError.classList.add("d-none");
            quickCustomerError.textContent = "";
        });

        if (config.initialCustomer && config.initialCustomer.maKH) {
            selectedCustomer = normalizeCustomer(config.initialCustomer);
            if (customerPhoneSearch) {
                customerPhoneSearch.value = selectedCustomer.sdt || "";
            }
            if (customerStatus) {
                customerStatus.className = "small mt-2 text-success";
                customerStatus.textContent = `Đã chọn ${selectedCustomer.tenKH} để tích điểm.`;
            }
        }

        updateCart();
    };

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            app.initOrderCreate(document);
        });
    } else {
        app.initOrderCreate(document);
    }
})();
