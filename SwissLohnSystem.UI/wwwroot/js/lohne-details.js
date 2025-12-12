// wwwroot/js/lohne-details.js
(function () {
    console.log("[lohne-details] script loaded");

    // ---- Küçük helper'lar ----
    function norm(v) {
        if (v === null || v === undefined || v === "") return null;
        var n = Number(v);
        return isFinite(n) ? n : null;
    }

    function fmtNum(v, decimals, showZero) {
        if (decimals === undefined) decimals = 2;
        if (showZero === undefined) showZero = true;
        var n = norm(v);
        if (n === null) return "–";
        if (!showZero && n === 0) return "–";
        return n.toFixed(decimals);
    }

    function fmtChf(v, typeHint) {
        var n = norm(v);
        if (n === null) return "–";
        var t = (typeHint || "").toString().toLowerCase();
        var sign = "";
        if (t.indexOf("deduct") >= 0 || t.indexOf("abzug") >= 0 || t.indexOf("employee") >= 0) {
            sign = "- ";
        } else if (t.indexOf("employer") >= 0 || t.indexOf("ag") >= 0) {
            sign = "+ ";
        } else {
            sign = "";
        }
        return sign + "CHF " + n.toFixed(2);
    }

    function appendRow(tbody, cols, isSection) {
        if (!tbody) return;
        var tr = document.createElement("tr");
        if (isSection) tr.className = "font-weight-bold bg-light";
        tr.innerHTML =
            "<td>" + (cols[0] || "") + "</td>" +
            "<td class='text-right'>" + (cols[1] || "") + "</td>" +
            "<td class='text-right'>" + (cols[2] || "") + "</td>" +
            "<td class='text-right'>" + (cols[3] || "") + "</td>" +
            "<td class='text-right'>" + (cols[4] || "") + "</td>";
        tbody.appendChild(tr);
    }

    document.addEventListener("DOMContentLoaded", function () {

        var API = (window.API_BASE_URL || "").replace(/\/+$/, "");
        var EMP = window.LOHN_EMPLOYEE_ID || 0;
        var Y = window.LOHN_YEAR || 0;
        var M = window.LOHN_MONTH || 0;

        console.log("[lohne-details] API_BASE =", API);
        console.log("[lohne-details] EMPLOYEE_ID =", EMP, "MONTH =", M, "YEAR =", Y);

        var btnFinalize = document.getElementById("btnFinalize");
        var badgeStatus = document.getElementById("badgeStatus");
        var btnPrint = document.getElementById("btnLdmPrint");

        // kartlar – hem yeni id'leri hem eski id'leri destekleyelim
        var cardGross = document.getElementById("cardGross") || document.getElementById("bruttoSalary");
        var cardEmp = document.getElementById("cardEmpTotal") || document.getElementById("totalDeductions");
        var cardEr = document.getElementById("cardErTotal");
        var cardNet = document.getElementById("cardNet") || document.getElementById("netSalary");

        var workedLabel = document.getElementById("labelWorkedDays");

        // ---------- PRINT ----------
        if (btnPrint) {
            btnPrint.addEventListener("click", function () {
                console.log("[lohne-details] print click");
                var printable = document.getElementById("lohnDetailPrintable");
                if (!printable) { window.print(); return; }

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

        // ---------- Finalisieren ----------
        if (btnFinalize) {
            btnFinalize.addEventListener("click", function () {
                if (!API || !EMP || !Y || !M) {
                    alert("Fehlende Daten für die Finalisierung.");
                    return;
                }
                if (!confirm("Diese Lohnabrechnung wirklich finalisieren?")) return;

                var url = API + "/api/Lohn/finalize";
                var payload = { employeeId: EMP, month: M, year: Y };
                console.log("[lohne-details] finalize POST", url, payload);

                fetch(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                })
                    .then(res => res.json().then(j => ({ ok: res.ok, json: j })))
                    .then(r => {
                        console.log("[lohne-details] finalize response", r);
                        if (!r.ok || !r.json || !r.json.success) {
                            alert((r.json && r.json.message) || "Finalisierung fehlgeschlagen.");
                            return;
                        }
                        alert(r.json.message || "Lohnabrechnung wurde finalisiert.");
                        btnFinalize.classList.add("d-none");
                        if (badgeStatus) {
                            badgeStatus.textContent = "Final";
                            badgeStatus.className = "badge badge-success";
                        }
                    })
                    .catch(err => {
                        console.error("[lohne-details] finalize error", err);
                        alert("Finalisierung fehlgeschlagen (Netzwerkfehler).");
                    });
            });
        }

        // ---------- Gearbeitete Tage (WorkDays) ----------
        function loadWorkedDays() {
            if (!API || !EMP || !workedLabel) return;

            workedLabel.textContent = "wird geladen…";

            var url = API + "/api/WorkDay/Employee/" + EMP;
            console.log("[lohne-details] GET WorkDays", url);

            fetch(url)
                .then(res => res.json().then(j => ({ ok: res.ok, json: j })))
                .then(r => {
                    console.log("[lohne-details] WorkDays response", r);
                    if (!r.ok || !r.json || !r.json.success || !r.json.data) {
                        workedLabel.textContent = "nicht erfasst";
                        return;
                    }

                    var ym = Y + "-" + String(M).padStart(2, "0");
                    var workedDates = {};
                    var sick = 0;
                    var unpaid = 0;

                    r.json.data.forEach(w => {
                        if (!w.date) return;
                        var dateStr = w.date.split("T")[0];
                        if (dateStr.indexOf(ym) !== 0) return;

                        var dt = (w.dayType || "").toString().toLowerCase();
                        if (dt === "workday" || dt === "arbeitstag" || dt === "0") {
                            workedDates[dateStr] = true;
                        } else if (dt === "sick" || dt === "krankheit" || dt === "1") {
                            sick++;
                        } else if (dt === "unpaid" || dt === "unbezahlt" || dt === "3") {
                            unpaid++;
                        }
                    });

                    var workedCount = Object.keys(workedDates).length;
                    var parts = [];
                    if (workedCount) parts.push(workedCount + " Arbeitstage");
                    if (sick) parts.push(sick + " Krankheitstage");
                    if (unpaid) parts.push(unpaid + " unbezahlte Tage");

                    workedLabel.textContent = parts.length ? parts.join(" / ") : "nicht erfasst";
                })
                .catch(err => {
                    console.error("[lohne-details] WorkDays error", err);
                    workedLabel.textContent = "–";
                });
        }

        // ---------- Slip + kartlar (AHV/ALV/BVG/QST vs.) ----------
        function loadSlip() {
            var tbody = document.getElementById("ldmSlipBody");
            if (!tbody) return;

            if (!API || !EMP || !Y || !M) {
                tbody.innerHTML = "<tr><td colspan='5' class='text-center text-muted'>Keine Daten.</td></tr>";
                return;
            }

            tbody.innerHTML =
                "<tr><td colspan='5' class='text-center text-muted'>Wird geladen…</td></tr>";

            var monthStr = String(M).padStart(2, "0");
            var periodDate = Y + "-" + monthStr + "-01T00:00:00";
            var url = API + "/api/Lohn/calc";
            var payload = { employeeId: EMP, period: periodDate };

            console.log("[lohne-details] POST calc", url, payload);

            fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            })
                .then(res => res.json().then(j => ({ ok: res.ok, json: j })))
                .then(r => {
                    console.log("[lohne-details] calc response", r);
                    if (!r.ok || !r.json || !r.json.success || !r.json.data) {
                        tbody.innerHTML =
                            "<tr><td colspan='5' class='text-center text-muted'>Positionen konnten nicht geladen werden.</td></tr>";
                        if (cardEr) cardEr.textContent = "–";
                        return;
                    }

                    var d = r.json.data;
                    var emp = d.employee || {};
                    var ag = d.employer || {};
                    var net = norm(d.netToPay) || 0;

                    // ---- Employee & Employer toplamları ELLE topla ----
                    var ahvEmp = norm(emp.ahv_IV_EO) || 0;
                    var alvEmp = norm(emp.alv) || 0;
                    var nbuEmp = norm(emp.uvg_NBU) || 0;
                    var bvgEmp = norm(emp.bvg) || 0;
                    var qstEmp = norm(emp.withholdingTax) || 0;
                    var otherEmp = norm(emp.other) || 0;
                    var empTotal = ahvEmp + alvEmp + nbuEmp + bvgEmp + qstEmp + otherEmp;

                    var ahvEr = norm(ag.ahv_IV_EO) || 0;
                    var alvEr = norm(ag.alv) || 0;
                    var buEr = norm(ag.uvg_NBU) || 0; // BU jetzt hier
                    var bvgEr = norm(ag.bvg) || 0;
                    var fakEr = norm(ag.other) || 0;
                    var agTotal = ahvEr + alvEr + buEr + bvgEr + fakEr;

                    var gross = net + empTotal;

                    // ---- Kartlar ----
                    if (cardGross) cardGross.textContent = fmtNum(gross) + " CHF";
                    if (cardEmp) cardEmp.textContent = fmtNum(empTotal) + " CHF";
                    if (cardEr) cardEr.textContent = fmtNum(agTotal) + " CHF";
                    if (cardNet) cardNet.textContent = fmtNum(net) + " CHF";

                    // ---- Tablo ----
                    tbody.innerHTML = "";

                    // BRUTTOLOHN
                    appendRow(tbody, [
                        "<strong>BRUTTOLohn</strong>",
                        "",
                        "",
                        "",
                        "<strong>" + fmtChf(gross, "earning") + "</strong>"
                    ], true);

                    // Yardımcı: % = tutar / Brutto
                    function pct(amount) {
                        if (!gross || gross === 0) return null;
                        return (amount / gross) * 100;
                    }

                    // --- AN-Abzüge satırları ---
                    appendRow(tbody, [
                        "AHV/IV/EO (Arbeitnehmer)",
                        fmtNum(pct(ahvEmp), 3, true),
                        "–",
                        "–",
                        fmtChf(ahvEmp, "deduction")
                    ], false);

                    appendRow(tbody, [
                        "ALV (Arbeitnehmer)",
                        fmtNum(pct(alvEmp), 3, true),
                        "–",
                        "–",
                        fmtChf(alvEmp, "deduction")
                    ], false);

                    appendRow(tbody, [
                        "Nichtberufsunfall (AN)",
                        fmtNum(pct(nbuEmp), 3, true),
                        "–",
                        "–",
                        fmtChf(nbuEmp, "deduction")
                    ], false);

                    appendRow(tbody, [
                        "BVG (Arbeitnehmer)",
                        fmtNum(pct(bvgEmp), 3, true),
                        "–",
                        "–",
                        fmtChf(bvgEmp, "deduction")
                    ], false);

                    // Quellensteuer (QST) – her zaman göster (0 bile olsa)
                    appendRow(tbody, [
                        "Quellensteuer (QST)",
                        "",        // tarif % detayı yok, sadece tutar
                        "–",
                        "–",
                        fmtChf(qstEmp, "deduction")
                    ], false);

                    if (otherEmp !== 0) {
                        appendRow(tbody, [
                            "Weitere Abzüge (AN)",
                            "",
                            "–",
                            "–",
                            fmtChf(otherEmp, "deduction")
                        ], false);
                    }

                    appendRow(tbody, [
                        "<strong>TOTAL ABZÜGE</strong>",
                        "",
                        "",
                        "",
                        "<strong>" + fmtChf(empTotal, "deduction") + "</strong>"
                    ], true);

                    // --- AG-Kosten ---
                    appendRow(tbody, [
                        "<strong>AG-KOSTEN</strong>",
                        "",
                        "",
                        "",
                        ""
                    ], true);

                    appendRow(tbody, [
                        "AHV/IV/EO (Arbeitgeber)",
                        fmtNum(pct(ahvEr), 3, true),
                        "–",
                        "–",
                        fmtChf(ahvEr, "employer")
                    ], false);

                    appendRow(tbody, [
                        "ALV (Arbeitgeber)",
                        fmtNum(pct(alvEr), 3, true),
                        "–",
                        "–",
                        fmtChf(alvEr, "employer")
                    ], false);

                    appendRow(tbody, [
                        "Berufsunfall (AG)",
                        fmtNum(pct(buEr), 3, true),
                        "–",
                        "–",
                        fmtChf(buEr, "employer")
                    ], false);

                    appendRow(tbody, [
                        "BVG (Arbeitgeber)",
                        fmtNum(pct(bvgEr), 3, true),
                        "–",
                        "–",
                        fmtChf(bvgEr, "employer")
                    ], false);

                    appendRow(tbody, [
                        "Familienausgleichskasse (FAK)",
                        "",    // genelde Brutto üzerinden; istersen pct(fakEr) ekleyebilirsin
                        "–",
                        "–",
                        fmtChf(fakEr, "employer")
                    ], false);

                    appendRow(tbody, [
                        "<strong>TOTAL AG-KOSTEN</strong>",
                        "",
                        "",
                        "",
                        "<strong>" + fmtChf(agTotal, "employer") + "</strong>"
                    ], true);

                    // --- NETTO ---
                    appendRow(tbody, [
                        "<strong>NETTOLOHN</strong>",
                        "",
                        "",
                        "",
                        "<strong>" + fmtChf(net, "netto") + "</strong>"
                    ], true);
                })
                .catch(err => {
                    console.error("[lohne-details] calc error", err);
                    if (cardEr) cardEr.textContent = "–";
                    tbody.innerHTML =
                        "<tr><td colspan='5' class='text-center text-muted'>Positionen konnten nicht geladen werden.</td></tr>";
                });
        }

        // başlangıçta çağır
        loadWorkedDays();
        loadSlip();
    });
})();
