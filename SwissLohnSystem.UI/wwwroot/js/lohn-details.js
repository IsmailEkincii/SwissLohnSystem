// wwwroot/js/lohn-details.js
(function () {
    console.log("[lohn-details] script loaded");

    const API_BASE = (window.API_BASE_URL || "").replace(/\/+$/, "");
    const EMPLOYEE_ID = window.LOHN_EMPLOYEE_ID || 0;
    const LOHN_MONTH = window.LOHN_MONTH || 0;
    const LOHN_YEAR = window.LOHN_YEAR || 0;

    console.log("[lohn-details] API_BASE =", API_BASE);
    console.log("[lohn-details] EMPLOYEE_ID =", EMPLOYEE_ID, "MONTH =", LOHN_MONTH, "YEAR =", LOHN_YEAR);

    if (!API_BASE) {
        console.warn("[lohn-details] API_BASE_URL fehlt, breche ab.");
        return;
    }

    function api(path) {
        return API_BASE + path;
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

    const btnFinalize = document.getElementById("btnFinalize");
    const badgeStatus = document.getElementById("badgeStatus");

    if (!btnFinalize) {
        console.log("[lohn-details] kein btnFinalize gefunden – nichts zu tun.");
        return;
    }

    btnFinalize.addEventListener("click", async function () {
        if (!EMPLOYEE_ID || !LOHN_MONTH || !LOHN_YEAR) {
            showToast("error", "Fehlende Lohn-Information für die Finalisierung.");
            return;
        }

        if (!confirm("Diese Lohnabrechnung wirklich finalisieren?")) {
            return;
        }

        const payload = {
            employeeId: EMPLOYEE_ID,
            month: LOHN_MONTH,
            year: LOHN_YEAR
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

            // UI güncelle
            btnFinalize.classList.add("d-none");
            if (badgeStatus) {
                badgeStatus.textContent = "Final";
                badgeStatus.className = "badge badge-success";
            }

        } catch (err) {
            console.error("[lohn-details] finalize exception", err);
            showToast("error", "Finalisierung fehlgeschlagen (Netzwerkfehler).");
        }
    });
})();
