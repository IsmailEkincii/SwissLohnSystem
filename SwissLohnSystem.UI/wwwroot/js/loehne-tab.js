// wwwroot/js/loehne-tab.js
(function () {
    console.log("[loehne-tab] script loaded");

    const API_BASE_URL = (window.API_BASE_URL || "").replace(/\/+$/, "");
    const COMPANY_ID = window.COMPANY_ID || 0;

    console.log("[loehne-tab] API_BASE_URL =", API_BASE_URL);
    console.log("[loehne-tab] COMPANY_ID =", COMPANY_ID);

    if (!API_BASE_URL || !COMPANY_ID) {
        console.warn("[loehne-tab] Missing API_BASE_URL or COMPANY_ID");
        return;
    }

    const inputPeriod = document.getElementById("period");
    const btnLoad = document.getElementById("btnLoadLohns");
    const tblBody = document.querySelector("#tblLohnMonthly tbody");

    // ---- Helpers ----
    function showError(msg) {
        if (window.toastr && toastr.error) toastr.error(msg);
        else alert("Fehler: " + msg);
    }

    function getCurrentPeriod() {
        const d = new Date();
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, "0");
        return `${y}-${m}`; // YYYY-MM
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
        if (isFinal) {
            return '<span class="badge badge-success">Final</span>';
        }
        return '<span class="badge badge-warning">Entwurf</span>';
    }

    // =========================
    // Lohn liste (tablo)
    // =========================
    async function loadMonthly() {
        if (!tblBody) return;

        const period = (inputPeriod && inputPeriod.value)
            ? inputPeriod.value
            : getCurrentPeriod();

        if (inputPeriod && !inputPeriod.value) {
            inputPeriod.value = period;
        }

        const url = `${API_BASE_URL}/api/Lohn/by-company/${COMPANY_ID}/monthly?period=${encodeURIComponent(period)}`;
        console.log("[loehne-tab] GET monthly loehne", url);

        tblBody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-muted p-3">
                    Löhne werden geladen...
                </td>
            </tr>`;

        try {
            const res = await fetch(url);
            if (!res.ok) {
                console.error("[loehne-tab] monthly http error", res.status);
                tblBody.innerHTML = `
                    <tr>
                        <td colspan="7" class="text-center text-danger p-3">
                            Fehler beim Laden der Löhne.
                        </td>
                    </tr>`;
                return;
            }

            const payload = await res.json();
            console.log("[loehne-tab] monthly response", payload);

            if (!payload.success || !payload.data || payload.data.length === 0) {
                tblBody.innerHTML = `
                    <tr>
                        <td colspan="7" class="text-center text-muted p-3">
                            Keine Löhne für diesen Monat vorhanden.
                        </td>
                    </tr>`;
                return;
            }

            const rows = payload.data; // CompanyMonthlyLohnDto listesi
            tblBody.innerHTML = "";

            rows.forEach(row => {
                const tr = document.createElement("tr");
                const periodLabel = `${String(row.month).padStart(2, "0")}.${row.year}`;
                const statusHtml = formatStatus(!!row.isFinal);
                const empName = row.employeeName || ("#" + row.employeeId);

                tr.innerHTML = `
                    <td>${empName}</td>
                    <td>${periodLabel}</td>
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

            // PDF butonları: Detay sayfasını yeni sekmede aç (orada print/PDF var)
            const pdfButtons = tblBody.querySelectorAll(".btn-lohn-pdf");
            pdfButtons.forEach(btn => {
                btn.addEventListener("click", function () {
                    const id = this.getAttribute("data-id");
                    if (!id) return;
                    const url = "/Lohn/Details/" + id;
                    window.open(url, "_blank");
                });
            });

        } catch (err) {
            console.error("[loehne-tab] monthly exception", err);
            tblBody.innerHTML = `
                <tr>
                    <td colspan="7" class="text-center text-danger p-3">
                        Fehler beim Laden der Löhne (Netzwerkfehler).
                    </td>
                </tr>`;
        }
    }

    // =========================
    // Event binding
    // =========================
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
