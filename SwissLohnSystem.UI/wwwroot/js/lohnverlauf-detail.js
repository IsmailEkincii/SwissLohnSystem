console.log("[lohnverlauf] external script loaded");

// ---- Yardımcı format fonksiyonları ----

function normalizeNumber(value) {
    if (value === null || value === undefined || value === "") return null;
    var n = Number(value);
    if (!isFinite(n)) return null;
    return n;
}

function formatNumber(value, decimals, showZero) {
    if (decimals === undefined) decimals = 2;
    if (showZero === undefined) showZero = true;

    var n = normalizeNumber(value);
    if (n === null) return "–";
    if (!showZero && n === 0) return "–";
    return n.toFixed(decimals);
}

function formatCurrency(amount, typeHint) {
    var n = normalizeNumber(amount);
    if (n === null) return "–";

    var t = (typeHint || "").toString().toLowerCase();
    var sign = "";

    if (t.indexOf("deduct") >= 0 || t.indexOf("abzug") >= 0 || t.indexOf("employee") >= 0) {
        sign = "-";
    } else if (t.indexOf("employer") >= 0 || t.indexOf("ag-kosten") >= 0) {
        sign = "+";
    } else if (t.indexOf("netto") >= 0) {
        sign = "";
    } else {
        sign = "+";
    }

    return sign + " CHF " + n.toFixed(2);
}

// 5 kolonlu satır ekle: Legende | % | Menge | Ansatz | Betrag
function appendRow(tbody, cols, isSectionRow) {
    if (!tbody) return;
    var tr = document.createElement("tr");
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

// ---- Ay ismi (Januar 2025 gibi) ----
var monthNamesDe = [
    "", "Januar", "Februar", "März", "April", "Mai", "Juni",
    "Juli", "August", "September", "Oktober", "November", "Dezember"
];

document.addEventListener("DOMContentLoaded", function () {

    var EMPLOYEE_ID = window.EMPLOYEE_ID || 0;
    var API_BASE_URL = (window.API_BASE_URL || "").replace(/\/+$/, "");

    console.log("[lohnverlauf] DOMContentLoaded");
    console.log("[lohnverlauf] EMPLOYEE_ID =", EMPLOYEE_ID);
    console.log("[lohnverlauf] API_BASE_URL =", API_BASE_URL);

    var rows = document.querySelectorAll(".lohn-row");
    console.log("[lohnverlauf] row count =", rows.length);

    if (!rows || rows.length === 0) {
        console.log("[lohnverlauf] no rows found");
        return;
    }

    // 🔥 WorkDays'ten belirli bir ay için toplam saat hesapla
    async function loadMonthHoursFromWorkDays(year, month) {
        if (!EMPLOYEE_ID || !API_BASE_URL) return { total: null, overtime: null };

        var url = API_BASE_URL + "/api/WorkDay/Employee/" + EMPLOYEE_ID;
        console.log("[lohnverlauf] loadMonthHoursFromWorkDays GET", url);

        try {
            var res = await fetch(url);
            if (!res.ok) {
                console.warn("[lohnverlauf] WorkDay HTTP error", res.status);
                return { total: null, overtime: null };
            }

            var json = await res.json();
            console.log("[lohnverlauf] WorkDay response", json);

            if (!json.success || !json.data) return { total: null, overtime: null };

            var list = json.data;
            var ym = year + "-" + String(month).padStart(2, "0");

            var total = 0;
            var ot = 0;

            list.forEach(function (w) {
                if (!w.date) return;
                var dateStr = w.date.split("T")[0]; // "2025-11-14"
                if (dateStr.indexOf(ym) !== 0) return;

                var h = normalizeNumber(w.hoursWorked);
                var oh = normalizeNumber(w.overtimeHours);
                if (h !== null) total += h;
                if (oh !== null) ot += oh;
            });

            return { total: total, overtime: ot };
        } catch (e) {
            console.error("[lohnverlauf] WorkDay load exception", e);
            return { total: null, overtime: null };
        }
    }

    async function openLohnDetail(row) {
        console.log("[lohnverlauf] openLohnDetail for row");

        var year = row.getAttribute("data-year");
        var month = row.getAttribute("data-month");

        if (!year || !month) {
            console.warn("[lohnverlauf] row missing year/month");
            alert("Fehlende Perioden-Information (Jahr/Monat).");
            return;
        }

        var monthNum = Number(month);
        month = month.toString().padStart(2, "0");
        var period = year + "-" + month;
        var periodDate = period + "-01T00:00:00";

        var grossFromRow = row.getAttribute("data-brutto");
        var netFromRow = row.getAttribute("data-net");
        var totalDedFromRow = row.getAttribute("data-totalded");

        // 🔥 Aylık saatler (row attribute'larından)
        var mhoursFromRow = row.getAttribute("data-mhours");
        var motHoursFromRow = row.getAttribute("data-mot-hours");
        console.log("[lohnverlauf] mhoursFromRow =", mhoursFromRow, "motHoursFromRow =", motHoursFromRow);

        var spanMonthlyHours = document.getElementById("ldmMonthlyHours");
        var spanMonthlyOvertime = document.getElementById("ldmMonthlyOvertime");

        var mhoursNum = normalizeNumber(mhoursFromRow);
        var motNum = normalizeNumber(motHoursFromRow);

        if (mhoursNum !== null || motNum !== null) {
            // Lohn tablosunda saatler varsa direkt kullan
            if (spanMonthlyHours) {
                spanMonthlyHours.textContent = formatNumber(mhoursNum || 0, 2, true) + " h";
            }
            if (spanMonthlyOvertime) {
                spanMonthlyOvertime.textContent = formatNumber(motNum || 0, 2, true) + " h";
            }
        } else {
            // 🔥 Fallback: WorkDays'ten hesapla
            var hoursResult = await loadMonthHoursFromWorkDays(parseInt(year, 10), parseInt(month, 10));
            console.log("[lohnverlauf] hoursResult =", hoursResult);

            if (spanMonthlyHours) {
                spanMonthlyHours.textContent =
                    hoursResult.total !== null ? formatNumber(hoursResult.total, 2, true) + " h" : "–";
            }
            if (spanMonthlyOvertime) {
                spanMonthlyOvertime.textContent =
                    hoursResult.overtime !== null ? formatNumber(hoursResult.overtime, 2, true) + " h" : "–";
            }
        }

        // Modal başlığındaki küçük yazı
        var spanPeriodTitle = document.getElementById("ldmPeriod");
        if (spanPeriodTitle) spanPeriodTitle.textContent = period;

        // "Lohnabrechnung November 2025" gibi
        var spanPeriodHeader = document.getElementById("ldmPeriodHeader");
        if (spanPeriodHeader) {
            var monthName = monthNamesDe[monthNum] || period;
            spanPeriodHeader.textContent = monthName + " " + year;
        }

        var url = API_BASE_URL + "/api/Lohn/calc";

        if (!EMPLOYEE_ID) {
            alert("EMPLOYEE_ID fehlt – Mitarbeiter konnte nicht ermittelt werden.");
            return;
        }

        var payload = {
            employeeId: Number(EMPLOYEE_ID),
            period: periodDate
        };

        console.log("[lohnverlauf] POST", url, payload);

        try {
            var res = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                console.error("[lohnverlauf] HTTP error", res.status);
                alert("Fehler bei der Berechnung (" + res.status + ").");
                return;
            }

            var json = await res.json();
            console.log("[lohnverlauf] calc response", json);

            if (!json.success) {
                alert(json.message || "Berechnung fehlgeschlagen.");
                return;
            }

            var d = json.data;
            if (!d) {
                alert("Keine Daten vom Server empfangen.");
                return;
            }

            var tbody = document.getElementById("ldmSlipBody");
            if (!tbody) return;
            tbody.innerHTML = "";

            var items = d.items || [];

            // ---- 1) AN ve AG toplamları ----
            var totalEmployeeDed = 0;
            var totalEmployerCost = 0;

            items.forEach(function (x) {
                var side = (x.side || "").toString().toLowerCase();
                var amt = normalizeNumber(x.amount);
                if (amt === null) return;

                if (side === "employee" || side === "an") {
                    totalEmployeeDed += amt;
                } else if (side === "employer" || side === "ag") {
                    totalEmployerCost += amt;
                }
            });

            // ---- 2) NETTO: API -> satır ----
            var netVal = (d.netToPay !== undefined && d.netToPay !== null) ? d.netToPay : netFromRow;
            var netNum = normalizeNumber(netVal);

            // ---- 3) BRUTTO: API -> satır -> (net + AN) ----
            var grossVal = (d.grossSalary !== undefined && d.grossSalary !== null)
                ? d.grossSalary
                : (d.bruttoSalary !== undefined && d.bruttoSalary !== null
                    ? d.bruttoSalary
                    : grossFromRow);
            var grossNum = normalizeNumber(grossVal);

            if (grossNum === null || grossNum === 0) {
                if (netNum !== null && normalizeNumber(totalEmployeeDed) !== null) {
                    grossNum = netNum + totalEmployeeDed;
                } else {
                    grossNum = 0;
                }
            }
            grossVal = grossNum;

            // Eğer net hâlâ yoksa brutto - AN ile hesapla
            if (netNum === null) {
                netNum = grossNum - totalEmployeeDed;
                netVal = netNum;
            }

            // ---- 4) Üst özetler (printable + kartlar) ----
            var sumGross = document.getElementById("sumGross");
            var sumEmp = document.getElementById("sumEmp");
            var sumEr = document.getElementById("sumEr");
            var sumNet = document.getElementById("sumNet");

            if (sumGross) sumGross.textContent = formatNumber(grossVal);
            if (sumEmp) sumEmp.textContent = formatNumber(totalEmployeeDed);
            if (sumEr) sumEr.textContent = formatNumber(totalEmployerCost);
            if (sumNet) sumNet.textContent = formatNumber(netVal);

            // Modal başındaki küçük kartlar
            var cardGross = document.getElementById("ldmGross");
            var cardEmp = document.getElementById("ldmEmpTotal");
            var cardEr = document.getElementById("ldmErTotal");
            var cardNet = document.getElementById("ldmNet");

            if (cardGross) cardGross.textContent = formatNumber(grossVal);
            if (cardEmp) cardEmp.textContent = formatNumber(totalEmployeeDed);
            if (cardEr) cardEr.textContent = formatNumber(totalEmployerCost);
            if (cardNet) cardNet.textContent = formatNumber(netVal);

            // ---- 5) GELİR KALEMLERİ ----
            items.forEach(function (x) {
                var typeRaw = (x.type || "").toString().toLowerCase();
                var side = (x.side || "").toString().toLowerCase();

                var isEarning =
                    typeRaw.indexOf("earn") >= 0 ||
                    typeRaw.indexOf("lohn") >= 0 ||
                    (typeRaw === "" && (side === "" || side === "employee"));

                if (!isEarning) return;

                var legend = x.title || x.code || "";
                var percentStr = "–";
                var qtyStr = "–";
                var basisStr = formatNumber(x.basis);

                if (x.rate !== undefined && x.rate !== null && x.rate !== "") {
                    var r = normalizeNumber(x.rate);
                    if (r !== null) percentStr = formatNumber(r * 100, 3, true);
                }

                if (x.quantity !== undefined && x.quantity !== null && x.quantity !== "") {
                    qtyStr = formatNumber(x.quantity, 3, true);
                }

                var betragStr = formatCurrency(x.amount, "earning");

                appendRow(tbody, [
                    legend,
                    percentStr,
                    qtyStr,
                    basisStr,
                    betragStr
                ], false);
            });

            // ---- 6) BRUTTOLOHN ----
            appendRow(tbody, [
                "<strong>BRUTTOLOHN</strong>",
                "",
                "",
                "",
                "<strong>" + formatCurrency(grossVal, "earning") + "</strong>"
            ], true);

            appendRow(tbody, ["", "", "", "", ""], false);

            // ---- 7) AN-KESİNTİLER ----
            items.forEach(function (x) {
                var side = (x.side || "").toString().toLowerCase();
                var amt = normalizeNumber(x.amount);
                if (amt === null || amt === 0) return;

                var isEmployee = side === "employee" || side === "an";
                if (!isEmployee) return;

                var legend = x.title || x.code || "";
                var percentStr = "–";
                var qtyStr = "–";
                var basisStr = formatNumber(x.basis);

                if (x.rate !== undefined && x.rate !== null && x.rate !== "") {
                    var r = normalizeNumber(x.rate);
                    if (r !== null) percentStr = formatNumber(r * 100, 3, true);
                }

                if (x.quantity !== undefined && x.quantity !== null && x.quantity !== "") {
                    qtyStr = formatNumber(x.quantity, 3, true);
                }

                var betragStr = formatCurrency(x.amount, "deduction");

                appendRow(tbody, [
                    legend,
                    percentStr,
                    qtyStr,
                    basisStr,
                    betragStr
                ], false);
            });

            // ---- 8) TOTAL ABZUEGE (sadece AN) ----
            appendRow(tbody, [
                "<strong>TOTAL ABZUEGE</strong>",
                "",
                "",
                "",
                "<strong>" + formatCurrency(totalEmployeeDed, "deduction") + "</strong>"
            ], true);

            appendRow(tbody, ["", "", "", "", ""], false);

            // ---- 9) AG-KOSTEN BLOĞU ----
            appendRow(tbody, [
                "<strong>AG-KOSTEN</strong>",
                "",
                "",
                "",
                ""
            ], true);

            items.forEach(function (x) {
                var side = (x.side || "").toString().toLowerCase();
                var amt = normalizeNumber(x.amount);
                if (amt === null || amt === 0) return;

                var isEmployer = side === "employer" || side === "ag";
                if (!isEmployer) return;

                var legend = x.title || x.code || "";
                var percentStr = "–";
                var qtyStr = "–";
                var basisStr = formatNumber(x.basis);

                if (x.rate !== undefined && x.rate !== null && x.rate !== "") {
                    var r = normalizeNumber(x.rate);
                    if (r !== null) percentStr = formatNumber(r * 100, 3, true);
                }

                if (x.quantity !== undefined && x.quantity !== null && x.quantity !== "") {
                    qtyStr = formatNumber(x.quantity, 3, true);
                }

                var betragStr = formatCurrency(x.amount, "employer");

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

            // ---- 10) NETTOLOHN ----
            appendRow(tbody, [
                "<strong>NETTOLOHN</strong>",
                "",
                "",
                "",
                "<strong>" + formatCurrency(netVal, "netto") + "</strong>"
            ], true);

            // ---- Modal aç ----
            if (window.$ && window.$("#lohnDetailModal").modal) {
                window.$("#lohnDetailModal").modal("show");
            } else {
                var modalEl = document.getElementById("lohnDetailModal");
                if (modalEl && window.bootstrap && window.bootstrap.Modal) {
                    var m = window.bootstrap.Modal.getOrCreateInstance(modalEl);
                    m.show();
                } else {
                    alert("Netto: " + formatCurrency(netVal, "netto"));
                }
            }

        } catch (e) {
            console.error("[lohnverlauf] fetch error", e);
            alert("Fehler bei der Berechnung: " + e.message);
        }
    }

    // Satır ve buton event'leri
    rows.forEach(function (row) {

        row.addEventListener("click", function (e) {
            if (e.target.closest(".btn-lohn-detail")) return;
            console.log("[lohnverlauf] row click");
            openLohnDetail(row);
        });

        var btn = row.querySelector(".btn-lohn-detail");
        if (btn) {
            btn.addEventListener("click", function (e) {
                e.stopPropagation();
                console.log("[lohnverlauf] button click");
                openLohnDetail(row);
            });
        }
    });

    // PDF / Print butonu
    var btnPrint = document.getElementById("btnLdmPrint");
    if (btnPrint) {
        btnPrint.addEventListener("click", function () {
            console.log("[lohnverlauf] print button click");
            var printable = document.getElementById("lohnDetailPrintable");
            if (!printable) {
                window.print();
                return;
            }

            var win = window.open("", "_blank");
            win.document.write("<html><head><title>Lohnabrechnung</title>");
            win.document.write(
                "<style>" +
                "body{font-family:Arial,sans-serif;font-size:13px;line-height:1.5;margin:18mm;}" +
                "h5{margin:0 0 6mm 0;font-size:15px;font-weight:bold;}" +
                "table{width:100%;border-collapse:collapse;margin-top:4mm;}" +
                "th,td{border:1px solid #d0d0d0;padding:5px 7px;font-size:12px;}" +
                "th{background:#f0f4fb;font-weight:bold;}" +
                ".bg-light{background:#f7f7f7 !important;}" +
                ".text-right{text-align:right;}" +
                "</style>"
            );
            win.document.write("</head><body>");
            win.document.write(printable.innerHTML);
            win.document.write("</body></html>");
            win.document.close();
            win.focus();
            win.print();
        });
    }
});
