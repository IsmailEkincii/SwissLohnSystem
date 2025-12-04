console.log("[loehne-tab] script loaded");

(function () {
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
                <td colspan="6" class="text-center text-muted p-3">
                    Lade Löhne...
                </td>
            </tr>`;

        try {
            const res = await fetch(url);
            if (!res.ok) {
                console.error("[loehne-tab] monthly http error", res.status);
                tblBody.innerHTML = `
                    <tr>
                        <td colspan="6" class="text-center text-danger p-3">
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
                        <td colspan="6" class="text-center text-muted p-3">
                            Keine Löhne für diesen Monat vorhanden.
                        </td>
                    </tr>`;
                return;
            }

            tblBody.innerHTML = "";
            payload.data.forEach(row => {
                const tr = document.createElement("tr");
                const periodLabel = `${String(row.month).padStart(2, "0")}.${row.year}`;

                tr.innerHTML = `
                    <td>${row.employeeName || ("#" + row.employeeId)}</td>
                    <td>${periodLabel}</td>
                    <td>${formatMoney(row.bruttoSalary)}</td>
                    <td>${formatMoney(row.netSalary)}</td>
                    <td>
                        <span class="badge badge-${row.isFinal ? "success" : "warning"}">
                            ${row.isFinal ? "Final" : "Entwurf"}
                        </span>
                    </td>
                    <td>
                        <a class="btn btn-xs btn-outline-secondary"
                           href="/Lohn/Details/${row.id}">
                            <i class="fas fa-file-alt"></i> Details
                        </a>
                    </td>
                `;

                tblBody.appendChild(tr);
            });

        } catch (err) {
            console.error("[loehne-tab] monthly exception", err);
            tblBody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center text-danger p-3">
                        Fehler beim Laden der Löhne.
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
