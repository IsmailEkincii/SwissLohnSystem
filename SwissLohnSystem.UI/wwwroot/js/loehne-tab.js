// wwwroot/js/loehne-tab.js
(function () {
    // ==========================
    // 🔧 Config & Helper Functions
    // ==========================
    window.__LOHN_DEBUG__ = true; // Konsolda log görmek için açık bırak (istersen false yap)
    function dbg() {
        if (window.__LOHN_DEBUG__) console.log.apply(console, arguments);
    }

    function getApiBase() {
        const b = (window.API_BASE || '').trim().replace(/\/+$/, '');
        dbg('[loehne] API_BASE =', b);
        return b;
    }

    function getCompanyId() {
        const cid = Number(window.COMPANY_ID || 0);
        dbg('[loehne] COMPANY_ID =', cid);
        return cid > 0 ? cid : null;
    }

    async function safeJson(res) {
        const status = res.status;
        if (!res.ok) {
            const text = await res.text();
            return { ok: false, status, message: text || res.statusText };
        }
        try {
            const json = await res.json();
            return { ok: true, status, data: json };
        } catch {
            return { ok: false, status, message: 'JSON parse error' };
        }
    }

    function unwrapEnvelope(env) {
        if (!env) return { success: false, message: 'No data', data: null };
        return {
            success: env.success ?? env.Success ?? false,
            message: env.message ?? env.Message ?? null,
            data: env.data ?? env.Data ?? null,
        };
    }

    function toCurrency(v) {
        const n = Number(v || 0);
        return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // ==========================
    // 📋 Löhne Listeleme
    // ==========================
    async function loadMonthly() {
        const companyId = getCompanyId();
        if (!companyId) return;

        const api = getApiBase();
        const periodInput = document.getElementById('period');
        if (periodInput && !periodInput.value) {
            const d = new Date();
            periodInput.value = d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0');
        }
        const period = periodInput ? periodInput.value : '';

        let url = `${api}/api/Lohn/by-company/${companyId}/monthly`;
        if (period) url += `?period=${encodeURIComponent(period)}`;
        dbg('[loehne] GET', url);

        try {
            const res = await fetch(url);
            const parsed = await safeJson(res);
            if (!parsed.ok) return renderRows([]);

            const env = unwrapEnvelope(parsed.data);
            if (!env.success) return renderRows([]);

            renderRows(Array.isArray(env.data) ? env.data : []);
        } catch (err) {
            console.error('[loehne] loadMonthly error', err);
            renderRows([]);
        }
    }

    function renderRows(rows) {
        const tbody = document.querySelector('#tblLohnMonthly tbody');
        if (!tbody) return;
        tbody.innerHTML = '';

        if (!rows || rows.length === 0) {
            tbody.innerHTML =
                '<tr><td colspan="6" class="text-center text-muted p-4">Keine Löhne gefunden.</td></tr>';
            return;
        }

        rows.forEach((r) => {
            const id = r.employeeId ?? r.EmployeeId;
            const name =
                r.employeeName ??
                r.EmployeeName ??
                `${r.firstName ?? r.FirstName ?? ''} ${r.lastName ?? r.LastName ?? ''}`.trim();
            const month = r.month ?? r.Month;
            const year = r.year ?? r.Year;
            const brutto = r.bruttoSalary ?? r.BruttoSalary ?? 0;
            const netto = r.netSalary ?? r.NetSalary ?? 0;
            const status = r.statusText ?? r.StatusText ?? '—';

            const tr = document.createElement('tr');
            tr.innerHTML = `
        <td>${name || '-'}</td>
        <td>${month}.${year}</td>
        <td>${toCurrency(brutto)}</td>
        <td><strong>${toCurrency(netto)}</strong></td>
        <td>${status}</td>
        <td>
          <a class="btn btn-sm btn-outline-info" href="/Employees/Details/${id}">Details</a>
        </td>`;
            tbody.appendChild(tr);
        });
    }

    // ==========================
    // 👥 Mitarbeiter Dropdown
    // ==========================
    async function loadEmployeesIntoSelect() {
        const companyId = getCompanyId();
        const sel = document.getElementById('employeeId');
        if (!sel || !companyId) return;

        const api = getApiBase();
        const url = `${api}/api/Company/${companyId}/Employees`;
        dbg('[loehne] GET employees', url);

        sel.innerHTML = '<option value="">-- bitte wählen --</option>';

        try {
            const res = await fetch(url);
            const parsed = await safeJson(res);
            if (!parsed.ok) return disableCalc(sel, true);

            const env = unwrapEnvelope(parsed.data);
            if (!env.success) return disableCalc(sel, true);

            const list = Array.isArray(env.data) ? env.data : [];
            if (list.length === 0) {
                disableCalc(sel, true);
                if (window.toastr) toastr.info('Keine Mitarbeiter gefunden.');
                return;
            }

            list.forEach((e) => {
                const id = e.id ?? e.Id;
                const name = `${e.firstName ?? e.FirstName ?? ''} ${e.lastName ?? e.LastName ?? ''}`.trim();
                const opt = document.createElement('option');
                opt.value = id;
                opt.textContent = name || `#${id}`;
                sel.appendChild(opt);
            });

            disableCalc(sel, false);
        } catch (err) {
            console.error('[loehne] loadEmployeesIntoSelect error', err);
            disableCalc(sel, true);
        }
    }

    function disableCalc(selectEl, disabled) {
        selectEl.disabled = disabled;
        const btn = document.getElementById('btnDoCalc');
        if (btn) btn.disabled = disabled;
    }

    // ==========================
    // 🧮 Lohn Berechnen
    // ==========================
    async function doCalc() {
        const sel = document.getElementById('employeeId');
        const monthInput = document.getElementById('calcMonth');
        const employeeId = Number(sel?.value || 0);
        const ym = monthInput?.value || '';

        if (!employeeId || !/^\d{4}-\d{2}$/.test(ym)) {
            if (window.toastr) toastr.error('Bitte Mitarbeiter und Monat auswählen.');
            return;
        }

        const [year, month] = ym.split('-').map(Number);
        const api = getApiBase();
        const url = `${api}/api/Lohn/calc`;
        dbg('[loehne] POST calc', url, { employeeId, year, month });

        try {
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ employeeId, year, month }),
            });
            const parsed = await safeJson(res);
            if (!parsed.ok) return toastr.error(parsed.message || 'Fehler bei Berechnung.');

            const env = unwrapEnvelope(parsed.data);
            if (env.success) {
                toastr.success(env.message || 'Lohn berechnet.');
                if (typeof $ !== 'undefined' && $('#calcModal').modal) $('#calcModal').modal('hide');
                await loadMonthly();
            } else {
                toastr.error(env.message || 'Berechnung fehlgeschlagen.');
            }
        } catch (err) {
            console.error('[loehne] doCalc error', err);
            toastr.error('Serverfehler.');
        }
    }

    // ==========================
    // ⚡ Init Events
    // ==========================
    document.addEventListener('DOMContentLoaded', function () {
        const btnLoad = document.getElementById('btnLoadLohns');
        const openCalc = document.getElementById('btnOpenCalcModal');
        const btnDoCalc = document.getElementById('btnDoCalc');

        if (btnLoad) btnLoad.addEventListener('click', (e) => { e.preventDefault(); loadMonthly(); });
        if (openCalc)
            openCalc.addEventListener('click', () => {
                loadEmployeesIntoSelect();
                const m = document.getElementById('calcMonth');
                if (m && !m.value) {
                    const d = new Date();
                    m.value = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
                }
            });
        if (btnDoCalc) btnDoCalc.addEventListener('click', (e) => { e.preventDefault(); doCalc(); });

        loadMonthly();
    });
})();
