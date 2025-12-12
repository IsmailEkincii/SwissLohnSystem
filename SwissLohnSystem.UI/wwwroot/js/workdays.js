// wwwroot/js/workdays.js
console.log("[workdays] script file loaded");

document.addEventListener("DOMContentLoaded", function () {
    console.log("[workdays] DOMContentLoaded");

    const API_BASE = window.API_BASE_URL || "";
    const EMPLOYEE_ID = window.EMPLOYEE_ID || 0;

    console.log("[workdays] API_BASE =", API_BASE);
    console.log("[workdays] EMPLOYEE_ID =", EMPLOYEE_ID);

    if (!API_BASE || !EMPLOYEE_ID) {
        console.warn("[workdays] Missing API_BASE or EMPLOYEE_ID, aborting init.");
        return;
    }

    const tableBody = document.getElementById("wdTableBody");
    const btnNew = document.getElementById("btnWdNew");
    const modalEl = document.getElementById("workdayModal");
    const form = document.getElementById("workdayForm");

    if (!tableBody || !btnNew || !modalEl || !form) {
        console.warn("[workdays] Required DOM elements not found.", {
            tableBody,
            btnNew,
            modalEl,
            form
        });
        return;
    }

    const dateInput = document.getElementById("wdDate");
    const hoursInput = document.getElementById("wdHours");
    const otInput = document.getElementById("wdOvertime");
    const idInput = document.getElementById("wdId");
    const modalTitle = document.getElementById("wdModalTitle");
    const dayTypeSelect = document.getElementById("wdDayType");

    let bootstrapModal = null;
    if (window.bootstrap && bootstrap.Modal) {
        bootstrapModal = new bootstrap.Modal(modalEl);
    }

    function parseDecimal(value) {
        if (value == null) return 0;
        const v = value.toString().replace(',', '.');
        const n = parseFloat(v);
        return isNaN(n) ? 0 : n;
    }

    function toIsoDate(dateStr) {
        // input type="date" → YYYY-MM-DD
        if (!dateStr) return null;
        return dateStr;
    }

    // ✅ UI (0/1/2/3) -> API ("Work"/"Sick"/"Vacation"/"Unpaid")
    function toDayTypeString(v) {
        const n = parseInt(v, 10);
        switch (n) {
            case 0: return "Work";
            case 1: return "Sick";
            case 2: return "Vacation";
            case 3: return "Unpaid";
            default: return "Work";
        }
    }

    // ✅ API ("Work" vs) or number -> UI select value ("0/1/2/3")
    function toDayTypeSelectValue(v) {
        if (v === null || v === undefined) return "0";

        // If API returns number
        if (typeof v === "number") return String(v);

        const s = String(v).trim().toLowerCase();

        // If API returns string types
        switch (s) {
            case "work":
            case "arbeit":
            case "arbeitstag":
                return "0";
            case "sick":
            case "krank":
            case "krankheit":
                return "1";
            case "vacation":
            case "ferien":
            case "holiday":
                return "2";
            case "unpaid":
            case "unbezahlt":
                return "3";
            default:
                // If API accidentally stored "0"/"1" as string, still handle it
                if (s === "0" || s === "1" || s === "2" || s === "3") return s;
                return "0";
        }
    }

    function dayTypeLabelFromApi(v) {
        const sel = toDayTypeSelectValue(v);
        switch (sel) {
            case "0": return "Arbeitstag";
            case "1": return "Krankheit";
            case "2": return "Ferien";
            case "3": return "Unbezahlt";
            default: return "";
        }
    }

    async function loadWorkDays() {
        const url = `${API_BASE}/api/WorkDay/Employee/${EMPLOYEE_ID}`;
        console.log("[workdays] GET", url);

        tableBody.innerHTML = `<tr><td colspan="4" class="text-center text-muted p-3">Lade...</td></tr>`;

        try {
            const res = await fetch(url);
            if (!res.ok) {
                console.error("[workdays] load error", res.status);
                tableBody.innerHTML = `<tr><td colspan="4" class="text-center text-danger p-3">Fehler beim Laden der Arbeitszeiten.</td></tr>`;
                return;
            }

            const payload = await res.json();
            console.log("[workdays] load response", payload);

            if (!payload.success || !payload.data) {
                tableBody.innerHTML = `<tr><td colspan="4" class="text-center text-muted p-3">Keine Daten.</td></tr>`;
                return;
            }

            const list = payload.data;
            if (!list.length) {
                tableBody.innerHTML = `<tr><td colspan="4" class="text-center text-muted p-3">Keine Arbeitszeiten erfasst.</td></tr>`;
                return;
            }

            tableBody.innerHTML = "";
            for (const w of list) {
                const tr = document.createElement("tr");
                const dateOnly = w.date ? w.date.split("T")[0] : "";

                const dayTypeLabel = dayTypeLabelFromApi(w.dayType);

                tr.innerHTML = `
                    <td>${dateOnly}</td>
                    <td>${Number(w.hoursWorked || 0).toFixed(2)}</td>
                    <td>${Number(w.overtimeHours || 0).toFixed(2)}</td>
                    <td class="text-right">
                        <span class="badge badge-light mr-1">${dayTypeLabel}</span>
                        <button class="btn btn-xs btn-outline-secondary btn-wd-edit" data-id="${w.id}">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-xs btn-outline-danger btn-wd-delete" data-id="${w.id}">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                `;
                tableBody.appendChild(tr);
            }
        } catch (err) {
            console.error("[workdays] load exception", err);
            tableBody.innerHTML = `<tr><td colspan="4" class="text-center text-danger p-3">Fehler beim Laden der Arbeitszeiten.</td></tr>`;
        }
    }

    function openNewModal() {
        console.log("[workdays] openNewModal");
        idInput.value = "";

        // Bugün
        if (dateInput) {
            const today = new Date();
            const yyyy = today.getFullYear();
            const mm = String(today.getMonth() + 1).padStart(2, "0");
            const dd = String(today.getDate()).padStart(2, "0");
            dateInput.value = `${yyyy}-${mm}-${dd}`;
        }

        hoursInput.value = "8";
        otInput.value = "0";

        // Varsayılan: Arbeitstag
        if (dayTypeSelect) {
            dayTypeSelect.value = "0";
        }

        modalTitle.textContent = "Neue Arbeitszeit";

        if (bootstrapModal) bootstrapModal.show();
        else if (window.$) $(modalEl).modal("show");
    }

    function openEditModal(workDay) {
        console.log("[workdays] openEditModal", workDay);
        idInput.value = workDay.id;
        dateInput.value = workDay.date ? workDay.date.split("T")[0] : "";
        hoursInput.value = String(workDay.hoursWorked ?? 0).replace('.', ',');
        otInput.value = String(workDay.overtimeHours ?? 0).replace('.', ',');

        // ✅ API string/number ne dönerse dönsün select’e uyarlıyoruz
        if (dayTypeSelect) {
            dayTypeSelect.value = toDayTypeSelectValue(workDay.dayType);
        }

        modalTitle.textContent = "Arbeitszeit bearbeiten";

        if (bootstrapModal) bootstrapModal.show();
        else if (window.$) $(modalEl).modal("show");
    }

    async function fetchWorkDay(id) {
        const url = `${API_BASE}/api/WorkDay/${id}`;
        console.log("[workdays] GET by id", url);
        const res = await fetch(url);
        if (!res.ok) {
            console.error("[workdays] GET by id error", res.status);
            return null;
        }
        const payload = await res.json();
        if (!payload.success) return null;
        return payload.data;
    }

    async function saveWorkDay() {
        const id = parseInt(idInput.value || "0", 10);
        const dateVal = toIsoDate(dateInput.value);
        const hours = parseDecimal(hoursInput.value);
        const overtime = parseDecimal(otInput.value);

        if (!dateVal) {
            alert("Bitte Datum auswählen.");
            return;
        }

        // UI select -> number -> API string
        let dayTypeNum = 0;
        if (dayTypeSelect && dayTypeSelect.value !== "") {
            dayTypeNum = parseInt(dayTypeSelect.value, 10);
        }

        const body = {
            employeeId: EMPLOYEE_ID,
            date: dateVal,
            hoursWorked: hours,
            overtimeHours: overtime,
            // ✅ FIX: API string bekliyor
            dayType: toDayTypeString(dayTypeNum)
        };

        let url, method;
        if (id > 0) {
            url = `${API_BASE}/api/WorkDay/${id}`;
            method = "PUT";
            body.id = id;
        } else {
            url = `${API_BASE}/api/WorkDay`;
            method = "POST";
        }

        console.log("[workdays] SAVE", method, url, body);
        console.log("[workdays] SAVE JSON", JSON.stringify(body));


        try {
            const res = await fetch(url, {
                method,
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(body)
            });

            const payload = await res.json().catch(() => null);
            console.log("[workdays] save response", res.status, payload);

            if (!res.ok || !payload || !payload.success) {
                alert(payload && payload.message
                    ? `Fehler: ${payload.message}`
                    : "Speichern fehlgeschlagen.");
                return;
            }

            if (bootstrapModal) bootstrapModal.hide();
            else if (window.$) $(modalEl).modal("hide");

            await loadWorkDays();
        } catch (err) {
            console.error("[workdays] save exception", err);
            alert("Speichern fehlgeschlagen (Netzwerkfehler).");
        }
    }

    async function deleteWorkDay(id) {
        if (!confirm("Eintrag wirklich löschen?")) return;

        const url = `${API_BASE}/api/WorkDay/${id}`;
        console.log("[workdays] DELETE", url);
        try {
            const res = await fetch(url, { method: "DELETE" });
            const payload = await res.json().catch(() => null);
            console.log("[workdays] delete response", res.status, payload);

            if (!res.ok || !payload || !payload.success) {
                alert(payload && payload.message
                    ? `Fehler: ${payload.message}`
                    : "Löschen fehlgeschlagen.");
                return;
            }

            await loadWorkDays();
        } catch (err) {
            console.error("[workdays] delete exception", err);
            alert("Löschen fehlgeschlagen (Netzwerkfehler).");
        }
    }

    // Events
    btnNew.addEventListener("click", function () {
        console.log("[workdays] btnWdNew clicked");
        openNewModal();
    });

    tableBody.addEventListener("click", async (e) => {
        const editBtn = e.target.closest(".btn-wd-edit");
        const delBtn = e.target.closest(".btn-wd-delete");

        if (editBtn) {
            const id = parseInt(editBtn.dataset.id, 10);
            const wd = await fetchWorkDay(id);
            if (wd) openEditModal(wd);
        }

        if (delBtn) {
            const id = parseInt(delBtn.dataset.id, 10);
            await deleteWorkDay(id);
        }
    });

    form.addEventListener("submit", (e) => {
        e.preventDefault();
        saveWorkDay();
    });

    // İlk yükleme
    loadWorkDays();
});
