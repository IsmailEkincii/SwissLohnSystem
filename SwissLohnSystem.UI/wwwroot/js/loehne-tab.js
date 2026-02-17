// wwwroot/js/loehne-tab.js
(function () {
    console.log("[loehne-tab] script loaded");

    const API_BASE_URL = (window.API_BASE_URL || "").replace(/\/+$/, "");
    const COMPANY_ID = Number(window.COMPANY_ID || 0);

    console.log("[loehne-tab] API_BASE_URL =", API_BASE_URL);
    console.log("[loehne-tab] COMPANY_ID =", COMPANY_ID);

    if (!API_BASE_URL || !COMPANY_ID) {
        console.warn("[loehne-tab] Missing API_BASE_URL or COMPANY_ID");
        return;
    }

    const inputPeriod = document.getElementById("period");
    const btnLoad = document.getElementById("btnLoadLohns");
    const tblBody = document.querySelector("#tblLohnMonthly tbody");

    if (!tblBody) {
        console.warn("[loehne-tab] Missing table body (#tblLohnMonthly tbody)");
        return;
    }

    // -------------------------
    // Helpers
    // -------------------------
    function showError(msg) {
        if (window.toastr && typeof toastr.error === "function") toastr.error(msg);
        else alert("Fehler: " + msg);
    }

    function showInfo(msg) {
        if (window.toastr && typeof toastr.info === "function") toastr.info(msg);
        else console.log(msg);
    }

    function showSuccess(msg) {
        if (window.toastr && typeof toastr.success === "function") toastr.success(msg);
        else console.log(msg);
    }

    function getCurrentPeriod() {
        const d = new Date();
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, "0");
        return `${y}-${m}`; // YYYY-MM
    }

    function parsePeriod(period) {
        // period: YYYY-MM
        const parts = String(period || "").split("-");
        const year = Number(parts[0] || 0);
        const month = Number(parts[1] || 0);

        if (!Number.isFinite(year) || year < 1900) return null;
        if (!Number.isFinite(month) || month < 1 || month > 12) return null;

        return { year, month };
    }

    function normalizeNumber(value) {
        if (value === null || value === undefined || value === "") return null;
        const n = Number(value);
        if (!isFinite(n)) return null;
        return n;
    }

    function formatMoney(value) {
        const n = normalizeNumber(value);
        if (n === null) return "–";
        return n.toFixed(2) + " CHF";
    }

    function formatStatus(isFinal) {
        if (isFinal) return '<span class="badge badge-success">Final</span>';
        return '<span class="badge badge-warning">Entwurf</span>';
    }

    function renderLoading() {
        tblBody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-muted p-3">
                    Löhne werden geladen...
                </td>
            </tr>`;
    }

    function renderEmpty() {
        tblBody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-muted p-3">
                    Keine Löhne für diesen Monat vorhanden.
                </td>
            </tr>`;
    }

    function renderErrorRow(message) {
        tblBody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-danger p-3">
                    ${message || "Fehler beim Laden der Löhne."}
                </td>
            </tr>`;
    }

    async function fetchJson(url) {
        const res = await fetch(url, { headers: { "Accept": "application/json" } });
        let payload = null;

        try {
            payload = await res.json();
        } catch {
            payload = null;
        }

        return { res, payload };
    }

    // -------------------------
    // Load monthly rows
    // -------------------------
    async function loadMonthly() {
        const period = (inputPeriod && inputPeriod.value) ? inputPeriod.value : getCurrentPeriod();
        if (inputPeriod && !inputPeriod.value) inputPeriod.value = period;

        const p = parsePeriod(period);
        if (!p) {
            showError("Ungültiges Monat-Format. Bitte YYYY-MM wählen.");
            return;
        }

        const url = `${API_BASE_URL}/api/Lohn/by-company/${COMPANY_ID}?year=${p.year}&month=${p.month}`;
        console.log("[loehne-tab] GET", url);

        renderLoading();

        try {
            const { res, payload } = await fetchJson(url);

            // API envelope: { success, data, message }
            if (!res.ok) {
                const msg =
                    (payload && payload.message) ||
                    `HTTP ${res.status} – Fehler beim Laden.`;
                console.error("[loehne-tab] http error", res.status, payload);
                renderErrorRow(msg);
                showError(msg);
                return;
            }

            if (!payload || payload.success !== true) {
                const msg = (payload && payload.message) || "Fehler: API response ungültig.";
                console.error("[loehne-tab] invalid payload", payload);
                renderErrorRow(msg);
                showError(msg);
                return;
            }

            const rows = Array.isArray(payload.data) ? payload.data : [];
            if (rows.length === 0) {
                renderEmpty();
                return;
            }

            tblBody.innerHTML = "";

            rows.forEach(row => {
                const tr = document.createElement("tr");

                const periodLabel = `${String(row.month).padStart(2, "0")}.${row.year}`;
                const statusHtml = formatStatus(!!row.isFinal);
                const empName = row.employeeName || ("#" + row.employeeId);

                tr.innerHTML = `
                    <td>${escapeHtml(empName)}</td>
                    <td>${escapeHtml(periodLabel)}</td>
                    <td class="text-right">${formatMoney(row.bruttoSalary)}</td>
                    <td class="text-right">${formatMoney(row.totalDeductions)}</td>
                    <td class="text-right">${formatMoney(row.netSalary)}</td>
                    <td>${statusHtml}</td>
                    <td>
                        <a class="btn btn-xs btn-outline-primary mr-1"
                           href="/Lohn/Details/${row.id}">
                            <i class="fas fa-search"></i> Details
                        </a>
                        <button type="button"
                                class="btn btn-xs btn-outline-secondary btn-lohn-pdf"
                                data-id="${row.id}">
                            <i class="fas fa-file-pdf"></i> PDF
                        </button>
                    </td>
                `;

                tblBody.appendChild(tr);
            });

            // PDF actions
            const pdfButtons = tblBody.querySelectorAll(".btn-lohn-pdf");
            pdfButtons.forEach(btn => {
                btn.addEventListener("click", async function () {
                    const id = this.getAttribute("data-id");
                    if (!id) return;

                    const pdfUrl = `${API_BASE_URL}/api/Lohn/${id}/pdf`;
                    console.log("[loehne-tab] open pdf", pdfUrl);

                    // API: final değilse 409 Conflict
                    try {
                        const res = await fetch(pdfUrl, { method: "GET" });
                        if (!res.ok) {
                            let msg = `PDF konnte nicht erstellt werden (HTTP ${res.status}).`;
                            try {
                                const json = await res.json();
                                msg = (json && json.message) ? json.message : msg;
                            } catch { }
                            showError(msg);
                            return;
                        }

                        // PDF blob
                        const blob = await res.blob();
                        const blobUrl = URL.createObjectURL(blob);
                        window.open(blobUrl, "_blank");

                        // cleanup (biraz sonra)
                        setTimeout(() => URL.revokeObjectURL(blobUrl), 60_000);
                    } catch (e) {
                        console.error("[loehne-tab] pdf error", e);
                        showError("Netzwerkfehler beim PDF Download.");
                    }
                });
            });

        } catch (err) {
            console.error("[loehne-tab] exception", err);
            renderErrorRow("Fehler beim Laden der Löhne (Netzwerkfehler).");
            showError("Fehler beim Laden der Löhne (Netzwerkfehler).");
        }
    }

    // basic XSS-safe rendering for strings
    function escapeHtml(str) {
        return String(str ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    // -------------------------
    // Init
    // -------------------------
    document.addEventListener("DOMContentLoaded", function () {
        console.log("[loehne-tab] DOMContentLoaded");

        const current = getCurrentPeriod();
        if (inputPeriod && !inputPeriod.value) inputPeriod.value = current;

        loadMonthly();

        if (btnLoad) {
            btnLoad.addEventListener("click", function () {
                loadMonthly();
            });
        }
    });
})();
