// wwwroot/js/loehne-tab.js
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

    const ddlEmployee = document.getElementById("employeeId");
    const inputPeriod = document.getElementById("period");
    const inputCalcMonth = document.getElementById("calcMonth");
    const btnLoad = document.getElementById("btnLoadLohns");
    const btnOpenModal = document.getElementById("btnOpenCalcModal");
    const btnDoCalc = document.getElementById("btnDoCalc");
    const tblBody = document.querySelector("#tblLohnMonthly tbody");

    // -------- Helpers --------
    function showSuccess(msg) {
        if (window.toastr && toastr.success) toastr.success(msg);
        else alert(msg);
    }
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
    // 1) Mitarbeiter listesi
    // =========================
    async function loadEmployees() {
        if (!ddlEmployee) return;
        const url = `${API_BASE_URL}/api/Company/${COMPANY_ID}/Employees`;
        console.log("[loehne-tab] GET employees", url);

        ddlEmployee.innerHTML = `<option value="">-- bitte wählen --</option>`;

        try {
            const res = await fetch(url);
            if (!res.ok) {
                console.error("[loehne-tab] employees http error", res.status);
                return;
            }
            const payload = await res.json();
            console.log("[loehne-tab] employees response", payload);

            if (!payload.success || !payload.data) return;

            payload.data.forEach(e => {
                const opt = document.createElement("option");
                const fullName = `${e.firstName || ""} ${e.lastName || ""}`.trim();
                opt.value = e.id;
                opt.textContent = fullName || `#${e.id}`;
                ddlEmployee.appendChild(opt);
            });
        } catch (err) {
            console.error("[loehne-tab] employees exception", err);
        }
    }

    // =========================
    // 2) Lohn liste (tablo)
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
        <td>${row.bruttoSalary.toFixed(2)}</td>
        <td>${row.netSalary.toFixed(2)}</td>
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
    // 3) Employee detayını çek
    // =========================
    async function loadEmployeeForPayroll(employeeId) {
        const url = `${API_BASE_URL}/api/Employee/${employeeId}`;
        console.log("[loehne-tab] GET employee for payroll", url);

        const res = await fetch(url);
        if (!res.ok) {
            console.error("[loehne-tab] employee http error", res.status);
            throw new Error("Mitarbeiter konnte nicht geladen werden.");
        }

        const payload = await res.json();
        console.log("[loehne-tab] employee response", payload);

        if (!payload.success || !payload.data) {
            throw new Error(payload.message || "Mitarbeiter konnte nicht geladen werden.");
        }

        return payload.data;
    }

    // =========================
    // 4) Lohn berechnen + speichern (Quick)
    // =========================
    async function doCalcAndSave() {
        if (!ddlEmployee || !inputCalcMonth) return;

        const empId = parseInt(ddlEmployee.value || "0", 10);
        const monthVal = inputCalcMonth.value; // "YYYY-MM"

        if (!empId || isNaN(empId)) {
            showError("Bitte einen Mitarbeiter auswählen.");
            return;
        }
        if (!monthVal || monthVal.length !== 7) {
            showError("Bitte einen gültigen Monat (YYYY-MM) wählen.");
            return;
        }

        const periodIso = monthVal + "-01T00:00:00";
        console.log("[loehne-tab] doCalcAndSave employeeId =", empId, "period =", monthVal);

        let emp;
        try {
            emp = await loadEmployeeForPayroll(empId);
        } catch (err) {
            console.error(err);
            showError(err.message || "Mitarbeiter konnte nicht geladen werden.");
            return;
        }

        const payload = {
            employeeId: empId,
            period: periodIso,

            // Bonus / Zulagen / Abzüge (şimdilik 0)
            bonus: 0,
            extraAllowance: 0,
            unpaidDeduction: 0,
            otherDeduction: 0,

            // Sozialversicherungen: Employee snapshot
            applyAhv: !!emp.applyAhv,
            applyAlv: !!emp.applyAlv,
            applyBvg: !!emp.applyBvg,
            applyNbu: !!emp.applyNbu,
            applyBu: !!emp.applyBu,
            applyFak: !!emp.applyFak,
            applyQst: !!emp.applyQst,

            // Steuer / Bewilligung
            permitType: emp.permitType || "B",
            canton: emp.canton || "ZH",
            churchMember: !!emp.churchMember,
            withholdingTaxCode: emp.withholdingTaxCode || null
        };

        const url = `${API_BASE_URL}/api/Lohn/calc-and-save`;
        console.log("[loehne-tab] POST calc-and-save", url, payload);

        try {
            const res = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            const json = await res.json().catch(() => null);
            console.log("[loehne-tab] calc-and-save response", res.status, json);

            if (!res.ok || !json || !json.success) {
                showError(json && json.message
                    ? json.message
                    : "Berechnung / Speichern fehlgeschlagen.");
                return;
            }

            showSuccess("Lohn wurde berechnet und als Entwurf gespeichert.");

            // Modal kapat
            const modal = document.getElementById("calcModal");
            if (window.$ && window.$("#calcModal").modal) {
                window.$("#calcModal").modal("hide");
            } else if (modal && window.bootstrap && window.bootstrap.Modal) {
                const m = window.bootstrap.Modal.getOrCreateInstance(modal);
                m.hide();
            }

            // Listeyi güncelle
            await loadMonthly();
        } catch (err) {
            console.error("[loehne-tab] calc-and-save exception", err);
            showError("Berechnung / Speichern fehlgeschlagen (Netzwerkfehler).");
        }
    }

    // =========================
    // 5) Event binding
    // =========================
    document.addEventListener("DOMContentLoaded", function () {
        console.log("[loehne-tab] DOMContentLoaded");

        const current = getCurrentPeriod();
        if (inputPeriod && !inputPeriod.value) inputPeriod.value = current;
        if (inputCalcMonth && !inputCalcMonth.value) inputCalcMonth.value = current;

        loadEmployees();
        loadMonthly();

        if (btnLoad) {
            btnLoad.addEventListener("click", function () {
                loadMonthly();
            });
        }

        if (btnOpenModal && inputCalcMonth) {
            btnOpenModal.addEventListener("click", function () {
                if (!inputCalcMonth.value) {
                    inputCalcMonth.value = getCurrentPeriod();
                }
            });
        }

        if (btnDoCalc) {
            btnDoCalc.addEventListener("click", function () {
                doCalcAndSave();
            });
        }
    });
})();
