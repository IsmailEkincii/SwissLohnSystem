// wwwroot/js/lohne-details.js
(function () {
    "use strict";

    function norm(v) {
        if (v === null || v === undefined || v === "") return 0;
        var n = Number(v);
        return isFinite(n) ? n : 0;
    }

    function chf(v) {
        return "CHF " + norm(v).toFixed(2);
    }

    function setText(id, text) {
        var el = document.getElementById(id);
        if (el) el.textContent = text;
    }

    function escapeHtml(s) {
        if (s === null || s === undefined) return "";
        return String(s)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function rowHtml(cols, cls) {
        return (
            "<tr class='" +
            (cls || "") +
            "'>" +
            "<td>" +
            (cols[0] || "") +
            "</td>" +
            "<td class='text-right'>" +
            (cols[1] || "") +
            "</td>" +
            "<td class='text-right'>" +
            (cols[2] || "") +
            "</td>" +
            "<td class='text-right'>" +
            (cols[3] || "") +
            "</td>" +
            "</tr>"
        );
    }

    function formatBasis(basis) {
        if (!basis) return "—";
        return escapeHtml(basis);
    }

    // API rate: 0.053 => ekranda 5.30 %
    function formatRate(rate) {
        if (rate === null || rate === undefined) return "—";
        var r = Number(rate);
        if (!isFinite(r)) return "—";
        var pct = r <= 1 ? r * 100 : r;
        return pct.toFixed(2) + " %";
    }

    function ensureApiBase() {
        return (window.API_BASE_URL || "").toString().replace(/\/+$/, "");
    }

    function ensureLohnId() {
        return Number(window.LOHN_ID || 0);
    }

    async function loadItems() {
        var API = ensureApiBase();
        var id = ensureLohnId();

        var tbody = document.getElementById("ldmSlipBody");
        if (!tbody) {
            console.warn("lohne-details.js: #ldmSlipBody bulunamadı. CSHTML'de <tbody id='ldmSlipBody'> olmalı.");
            return;
        }

        if (!API || !id) {
            tbody.innerHTML = rowHtml(["Keine Daten.", "", "", ""], "");
            return;
        }

        var url = API + "/api/Lohn/" + id + "/items";
        tbody.innerHTML = rowHtml(["Wird geladen…", "", "", ""], "text-muted");

        try {
            var res = await fetch(url, { method: "GET" });
            var json = await res.json().catch(() => null);

            if (!res.ok || !json || json.success !== true || !Array.isArray(json.data)) {
                var msg = (json && json.message) ? json.message : "Positionen konnten nicht geladen werden.";
                tbody.innerHTML = rowHtml([escapeHtml(msg), "", "", ""], "text-muted");
                return;
            }

            var items = json.data || [];

            // gruplar
            var emp = items.filter(function (x) {
                return (x.side || "").toLowerCase() === "employee" && (x.type || "").toLowerCase() === "deduction";
            });

            var ag = items.filter(function (x) {
                return (x.side || "").toLowerCase() === "employer" && (x.type || "").toLowerCase() === "contribution";
            });

            var empTotal = emp.reduce(function (s, i) { return s + norm(i.amount); }, 0);
            var agTotal = ag.reduce(function (s, i) { return s + norm(i.amount); }, 0);

            // karttaki AG total (senin cshtml’de id varsa)
            setText("cardErTotal", chf(agTotal));

            // tablo bas
            tbody.innerHTML = "";

            // AN Header
            tbody.innerHTML += "<tr class='table-secondary'><td colspan='4'><strong>Abzüge Arbeitnehmer (AN)</strong></td></tr>";
            emp.forEach(function (i) {
                tbody.innerHTML += rowHtml(
                    [
                        escapeHtml(i.title || i.code || ""),
                        formatBasis(i.basis),
                        formatRate(i.rate),
                        norm(i.amount).toFixed(2)
                    ],
                    ""
                );
            });
            tbody.innerHTML += rowHtml(
                ["<strong>Total AN</strong>", "", "", "<strong>" + norm(empTotal).toFixed(2) + "</strong>"],
                "font-weight-bold"
            );

            // AG Header
            tbody.innerHTML += "<tr class='table-secondary'><td colspan='4'><strong>Beiträge Arbeitgeber (AG)</strong></td></tr>";
            ag.forEach(function (i) {
                tbody.innerHTML += rowHtml(
                    [
                        escapeHtml(i.title || i.code || ""),
                        formatBasis(i.basis),
                        formatRate(i.rate),
                        norm(i.amount).toFixed(2)
                    ],
                    ""
                );
            });
            tbody.innerHTML += rowHtml(
                ["<strong>Total AG</strong>", "", "", "<strong>" + norm(agTotal).toFixed(2) + "</strong>"],
                "font-weight-bold"
            );
        } catch (e) {
            tbody.innerHTML = rowHtml(["Positionen konnten nicht geladen werden.", "", "", ""], "text-muted");
        }
    }

    async function finalizeLohn() {
        var btn = document.getElementById("btnFinalize");
        if (!btn) return;

        btn.addEventListener("click", async function () {
            if (!confirm("Diese Lohnabrechnung wirklich finalisieren?")) return;

            var base = ensureApiBase();
            var id = ensureLohnId();

            try {
                btn.disabled = true;

                var res = await fetch(base + "/api/Lohn/" + id + "/finalize", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" }
                });

                var json = await res.json().catch(() => null);

                // ✅ senin API formatın: success/message
                if (!res.ok || !json || json.success !== true) {
                    var msg = (json && json.message) ? json.message : "Finalisieren fehlgeschlagen.";
                    if (window.toastr) toastr.error(msg);
                    else alert(msg);
                    btn.disabled = false;
                    return;
                }

                var okMsg = json.message || "Finalisiert.";
                if (window.toastr) toastr.success(okMsg);
                else alert(okMsg);

                window.location.reload();
            } catch (e) {
                if (window.toastr) toastr.error("Netzwerkfehler.");
                else alert("Netzwerkfehler.");
                btn.disabled = false;
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        loadItems();
        finalizeLohn();
    });
})();
