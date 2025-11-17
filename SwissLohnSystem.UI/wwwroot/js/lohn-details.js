// wwwroot/js/lohn-details.js
(function () {
    console.log("[lohn-details] script loaded");

    const LOHN_ID = window.LOHN_ID || 0;
    const API_BASE = (window.API_BASE_URL || "").replace(/\/+$/, "");

    console.log("[lohn-details] LOHN_ID =", LOHN_ID);
    console.log("[lohn-details] API_BASE =", API_BASE);

    if (!LOHN_ID) {
        console.warn("[lohn-details] LOHN_ID fehlt, breche ab.");
        return;
    }

    function api(path) {
        return API_BASE + path;
    }

    // ---------- Helfer: Zahlen / Format ----------

    function normalizeNumber(value) {
        if (value === null || value === undefined || value === "") return null;
        const n = Number(value);
        if (!isFinite(n)) return null;
        return n;
    }

    function formatMoney(value, withCurrency) {
        const n = normalizeNumber(value);
        if (n === null) return "—";
        const txt = n.toFixed(2);
        return withCurrency ? `CHF ${txt}` : txt;
    }

    function formatHours(value) {
        const n = normalizeNumber(value);
        if (n === null) return "—";
        return n.toFixed(2) + " h";
    }

    function formatNumber(value, decimals = 2, showZero = true) {
        const n = normalizeNumber(value);
        if (n === null) return "–";
        if (!showZero && n === 0) return "–";
        return n.toFixed(decimals);
    }

    function formatCurrency(amount, typeHint) {
        const n = normalizeNumber(amount);
        if (n === null) return "–";

        const t = (typeHint || "").toString().toLowerCase();
        let sign = "";

        if (t.includes("deduct") || t.includes("abzug") || t.includes("employee")) {
            sign = "-";
        } else if (t.includes("employer") || t.includes("ag-kosten")) {
            sign = "+";
        } else if (t.includes("netto")) {
            sign = "";
        } else {
            sign = "+";
        }

        return sign + " CHF " + n.toFixed(2);
    }

    // 5-spaltige Zeile für Detailtabelle
    function appendRow(tbody, cols, isSectionRow) {
        if (!tbody) return;
        const tr = document.createElement("tr");
        if (isSectionRow) {
            tr.className = "font-weight-bold bg-light";
        }
        tr.innerHTML =
            "<td>" + (cols[0] || "") + "</td>" +
            "<td class='text-right'>" + (cols[1] || "") + "</td>" +
            "<td class='text-right'>" + (cols[2] || "") + "</td>" +
            "<td class='text-right'>" + (cols[3] || "") + "</td>" +
            "<td class='text-right'>" + (cols[4] || "") + "</td>";
        tbody.appendChild(tr);
    }

    function showToast(type, msg) {
        if (window.toastr) {
            if (type === "error") toastr.error(msg);
            else if (type === "success") toastr.success(msg);
            else toastr.info(msg);
        } else {
            alert(msg);
        }
    }

    let currentLohn = null; // für Finalisieren etc.

    // ---------- Haupt-Ladefunktion ----------

    async function loadLohn() {
        const url = api(`/api/Lohn/${LOHN_ID}`);
        console.log("[lohn-details] GET", url);

        try {
            const res = await fetch(url);
            if (!res.ok) {
                console.error("[lohn-details] HTTP error", res.status);
                showToast("error", `Lohn konnte nicht geladen werden (HTTP ${res.status}).`);
                return;
            }

            const payload = await res.json();
            console.log("[lohn-details] response", payload);

            if (!payload.success || !payload.data) {
                showToast("error", payload.message || "Lohnabrechnung wurde nicht gefunden.");
                return;
            }

            const l = payload.data;
            currentLohn = l;

            fillHeaderAndCards(l);
            fillItemsTable(l);
            await loadEmployeeAndCompany(l.employeeId);
            await loadCalcItems(l);   // 🔥 Detailtabelle aus aktueller Berechnung

        } catch (err) {
            console.error("[lohn-details] exception", err);
            showToast("error", "Fehler beim Laden der Lohnabrechnung.");
        }
    }

    // ---------- Header + Karten ----------

    function fillHeaderAndCards(l) {
        const dtPeriod = document.getElementById("period");
        const dtCreated = document.getElementById("createdAt");
        const badge = document.getElementById("badgeStatus");
        const btnFinalize = document.getElementById("btnFinalize");
        const btnBack = document.getElementById("btnBackToCompany");

        // Arbeitszeit-Elemente
        const elMonthlyHours = document.getElementById("ldmMonthlyHours");
        const elMonthlyOvertime = document.getElementById("ldmMonthlyOvertime");

        const periodText = `${String(l.month).padStart(2, "0")}.${l.year}`;
        if (dtPeriod) dtPeriod.textContent = periodText;

        if (dtCreated && l.createdAt) {
            const created = new Date(l.createdAt);
            if (!isNaN(created.getTime())) {
                dtCreated.textContent = created.toLocaleDateString("de-CH");
            } else {
                dtCreated.textContent = "—";
            }
        }

        // Status / Final
        if (badge) {
            if (l.isFinal) {
                badge.textContent = "Final";
                badge.className = "badge badge-success";
            } else {
                badge.textContent = "Entwurf";
                badge.className = "badge badge-warning";
            }
        }

        if (btnFinalize) {
            if (l.isFinal) {
                btnFinalize.classList.add("d-none");
            } else {
                btnFinalize.classList.remove("d-none");
                btnFinalize.onclick = onFinalizeClick;
            }
        }

        if (btnBack && !btnBack.getAttribute("data-fixed")) {
            btnBack.setAttribute("data-fixed", "1");
        }

        // Karten (Geld)
        const elBrutto = document.getElementById("bruttoSalary");
        const elDed = document.getElementById("totalDeductions");
        const elNet = document.getElementById("netSalary");
        const elOvertime = document.getElementById("overtimePay");
        const elHoliday = document.getElementById("holidayAllowance");
        const elChild = document.getElementById("childAllowance");

        if (elBrutto) elBrutto.textContent = formatMoney(l.bruttoSalary, true);
        if (elDed) elDed.textContent = formatMoney(l.totalDeductions, true);
        if (elNet) elNet.textContent = formatMoney(l.netSalary, true);

        if (elOvertime) elOvertime.textContent = formatMoney(l.overtimePay, true);
        if (elHoliday) elHoliday.textContent = formatMoney(l.holidayAllowance, true);
        if (elChild) elChild.textContent = formatMoney(l.childAllowance, true);

        // Arbeitszeit (aus gespeicherten Feldern)
        if (elMonthlyHours) {
            elMonthlyHours.textContent = formatHours(l.monthlyHours);
        }
        if (elMonthlyOvertime) {
            elMonthlyOvertime.textContent = formatHours(l.monthlyOvertimeHours);
        }
    }

    // ---------- Kurzfassungstabelle ----------

    function fillItemsTable(l) {
        const tbody = document.getElementById("itemsBody");
        if (!tbody) return;

        tbody.innerHTML = "";

        function addRow(label, amount) {
            const tr = document.createElement("tr");
            tr.innerHTML =
                `<td>${label}</td>` +
                `<td class="text-right">${formatMoney(amount, true)}</td>`;
            tbody.appendChild(tr);
        }

        addRow("Bruttolohn", l.bruttoSalary);

        if (normalizeNumber(l.overtimePay) > 0) {
            addRow("Überstunden-Vergütung", l.overtimePay);
        }
        if (normalizeNumber(l.holidayAllowance) > 0) {
            addRow("Ferienentschädigung", l.holidayAllowance);
        }
        if (normalizeNumber(l.childAllowance) > 0) {
            addRow("Kinderzulagen", l.childAllowance);
        }

        addRow("Abzüge (Total)", -Math.abs(normalizeNumber(l.totalDeductions) || 0));

        const sep = document.createElement("tr");
        sep.innerHTML = `<td colspan="2">&nbsp;</td>`;
        tbody.appendChild(sep);

        addRow("Netto auszuzahlen", l.netSalary);
    }

    // ---------- Detailtabelle (wie PDF) ----------

    async function loadCalcItems(l) {
        const tbody = document.getElementById("ldmSlipBody");
        if (!tbody) return;

        const url = api("/api/Lohn/calc");
        const periodDate = `${l.year}-${String(l.month).padStart(2, "0")}-01T00:00:00`;

        const payload = {
            employeeId: l.employeeId,
            period: periodDate
        };

        console.log("[lohn-details] POST calc", url, payload);

        try {
            const res = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                console.error("[lohn-details] calc HTTP error", res.status);
                tbody.innerHTML =
                    `<tr><td colspan="5" class="text-center text-danger p-3">
                        Berechnungsdetails konnten nicht geladen werden (HTTP ${res.status}).
                      </td></tr>`;
                return;
            }

            const json = await res.json();
            console.log("[lohn-details] calc response", json);

            if (!json.success || !json.data) {
                tbody.innerHTML =
                    `<tr><td colspan="5" class="text-center text-danger p-3">
                        ${json.message || "Berechnung fehlgeschlagen."}
                      </td></tr>`;
                return;
            }

            const d = json.data;
            const items = d.items || [];
            tbody.innerHTML = "";

            let totalEmployeeDed = 0;
            let totalEmployerCost = 0;

            items.forEach(x => {
                const side = (x.side || "").toString().toLowerCase();
                const amt = normalizeNumber(x.amount);
                if (amt === null) return;

                if (side === "employee" || side === "an") {
                    totalEmployeeDed += amt;
                } else if (side === "employer" || side === "ag") {
                    totalEmployerCost += amt;
                }
            });

            let netVal = d.netToPay ?? l.netSalary ?? null;
            let netNum = normalizeNumber(netVal);

            let grossVal = d.grossSalary ?? d.bruttoSalary ?? l.bruttoSalary ?? null;
            let grossNum = normalizeNumber(grossVal);

            if (grossNum === null || grossNum === 0) {
                if (netNum !== null && normalizeNumber(totalEmployeeDed) !== null) {
                    grossNum = netNum + totalEmployeeDed;
                } else {
                    grossNum = 0;
                }
            }
            grossVal = grossNum;

            if (netNum === null) {
                netNum = grossNum - totalEmployeeDed;
                netVal = netNum;
            }

            // 1) Einnahmen / Brutto-Positionen
            items.forEach(x => {
                const typeRaw = (x.type || "").toString().toLowerCase();
                const side = (x.side || "").toString().toLowerCase();

                const isEarning =
                    typeRaw.includes("earn") ||
                    typeRaw.includes("lohn") ||
                    (typeRaw === "" && (side === "" || side === "employee"));

                if (!isEarning) return;

                const legend = x.title || x.code || "";
                let percentStr = "–";
                let qtyStr = "–";
                const basisStr = formatNumber(x.basis);

                if (x.rate !== undefined && x.rate !== null && x.rate !== "") {
                    const r = normalizeNumber(x.rate);
                    if (r !== null) percentStr = formatNumber(r * 100, 3, true);
                }

                if (x.quantity !== undefined && x.quantity !== null && x.quantity !== "") {
                    qtyStr = formatNumber(x.quantity, 3, true);
                }

                const betragStr = formatCurrency(x.amount, "earning");

                appendRow(tbody, [
                    legend,
                    percentStr,
                    qtyStr,
                    basisStr,
                    betragStr
                ], false);
            });

            // 2) BRUTTOLohn
            appendRow(tbody, [
                "<strong>BRUTTOLohn</strong>",
                "",
                "",
                "",
                "<strong>" + formatCurrency(grossVal, "earning") + "</strong>"
            ], true);

            appendRow(tbody, ["", "", "", "", ""], false);

            // 3) Arbeitnehmerabzüge
            items.forEach(x => {
                const side = (x.side || "").toString().toLowerCase();
                const amt = normalizeNumber(x.amount);
                if (amt === null || amt === 0) return;

                const isEmployee = side === "employee" || side === "an";
                if (!isEmployee) return;

                const legend = x.title || x.code || "";
                let percentStr = "–";
                let qtyStr = "–";
                const basisStr = formatNumber(x.basis);

                if (x.rate !== undefined && x.rate !== null && x.rate !== "") {
                    const r = normalizeNumber(x.rate);
                    if (r !== null) percentStr = formatNumber(r * 100, 3, true);
                }

                if (x.quantity !== undefined && x.quantity !== null && x.quantity !== "") {
                    qtyStr = formatNumber(x.quantity, 3, true);
                }

                const betragStr = formatCurrency(x.amount, "deduction");

                appendRow(tbody, [
                    legend,
                    percentStr,
                    qtyStr,
                    basisStr,
                    betragStr
                ], false);
            });

            appendRow(tbody, [
                "<strong>TOTAL ABZUEGE</strong>",
                "",
                "",
                "",
                "<strong>" + formatCurrency(totalEmployeeDed, "deduction") + "</strong>"
            ], true);

            appendRow(tbody, ["", "", "", "", ""], false);

            // 4) Arbeitgeberkosten
            appendRow(tbody, [
                "<strong>AG-KOSTEN</strong>",
                "",
                "",
                "",
                ""
            ], true);

            items.forEach(x => {
                const side = (x.side || "").toString().toLowerCase();
                const amt = normalizeNumber(x.amount);
                if (amt === null || amt === 0) return;

                const isEmployer = side === "employer" || side === "ag";
                if (!isEmployer) return;

                const legend = x.title || x.code || "";
                let percentStr = "–";
                let qtyStr = "–";
                const basisStr = formatNumber(x.basis);

                if (x.rate !== undefined && x.rate !== null && x.rate !== "") {
                    const r = normalizeNumber(x.rate);
                    if (r !== null) percentStr = formatNumber(r * 100, 3, true);
                }

                if (x.quantity !== undefined && x.quantity !== null && x.quantity !== "") {
                    qtyStr = formatNumber(x.quantity, 3, true);
                }

                const betragStr = formatCurrency(x.amount, "employer");

                appendRow(tbody, [
                    legend,
                    percentStr,
                    qtyStr,
                    basisStr,
                    betragStr
                ], false);
            });

            appendRow(tbody, [
                "<strong>TOTAL AG-KOSTEN</strong>",
                "",
                "",
                "",
                "<strong>" + formatCurrency(totalEmployerCost, "employer") + "</strong>"
            ], true);

            // 5) NETTO
            appendRow(tbody, [
                "<strong>NETTOLOHN</strong>",
                "",
                "",
                "",
                "<strong>" + formatCurrency(netVal, "netto") + "</strong>"
            ], true);

        } catch (err) {
            console.error("[lohn-details] loadCalcItems exception", err);
            tbody.innerHTML =
                `<tr><td colspan="5" class="text-center text-danger p-3">
                    Fehler beim Laden der Berechnungsdetails.
                  </td></tr>`;
        }
    }

    // ---------- Mitarbeiter & Firma ----------

    async function loadEmployeeAndCompany(employeeId) {
        if (!employeeId) return;

        const elEmpName = document.getElementById("employeeName");
        const elCompanyName = document.getElementById("companyName");
        const bcCompany = document.getElementById("bc-company");
        const btnBack = document.getElementById("btnBackToCompany");

        try {
            const urlEmp = api(`/api/Employee/${employeeId}`);
            console.log("[lohn-details] GET employee", urlEmp);

            const resEmp = await fetch(urlEmp);
            if (!resEmp.ok) {
                console.warn("[lohn-details] employee HTTP error", resEmp.status);
                if (elEmpName) elEmpName.textContent = "—";
                return;
            }

            const payloadEmp = await resEmp.json();
            console.log("[lohn-details] employee response", payloadEmp);
            if (!payloadEmp.success || !payloadEmp.data) {
                if (elEmpName) elEmpName.textContent = "—";
                return;
            }

            const e = payloadEmp.data;
            const fullName = `${e.firstName || ""} ${e.lastName || ""}`.trim();
            const companyId = e.companyId;

            if (elEmpName) elEmpName.textContent = fullName || "—";

            if (companyId) {
                try {
                    const urlCo = api(`/api/Company/${companyId}`);
                    console.log("[lohn-details] GET company", urlCo);

                    const resCo = await fetch(urlCo);
                    if (resCo.ok) {
                        const payloadCo = await resCo.json();
                        console.log("[lohn-details] company response", payloadCo);

                        if (payloadCo.success && payloadCo.data) {
                            const c = payloadCo.data;
                            const cname = c.name || "Firma";

                            if (elCompanyName) elCompanyName.textContent = cname;

                            if (bcCompany) {
                                bcCompany.innerHTML =
                                    `<a href="/Companies/Details/${companyId}#loehne">${cname}</a>`;
                            }

                            if (btnBack) {
                                btnBack.href = `/Companies/Details/${companyId}#loehne`;
                            }
                        }
                    }
                } catch (errCo) {
                    console.warn("[lohn-details] company load error", errCo);
                }
            }

        } catch (errEmp) {
            console.error("[lohn-details] employee load exception", errEmp);
        }
    }

    // ---------- Finalisieren ----------

    async function onFinalizeClick() {
        if (!currentLohn) {
            showToast("error", "Lohn-Daten sind nicht geladen.");
            return;
        }

        if (!confirm("Diese Lohnabrechnung wirklich finalisieren?")) {
            return;
        }

        const payload = {
            employeeId: currentLohn.employeeId,
            month: currentLohn.month,
            year: currentLohn.year
        };

        const url = api("/api/Lohn/finalize");
        console.log("[lohn-details] POST finalize", url, payload);

        try {
            const res = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            const json = await res.json().catch(() => null);
            console.log("[lohn-details] finalize response", res.status, json);

            if (!res.ok || !json || !json.success) {
                showToast(
                    "error",
                    (json && json.message) || "Finalisierung fehlgeschlagen."
                );
                return;
            }

            showToast("success", json.message || "Lohnabrechnung wurde finalisiert.");

            currentLohn.isFinal = true;
            const badge = document.getElementById("badgeStatus");
            const btnFinalize = document.getElementById("btnFinalize");

            if (badge) {
                badge.textContent = "Final";
                badge.className = "badge badge-success";
            }
            if (btnFinalize) {
                btnFinalize.classList.add("d-none");
            }

        } catch (err) {
            console.error("[lohn-details] finalize exception", err);
            showToast("error", "Finalisierung fehlgeschlagen (Netzwerkfehler).");
        }
    }

    // ---------- init ----------

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", loadLohn);
    } else {
        loadLohn();
    }
})();
