// wwwroot/js/qst-codes.js
(function () {
    "use strict";

    function log(...args) { console.log("[qst-codes]", ...args); }
    function warn(...args) { console.warn("[qst-codes]", ...args); }
    function err(...args) { console.error("[qst-codes]", ...args); }

    function $(id) { return document.getElementById(id); }

    function getApiBaseUrl() {
        const raw = (window.API_BASE_URL || "").toString().trim();
        return raw.replace(/\/+$/, ""); // trim trailing slash
    }

    function getCompanyId() {
        const v = window.COMPANY_ID;
        const n = Number(v);
        return Number.isFinite(n) ? n : 0;
    }

    function setDisabled(selectEl, disabled) {
        if (!selectEl) return;
        selectEl.disabled = !!disabled;
        if (disabled) selectEl.value = "";
    }

    function showToastrError(message) {
        if (window.toastr && typeof window.toastr.error === "function") {
            window.toastr.error(message);
        } else {
            // fallback
            alert(message);
        }
    }

    function parseJsonPayload(json) {
        // Accept:
        // 1) ["A0","A1"]
        // 2) { success:true, data:["A0","A1"], message:null }
        // 3) { data:["A0","A1"] }  (some implementations)
        if (Array.isArray(json)) return json;

        if (json && typeof json === "object") {
            const data = json.data ?? json.Data;
            if (Array.isArray(data)) return data;
            // sometimes data could be { items: [] }
            if (data && Array.isArray(data.items)) return data.items;
        }

        return [];
    }

    async function loadQstCodes({ apiBaseUrl, companyId, canton, permitType }) {
        const url =
            `${apiBaseUrl}/api/lookups/qst-codes` +
            `?companyId=${encodeURIComponent(companyId)}` +
            `&canton=${encodeURIComponent((canton || "ZH").toUpperCase())}` +
            `&permitType=${encodeURIComponent((permitType || "B").toUpperCase())}`;

        log("GET", url);

        const res = await fetch(url, {
            headers: { Accept: "application/json" }
        });

        if (!res.ok) {
            throw new Error(`HTTP ${res.status}`);
        }

        const json = await res.json();
        return parseJsonPayload(json);
    }

    function fillSelect(selectEl, codes, keepValue) {
        if (!selectEl) return;

        // Reset
        selectEl.innerHTML = "";
        selectEl.appendChild(new Option("-- bitte wählen --", ""));

        (codes || []).forEach((code) => {
            if (!code) return;
            selectEl.appendChild(new Option(String(code), String(code)));
        });

        // keep selection if exists
        if (keepValue) {
            const exists = Array.from(selectEl.options).some(o => o.value === keepValue);
            selectEl.value = exists ? keepValue : "";
        } else {
            selectEl.value = "";
        }
    }

    function getCurrentInputs() {
        const cantonSelect = $("Input_Canton");
        const permitSelect = $("Input_PermitType");
        const qstSelect = $("Input_WithholdingTaxCode");
        const applyQst = $("applyQstCheck") || $("Input_ApplyQST");

        return {
            cantonSelect,
            permitSelect,
            qstSelect,
            applyQst
        };
    }

    async function refresh() {
        const apiBaseUrl = getApiBaseUrl();
        const companyId = getCompanyId();

        const { cantonSelect, permitSelect, qstSelect, applyQst } = getCurrentInputs();

        if (!apiBaseUrl || !companyId) {
            warn("Missing API_BASE_URL or COMPANY_ID", { apiBaseUrl, companyId });
            return;
        }
        if (!cantonSelect || !permitSelect || !qstSelect) {
            warn("Missing required elements", { cantonSelect, permitSelect, qstSelect });
            return;
        }

        const qstEnabled = !applyQst || !!applyQst.checked;
        setDisabled(qstSelect, !qstEnabled);

        // QST kapalıysa boş bırakıp çık
        if (!qstEnabled) return;

        const canton = (cantonSelect.value || "ZH").trim().toUpperCase();
        const permitType = (permitSelect.value || "B").trim().toUpperCase();
        const current = qstSelect.value;

        try {
            // Loading placeholder
            qstSelect.innerHTML = "";
            qstSelect.appendChild(new Option("Laden...", ""));
            qstSelect.disabled = true;

            const codes = await loadQstCodes({ apiBaseUrl, companyId, canton, permitType });

            fillSelect(qstSelect, codes, current);
            qstSelect.disabled = false;

            // if still disabled due to applyQst
            setDisabled(qstSelect, !qstEnabled);
        } catch (e) {
            err("Failed to load QST codes", e);
            fillSelect(qstSelect, [], "");
            qstSelect.disabled = false;
            setDisabled(qstSelect, !qstEnabled);

            showToastrError("QST-Codes konnten nicht geladen werden.");
        }
    }

    function bind() {
        const { cantonSelect, permitSelect, applyQst } = getCurrentInputs();

        if (cantonSelect) cantonSelect.addEventListener("change", refresh);
        if (permitSelect) permitSelect.addEventListener("change", refresh);

        if (applyQst) {
            applyQst.addEventListener("change", function () {
                // toggle + reload if enabled
                refresh();
            });
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        log("init");
        bind();
        refresh();
    });
})();
